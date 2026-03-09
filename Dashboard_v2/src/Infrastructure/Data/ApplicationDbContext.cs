using System.Reflection;
using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<ResourceGrant> ResourceGrants => Set<ResourceGrant>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<PublicationType> PublicationTypes => Set<PublicationType>();
    public DbSet<Publication> Publications => Set<Publication>();
    public DbSet<Journal> Journals => Set<Journal>();
    public DbSet<IndexedPublication> IndexedPublications => Set<IndexedPublication>();
    public DbSet<SystemGrant> SystemGrants => Set<SystemGrant>();
    public DbSet<RoleSystemPermission> RoleSystemPermissions => Set<RoleSystemPermission>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
