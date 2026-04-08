# Arquitectura de Permisos - Sistema Híbrido

## Estrategia Recomendada

### Nivel 1: Claims de Identity (Permisos Globales)
**Usar para**: Permisos a nivel de aplicación que aplican globalmente

```csharp
// Ejemplos
User.HasClaim("department", "IT")
User.IsInRole("Administrator")
User.HasClaim("can_manage_users", "true")
```

**Ventajas:**
- ✓ Ya integrado en ASP.NET Identity
- ✓ Performance (cacheado en el token/sesión)
- ✓ Simple de implementar
- ✓ Estándar de la industria

**Usar para:**
- Roles básicos (Admin, Manager, User)
- Permisos de módulos (CanAccessReports, CanManageUsers)
- Metadatos del usuario (Department, EmployeeId)

---

### Nivel 2: Sistema Personalizado (Permisos Granulares)
**Usar para**: Permisos específicos de recursos

```sql
-- Permisos por recurso específico
SELECT * FROM grants 
WHERE user_id = ? 
  AND resource_id = ? 
  AND permission_id = ?
  AND (expires_at IS NULL OR expires_at > NOW())
```

**Ventajas:**
- ✓ Control fino por recurso
- ✓ Permisos temporales
- ✓ Delegación dinámica
- ✓ Auditoría detallada
- ✓ Permisos a nivel de campo

**Usar para:**
- Ownership de documentos/proyectos
- Permisos compartidos temporalmente
- Control de acceso a nivel de campo
- Workflow de aprobaciones

---

## Modelo de Datos Recomendado

### Tablas Core (Identity - Ya las tienes)
```
AspNetUsers (Identity)
AspNetRoles (Identity)
AspNetUserRoles (Identity)
AspNetUserClaims (Identity) → Para permisos globales simples
```

### Tablas Custom (Permisos Granulares)
```sql
-- Recursos genéricos del sistema
CREATE TABLE Resources (
    Id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    Type text NOT NULL, -- 'Document', 'Project', 'Report', etc.
    OwnerId text REFERENCES "AspNetUsers"("Id"), -- Integra con Identity
    Name text,
    Metadata jsonb,
    CreatedAt timestamptz DEFAULT now(),
    UpdatedAt timestamptz DEFAULT now()
);

-- Catálogo de permisos disponibles
CREATE TABLE Permissions (
    Id serial PRIMARY KEY,
    Name text UNIQUE NOT NULL, -- 'read', 'write', 'delete', 'approve'
    ResourceType text, -- NULL = global, o específico de tipo
    Description text
);

-- Grants: Asignación de permisos
CREATE TABLE ResourceGrants (
    Id serial PRIMARY KEY,
    UserId text REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE,
    ResourceId uuid REFERENCES Resources(Id) ON DELETE CASCADE,
    PermissionId int REFERENCES Permissions(Id) ON DELETE CASCADE,
    GrantedBy text REFERENCES "AspNetUsers"("Id"),
    GrantedAt timestamptz DEFAULT now(),
    ExpiresAt timestamptz,
    FieldsAllowed jsonb, -- NULL = todos los campos
    Conditions jsonb, -- Para reglas avanzadas
    UNIQUE(UserId, ResourceId, PermissionId)
);

-- Índices para performance
CREATE INDEX idx_grants_user ON ResourceGrants(UserId);
CREATE INDEX idx_grants_resource ON ResourceGrants(ResourceId);
CREATE INDEX idx_grants_expires ON ResourceGrants(ExpiresAt) 
    WHERE ExpiresAt IS NOT NULL;
```

---

## Flujo de Verificación de Permisos

```
┌─────────────────────────────────────────┐
│ 1. Verificar Claims (Nivel Global)     │
│    - ¿Es Admin? → Acceso total          │
│    - ¿Tiene permiso de módulo?          │
└───────────────┬─────────────────────────┘
                │
                ↓ NO pasó
┌─────────────────────────────────────────┐
│ 2. Verificar Ownership                  │
│    - ¿Es dueño del recurso?             │
└───────────────┬─────────────────────────┘
                │
                ↓ NO es dueño
┌─────────────────────────────────────────┐
│ 3. Verificar Grants Específicos         │
│    - ¿Tiene grant vigente?              │
│    - ¿No ha expirado?                   │
│    - ¿Campos permitidos?                │
└─────────────────────────────────────────┘
```

---

## Implementación en Clean Architecture

### Domain Layer
```csharp
// Entities
public class Resource : BaseEntity
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public string OwnerId { get; set; }
    public string Name { get; set; }
    public JsonDocument? Metadata { get; set; }
}

public class Permission : BaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? ResourceType { get; set; }
}

public class ResourceGrant : BaseEntity
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public Guid ResourceId { get; set; }
    public int PermissionId { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public JsonDocument? FieldsAllowed { get; set; }
}
```

### Application Layer
```csharp
// Interface
public interface IPermissionService
{
    // Claims-based (global)
    Task<bool> HasGlobalPermissionAsync(string userId, string permission);
    
    // Resource-based (granular)
    Task<bool> CanAccessResourceAsync(string userId, Guid resourceId, string permission);
    Task<bool> CanAccessFieldAsync(string userId, Guid resourceId, string fieldName);
    
    // Management
    Task GrantPermissionAsync(string userId, Guid resourceId, string permission, 
        DateTime? expiresAt = null, string[]? fieldsAllowed = null);
    Task RevokePermissionAsync(string userId, Guid resourceId, string permission);
}
```

---

## Ejemplo de Uso

### Verificación en Endpoint
```csharp
public async Task<IResult> GetDocument(
    Guid documentId,
    IPermissionService permissionService,
    IUser currentUser)
{
    // 1. Verificar permisos globales (Claims - rápido)
    if (currentUser.Roles?.Contains("Administrator") == true)
    {
        // Admin tiene acceso a todo
        return Results.Ok(fullDocument);
    }
    
    // 2. Verificar permisos granulares (BD - más lento pero flexible)
    var canRead = await permissionService.CanAccessResourceAsync(
        currentUser.Id!, 
        documentId, 
        "read"
    );
    
    if (!canRead)
        return Results.Forbid();
    
    // 3. Aplicar restricciones de campos
    var allowedFields = await permissionService.GetAllowedFieldsAsync(
        currentUser.Id!, 
        documentId
    );
    
    if (allowedFields != null)
    {
        // Filtrar solo los campos permitidos
        return Results.Ok(FilterFields(fullDocument, allowedFields));
    }
    
    return Results.Ok(fullDocument);
}
```

---

## Estrategia de Caché

**Importante**: El sistema custom requiere caché para no impactar performance

```csharp
// Caché distribuido (Redis) para grants
public class CachedPermissionService : IPermissionService
{
    private readonly IPermissionService _inner;
    private readonly IDistributedCache _cache;
    
    public async Task<bool> CanAccessResourceAsync(
        string userId, Guid resourceId, string permission)
    {
        var cacheKey = $"perm:{userId}:{resourceId}:{permission}";
        
        // Intentar desde caché
        var cached = await _cache.GetStringAsync(cacheKey);
        if (cached != null)
            return bool.Parse(cached);
        
        // Si no está en caché, consultar BD
        var result = await _inner.CanAccessResourceAsync(userId, resourceId, permission);
        
        // Cachear por 5 minutos
        await _cache.SetStringAsync(cacheKey, result.ToString(), 
            new DistributedCacheEntryOptions 
            { 
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) 
            });
        
        return result;
    }
}
```

---

## Cuándo Usar Cada Uno

| Escenario | Usar Claims | Usar Sistema Custom |
|-----------|-------------|---------------------|
| "¿Es administrador?" | ✓ | |
| "¿Puede ver reportes?" | ✓ | |
| "¿Puede editar ESTE documento?" | | ✓ |
| "¿Puede aprobar hasta el 15/03?" | | ✓ |
| "¿Puede ver solo campos X, Y?" | | ✓ |
| "¿El usuario pertenece a IT?" | ✓ | |
| "¿Puede delegar permisos?" | ✓ | |
| "¿Tiene permiso compartido temporal?" | | ✓ |

---

## Resumen de Recomendación

**Para tu dashboard:**

1. **Usa Claims (Identity)** para:
   - Roles de usuario (Admin, Manager, User)
   - Permisos de módulos/features
   - Metadatos básicos del usuario

2. **Usa Sistema Custom** para:
   - Permisos sobre documentos/recursos específicos
   - Compartir acceso temporal
   - Control fino de campos
   - Workflow de aprobaciones

3. **Implementa caché** para el sistema custom

4. **Combina ambos** en el flujo de autorización

**Seguridad**: Ambos son igualmente seguros si se implementan correctamente.

**Practicidad**: Híbrido es lo más práctico - usa la herramienta correcta para cada caso.

**Simplicidad**: Si solo necesitas roles básicos → Claims es suficiente. Si necesitas el sistema complejo que mostraste → Sistema custom es necesario.
