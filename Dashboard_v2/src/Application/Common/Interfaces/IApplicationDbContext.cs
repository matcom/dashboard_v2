using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Resource> Resources { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<ResourceGrant> ResourceGrants { get; }
    DbSet<RolePermission> RolePermissions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
