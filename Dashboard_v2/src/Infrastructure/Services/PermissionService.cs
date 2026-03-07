using System.Text.Json;
using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Domain.Constants;
using Dashboard_v2.Domain.Entities;
using Dashboard_v2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dashboard_v2.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(
        ApplicationDbContext context,
        ILogger<PermissionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ============ Verificación de Permisos ============

    public async Task<bool> HasPermissionAsync(string userId, int resourceId, string permissionName, CancellationToken cancellationToken = default)
    {
        // 1. Verificar si es owner (siempre tiene todos los permisos)
        var resource = await _context.Resources
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == resourceId, cancellationToken);

        if (resource == null)
            return false;

        if (resource.OwnerId == userId)
            return true;

        // 2. Verificar grants específicos del usuario sobre el recurso
        var hasGrant = await _context.ResourceGrants
            .AsNoTracking()
            .Include(g => g.Permission)
            .AnyAsync(g =>
                g.UserId == userId &&
                g.ResourceId == resourceId &&
                g.Permission.Name == permissionName &&
                g.IsActive &&
                (g.ExpiresAt == null || g.ExpiresAt > DateTimeOffset.UtcNow),
                cancellationToken);

        if (hasGrant)
            return true;

        // 3. Verificar permisos de rol sobre el tipo de recurso
        // Obtener los IDs de los roles del usuario directamente
        var roleIds = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync(cancellationToken);

        if (!roleIds.Any())
            return false;

        // Verificar si algún rol tiene el permiso para el tipo de recurso específico o para todos los tipos
        var hasRolePermission = await _context.RolePermissions
            .AsNoTracking()
            .Include(rp => rp.Permission)
            .AnyAsync(rp =>
                roleIds.Contains(rp.RoleId) &&
                rp.Permission.Name == permissionName &&
                rp.IsActive &&
                (rp.ResourceType == null || rp.ResourceType == resource.Type),
                cancellationToken);

        return hasRolePermission;
    }

    public async Task<bool> CanAccessFieldAsync(string userId, int resourceId, string fieldName, CancellationToken cancellationToken = default)
    {
        // Si es owner, tiene acceso a todos los campos
        if (await IsOwnerAsync(userId, resourceId, cancellationToken))
            return true;

        // Obtener todos los grants activos del usuario sobre el recurso
        var grants = await _context.ResourceGrants
            .AsNoTracking()
            .Where(g =>
                g.UserId == userId &&
                g.ResourceId == resourceId &&
                g.IsActive &&
                (g.ExpiresAt == null || g.ExpiresAt > DateTime.UtcNow))
            .ToListAsync(cancellationToken);

        if (!grants.Any())
            return false;

        // Si algún grant no tiene restricción de campos, tiene acceso total
        if (grants.Any(g => g.FieldsAllowed == null))
            return true;

        // Verificar si el campo está en alguno de los grants
        foreach (var grant in grants)
        {
            if (grant.FieldsAllowed != null)
            {
                var allowedFields = JsonSerializer.Deserialize<string[]>(grant.FieldsAllowed);
                if (allowedFields != null && allowedFields.Contains(fieldName, StringComparer.OrdinalIgnoreCase))
                    return true;
            }
        }

        return false;
    }

    public async Task<string[]?> GetAllowedFieldsAsync(string userId, int resourceId, CancellationToken cancellationToken = default)
    {
        // Si es owner, tiene acceso a todos los campos
        if (await IsOwnerAsync(userId, resourceId, cancellationToken))
            return null; // null significa todos los campos

        var grants = await _context.ResourceGrants
            .AsNoTracking()
            .Where(g =>
                g.UserId == userId &&
                g.ResourceId == resourceId &&
                g.IsActive &&
                (g.ExpiresAt == null || g.ExpiresAt > DateTime.UtcNow))
            .ToListAsync(cancellationToken);

        if (!grants.Any())
            return Array.Empty<string>(); // Sin acceso

        // Si algún grant no tiene restricción, acceso total
        if (grants.Any(g => g.FieldsAllowed == null))
            return null;

        // Combinar todos los campos permitidos de los grants
        var allFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var grant in grants)
        {
            if (grant.FieldsAllowed != null)
            {
                var fields = JsonSerializer.Deserialize<string[]>(grant.FieldsAllowed);
                if (fields != null)
                {
                    foreach (var field in fields)
                        allFields.Add(field);
                }
            }
        }

        return allFields.ToArray();
    }

    public async Task<bool> IsOwnerAsync(string userId, int resourceId, CancellationToken cancellationToken = default)
    {
        return await _context.Resources
            .AsNoTracking()
            .AnyAsync(r => r.Id == resourceId && r.OwnerId == userId, cancellationToken);
    }

    // ============ Gestión de Grants ============

    public async Task<int> GrantPermissionAsync(
        string userId,
        int resourceId,
        string permissionName,
        string grantedBy,
        DateTimeOffset? expiresAt = null,
        string[]? fieldsAllowed = null,
        CancellationToken cancellationToken = default)
    {
        // Verificar que el recurso existe
        var resourceExists = await _context.Resources
            .AnyAsync(r => r.Id == resourceId, cancellationToken);

        if (!resourceExists)
            throw new InvalidOperationException($"Resource {resourceId} does not exist.");

        // Obtener o verificar que el permiso existe
        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Name == permissionName, cancellationToken);

        if (permission == null)
            throw new InvalidOperationException($"Permission '{permissionName}' does not exist.");

        // Verificar si ya existe un grant activo
        var existingGrant = await _context.ResourceGrants
            .FirstOrDefaultAsync(g =>
                g.UserId == userId &&
                g.ResourceId == resourceId &&
                g.PermissionId == permission.Id &&
                g.IsActive,
                cancellationToken);

        if (existingGrant != null)
        {
            // Actualizar el grant existente
            existingGrant.ExpiresAt = expiresAt;
            existingGrant.FieldsAllowed = fieldsAllowed != null ? JsonSerializer.Serialize(fieldsAllowed) : null;
            existingGrant.GrantedBy = grantedBy;
            existingGrant.GrantedAt = DateTimeOffset.UtcNow;
            existingGrant.LastModified = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return existingGrant.Id;
        }

        // Crear nuevo grant
        var grant = new ResourceGrant
        {
            UserId = userId,
            ResourceId = resourceId,
            PermissionId = permission.Id,
            GrantedBy = grantedBy,
            GrantedAt = DateTimeOffset.UtcNow,
            ExpiresAt = expiresAt,
            FieldsAllowed = fieldsAllowed != null ? JsonSerializer.Serialize(fieldsAllowed) : null,
            IsActive = true
        };

        _context.ResourceGrants.Add(grant);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Permission '{Permission}' granted to user {UserId} on resource {ResourceId} by {GrantedBy}",
            permissionName, userId, resourceId, grantedBy);

        return grant.Id;
    }

    public async Task<bool> RevokePermissionAsync(string userId, int resourceId, string permissionName, CancellationToken cancellationToken = default)
    {
        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Name == permissionName, cancellationToken);

        if (permission == null)
            return false;

        var grant = await _context.ResourceGrants
            .FirstOrDefaultAsync(g =>
                g.UserId == userId &&
                g.ResourceId == resourceId &&
                g.PermissionId == permission.Id &&
                g.IsActive,
                cancellationToken);

        if (grant == null)
            return false;

        grant.IsActive = false;
        grant.LastModified = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Permission '{Permission}' revoked from user {UserId} on resource {ResourceId}",
            permissionName, userId, resourceId);

        return true;
    }

    public async Task<int> RevokeAllPermissionsAsync(string userId, int resourceId, CancellationToken cancellationToken = default)
    {
        var grants = await _context.ResourceGrants
            .Where(g =>
                g.UserId == userId &&
                g.ResourceId == resourceId &&
                g.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var grant in grants)
        {
            grant.IsActive = false;
            grant.LastModified = DateTimeOffset.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "All permissions revoked from user {UserId} on resource {ResourceId}. Count: {Count}",
            userId, resourceId, grants.Count);

        return grants.Count;
    }

    public async Task<bool> DeactivateGrantAsync(int grantId, CancellationToken cancellationToken = default)
    {
        var grant = await _context.ResourceGrants
            .FirstOrDefaultAsync(g => g.Id == grantId, cancellationToken);

        if (grant == null)
            return false;

        grant.IsActive = false;
        grant.LastModified = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ============ Consultas ============

    public async Task<List<string>> GetUserPermissionsAsync(string userId, int resourceId, CancellationToken cancellationToken = default)
    {
        // Si es owner, tiene todos los permisos
        if (await IsOwnerAsync(userId, resourceId, cancellationToken))
        {
            return await _context.Permissions
                .AsNoTracking()
                .Select(p => p.Name)
                .ToListAsync(cancellationToken);
        }

        // Obtener el recurso para conocer su tipo
        var resource = await _context.Resources
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == resourceId, cancellationToken);

        if (resource == null)
            return new List<string>();

        var allPermissions = new HashSet<string>();

        // 1. Obtener permisos de grants activos directos
        var grantPermissions = await _context.ResourceGrants
            .AsNoTracking()
            .Include(g => g.Permission)
            .Where(g =>
                g.UserId == userId &&
                g.ResourceId == resourceId &&
                g.IsActive &&
                (g.ExpiresAt == null || g.ExpiresAt > DateTimeOffset.UtcNow))
            .Select(g => g.Permission.Name)
            .ToListAsync(cancellationToken);

        foreach (var perm in grantPermissions)
            allPermissions.Add(perm);

        // 2. Obtener permisos heredados de roles
        var roleIds = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync(cancellationToken);

        if (roleIds.Any())
        {
            var rolePermissions = await _context.RolePermissions
                .AsNoTracking()
                .Include(rp => rp.Permission)
                .Where(rp =>
                    roleIds.Contains(rp.RoleId) &&
                    rp.IsActive &&
                    (rp.ResourceType == null || rp.ResourceType == resource.Type))
                .Select(rp => rp.Permission.Name)
                .ToListAsync(cancellationToken);

            foreach (var perm in rolePermissions)
                allPermissions.Add(perm);
        }

        return allPermissions.ToList();
    }

    public async Task<List<int>> GetUserResourcesAsync(string userId, string? permissionName = null, CancellationToken cancellationToken = default)
    {
        var query = _context.ResourceGrants
            .AsNoTracking()
            .Include(g => g.Permission)
            .Where(g =>
                g.UserId == userId &&
                g.IsActive &&
                (g.ExpiresAt == null || g.ExpiresAt > DateTimeOffset.UtcNow));

        if (!string.IsNullOrEmpty(permissionName))
        {
            query = query.Where(g => g.Permission.Name == permissionName);
        }

        var grantedResources = await query
            .Select(g => g.ResourceId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Agregar recursos propios
        var ownedResources = await _context.Resources
            .AsNoTracking()
            .Where(r => r.OwnerId == userId)
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        return grantedResources.Union(ownedResources).ToList();
    }

    public async Task<List<string>> GetResourceUsersAsync(int resourceId, string? permissionName = null, CancellationToken cancellationToken = default)
    {
        var query = _context.ResourceGrants
            .AsNoTracking()
            .Include(g => g.Permission)
            .Where(g =>
                g.ResourceId == resourceId &&
                g.IsActive &&
                (g.ExpiresAt == null || g.ExpiresAt > DateTimeOffset.UtcNow));

        if (!string.IsNullOrEmpty(permissionName))
        {
            query = query.Where(g => g.Permission.Name == permissionName);
        }

        var users = await query
            .Select(g => g.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Agregar owner
        var owner = await _context.Resources
            .AsNoTracking()
            .Where(r => r.Id == resourceId)
            .Select(r => r.OwnerId)
            .FirstOrDefaultAsync(cancellationToken);

        if (owner != null && !users.Contains(owner))
        {
            users.Add(owner);
        }

        return users;
    }

    // ============ Limpieza ============

    public async Task<int> CleanupExpiredGrantsAsync(CancellationToken cancellationToken = default)
    {
        var expiredGrants = await _context.ResourceGrants
            .Where(g => g.ExpiresAt != null && g.ExpiresAt < DateTimeOffset.UtcNow && g.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var grant in expiredGrants)
        {
            grant.IsActive = false;
            grant.LastModified = DateTimeOffset.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cleaned up {Count} expired grants", expiredGrants.Count);

        return expiredGrants.Count;
    }

    // ============ Permisos de Sistema ============

    public async Task<bool> HasSystemPermissionAsync(string userId, string permission, CancellationToken cancellationToken = default)
    {
        // Los usuarios con rol Administrator tienen acceso completo sin necesitar un grant explícito
        var isAdmin = await _context.UserRoles
            .AsNoTracking()
            .Include(ur => ur.Role)
            .AnyAsync(ur => ur.UserId == userId && ur.Role.Name == Roles.Administrator, cancellationToken);

        if (isAdmin) return true;

        // system.all concede acceso a cualquier permiso
        var hasAll = await _context.SystemGrants
            .AsNoTracking()
            .AnyAsync(g =>
                g.UserId == userId &&
                g.Permission == SystemPermissions.All &&
                g.IsActive &&
                (g.ExpiresAt == null || g.ExpiresAt > DateTimeOffset.UtcNow),
                cancellationToken);

        if (hasAll) return true;

        // Permiso específico
        return await _context.SystemGrants
            .AsNoTracking()
            .AnyAsync(g =>
                g.UserId == userId &&
                g.Permission == permission &&
                g.IsActive &&
                (g.ExpiresAt == null || g.ExpiresAt > DateTimeOffset.UtcNow),
                cancellationToken);
    }

    public async Task<int> GrantSystemPermissionAsync(
        string userId,
        string permission,
        string grantedBy,
        DateTimeOffset? expiresAt = null,
        CancellationToken cancellationToken = default)
    {
        // Si ya existe un grant activo para ese permiso, actualizar
        var existing = await _context.SystemGrants
            .FirstOrDefaultAsync(g =>
                g.UserId == userId &&
                g.Permission == permission &&
                g.IsActive,
                cancellationToken);

        if (existing != null)
        {
            existing.ExpiresAt = expiresAt;
            existing.GrantedBy = grantedBy;
            existing.GrantedAt = DateTimeOffset.UtcNow;
            existing.LastModified = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return existing.Id;
        }

        var grant = new SystemGrant
        {
            UserId    = userId,
            Permission = permission,
            GrantedBy = grantedBy,
            GrantedAt = DateTimeOffset.UtcNow,
            ExpiresAt = expiresAt,
            IsActive  = true
        };

        _context.SystemGrants.Add(grant);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "System permission '{Permission}' granted to user {UserId} by {GrantedBy}",
            permission, userId, grantedBy);

        return grant.Id;
    }

    public async Task<bool> RevokeSystemGrantAsync(int grantId, CancellationToken cancellationToken = default)
    {
        var grant = await _context.SystemGrants
            .FirstOrDefaultAsync(g => g.Id == grantId, cancellationToken);

        if (grant == null) return false;

        grant.IsActive = false;
        grant.LastModified = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("System grant {GrantId} revoked", grantId);
        return true;
    }

    public async Task<List<(int GrantId, string Permission, DateTimeOffset? ExpiresAt, DateTimeOffset GrantedAt)>>
        GetUserSystemGrantsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var grants = await _context.SystemGrants
            .AsNoTracking()
            .Where(g => g.UserId == userId && g.IsActive &&
                        (g.ExpiresAt == null || g.ExpiresAt > DateTimeOffset.UtcNow))
            .Select(g => new { g.Id, g.Permission, g.ExpiresAt, g.GrantedAt })
            .ToListAsync(cancellationToken);

        return grants
            .Select(g => (g.Id, g.Permission, g.ExpiresAt, g.GrantedAt))
            .ToList();
    }
}
