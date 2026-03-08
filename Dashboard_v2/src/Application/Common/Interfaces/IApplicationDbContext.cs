using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<Resource> Resources { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<ResourceGrant> ResourceGrants { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<PublicationType> PublicationTypes { get; }
    DbSet<Publication> Publications { get; }
    DbSet<Journal> Journals { get; }
    DbSet<IndexedPublication> IndexedPublications { get; }
    DbSet<SystemGrant> SystemGrants { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
