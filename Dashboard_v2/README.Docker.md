# Configuración de Base de Datos con Docker

Este proyecto utiliza PostgreSQL como base de datos. Puedes levantar una instancia local usando Docker.

## Requisitos Previos

- [Docker](https://docs.docker.com/get-docker/) instalado
- [Docker Compose](https://docs.docker.com/compose/install/) instalado

## Comandos de Docker

### Iniciar la base de datos

```bash
docker-compose up -d
```

El flag `-d` ejecuta el contenedor en modo detached (segundo plano).

### Verificar el estado

```bash
docker-compose ps
```

### Ver logs

```bash
docker-compose logs -f postgres
```

### Detener la base de datos

```bash
docker-compose stop
```

### Detener y eliminar contenedores

```bash
docker-compose down
```

### Eliminar también los volúmenes (⚠️ esto borrará todos los datos)

```bash
docker-compose down -v
```

## Configuración

La configuración de conexión se encuentra en `src/Web/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "Dashboard_v2Db": "Server=127.0.0.1;Port=5432;Database=Dashboard_v2Db;Username=admin;Password=password;"
  }
}
```

### Credenciales por defecto

- **Usuario**: `admin`
- **Contraseña**: `password`
- **Base de datos**: `Dashboard_v2Db`
- **Puerto**: `5432`

## Aplicar Migraciones

Después de levantar la base de datos, aplica las migraciones:

```bash
# Aplicar migraciones automáticamente (se hace al ejecutar en Development)
dotnet run --project src/Web

# O manualmente
dotnet ef database update --project src/Infrastructure --startup-project src/Web
```

## Conectarse a la base de datos

### Usando psql (cliente de PostgreSQL)

```bash
docker exec -it dashboard_v2_db psql -U admin -d Dashboard_v2Db
```

### Usando pgAdmin o cualquier cliente GUI

- **Host**: `localhost` o `127.0.0.1`
- **Puerto**: `5432`
- **Usuario**: `admin`
- **Contraseña**: `password`
- **Base de datos**: `Dashboard_v2Db`

## Solución de Problemas

### El puerto 5432 ya está en uso

Si ya tienes PostgreSQL instalado localmente, puedes cambiar el puerto en `docker-compose.yml`:

```yaml
ports:
  - "5433:5432"  # Usa el puerto 5433 en tu máquina local
```

Y actualiza la cadena de conexión en `appsettings.Development.json`:

```json
"Dashboard_v2Db": "Server=127.0.0.1;Port=5433;Database=Dashboard_v2Db;Username=admin;Password=password;"
```

### Resetear la base de datos

```bash
# Detener y eliminar todo (incluyendo volúmenes)
docker-compose down -v

# Volver a levantar
docker-compose up -d

# Aplicar migraciones
dotnet ef database update --project src/Infrastructure --startup-project src/Web
```
