using Dashboard_v2.Domain.Constants;
using Dashboard_v2.Domain.Entities;
using Dashboard_v2.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
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
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
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
        var administratorRole = new IdentityRole(Roles.Administrator);

        if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
        {
            await _roleManager.CreateAsync(administratorRole);
        }

        // Default users
        var administrator = new ApplicationUser 
        { 
            UserName = "administrator", 
            Email = "administrator@localhost" 
        };

        if (_userManager.Users.All(u => u.UserName != administrator.UserName && u.Email != administrator.Email))
        {
            await _userManager.CreateAsync(administrator, "Administrator1!");
            if (!string.IsNullOrWhiteSpace(administratorRole.Name))
            {
                await _userManager.AddToRolesAsync(administrator, new [] { administratorRole.Name });
            }
        }

        // Default data
        // Seed default permissions
        await SeedPermissionsAsync();
        
        // Seed role permissions
        await SeedRolePermissionsAsync();
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

    private async Task SeedRolePermissionsAsync()
    {
        // Asignar todos los permisos al rol Administrator
        var adminRole = await _roleManager.FindByNameAsync(Roles.Administrator);
        if (adminRole == null)
            return;

        var allPermissions = await _context.Permissions.ToListAsync();
        
        foreach (var permission in allPermissions)
        {
            // Verificar si ya existe el permiso para el rol (sin tipo de recurso específico = aplica a todos)
            var exists = await _context.RolePermissions
                .AnyAsync(rp => 
                    rp.RoleId == adminRole.Id && 
                    rp.PermissionId == permission.Id && 
                    rp.ResourceType == null);

            if (!exists)
            {
                var rolePermission = new RolePermission
                {
                    RoleId = adminRole.Id,
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
