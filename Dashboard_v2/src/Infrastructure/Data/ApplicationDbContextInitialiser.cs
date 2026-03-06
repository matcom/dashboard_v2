using Dashboard_v2.Domain.Constants;
using Dashboard_v2.Domain.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dashboard_v2.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;

    public ApplicationDbContextInitialiser(
        ILogger<ApplicationDbContextInitialiser> logger,
        ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            // Apply migrations automatically on startup (Development only)
            // For Production, use manual migration scripts or deployment pipelines
            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Default roles
        var adminRoleName = Roles.Administrator;
        var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == adminRoleName);
        if (adminRole == null)
        {
            adminRole = new Role { Id = Guid.NewGuid().ToString(), Name = adminRoleName };
            _context.Roles.Add(adminRole);
            await _context.SaveChangesAsync();
        }

        // Default users
        const string adminUserName = "administrator";
        const string adminEmail = "administrator@localhost";
        const string adminPassword = "Administrator1!";

        var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == adminUserName);
        if (adminUser == null)
        {
            adminUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = adminUserName,
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();
        }

        // Assign admin role to admin user
        var hasRole = await _context.UserRoles.AnyAsync(ur => ur.UserId == adminUser.Id && ur.RoleId == adminRole.Id);
        if (!hasRole)
        {
            _context.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });
            await _context.SaveChangesAsync();
        }

        // Default data
        // Seed default permissions
        await SeedPermissionsAsync();

        // Seed role permissions
        await SeedRolePermissionsAsync(adminRole.Id);
    }

    private async Task SeedPermissionsAsync()
    {
        // Permisos básicos del sistema
        var defaultPermissions = new[]
        {
            new { Name = "read", Description = "Permite leer/ver el recurso" },
            new { Name = "write", Description = "Permite editar/modificar el recurso" },
            new { Name = "delete", Description = "Permite eliminar el recurso" },
            new { Name = "share", Description = "Permite compartir el recurso con otros usuarios" },
            new { Name = "approve", Description = "Permite aprobar cambios o acciones sobre el recurso" },
            new { Name = "admin", Description = "Permisos administrativos completos sobre el recurso" }
        };

        foreach (var permissionData in defaultPermissions)
        {
            if (!_context.Permissions.Any(p => p.Name == permissionData.Name))
            {
                var permission = new Permission
                {
                    Name = permissionData.Name,
                    Description = permissionData.Description,
                    ResourceType = null // Aplicable a todos los tipos de recursos
                };
                _context.Permissions.Add(permission);
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedRolePermissionsAsync(string adminRoleId)
    {
        // Asignar todos los permisos al rol Administrator
        var allPermissions = await _context.Permissions.ToListAsync();
        
        foreach (var permission in allPermissions)
        {
            // Verificar si ya existe el permiso para el rol (sin tipo de recurso específico = aplica a todos)
            var exists = await _context.RolePermissions
                .AnyAsync(rp => 
                    rp.RoleId == adminRoleId && 
                    rp.PermissionId == permission.Id && 
                    rp.ResourceType == null);

            if (!exists)
            {
                var rolePermission = new RolePermission
                {
                    RoleId = adminRoleId,
                    PermissionId = permission.Id,
                    ResourceType = null, // Aplica a todos los tipos de recursos
                    IsActive = true
                };
                _context.RolePermissions.Add(rolePermission);
            }
        }

        await _context.SaveChangesAsync();
    }
}
