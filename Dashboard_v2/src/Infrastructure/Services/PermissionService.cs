using Dashboard_v2.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dashboard_v2.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(IApplicationDbContext context, ILogger<PermissionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> IsOwnerAsync(string userId, int resourceId, CancellationToken cancellationToken = default)
    {
        return await _context.Resources
            .AsNoTracking()
            .AnyAsync(r => r.Id == resourceId && r.OwnerId == userId, cancellationToken);
    }

    public async Task<bool> HasPermissionAsync(string userId, int resourceId, string permissionName, CancellationToken cancellationToken = default)
    {
        return await IsOwnerAsync(userId, resourceId, cancellationToken);
    }

    public async Task<bool> CanAccessFieldAsync(string userId, int resourceId, string fieldName, CancellationToken cancellationToken = default)
    {
        return await IsOwnerAsync(userId, resourceId, cancellationToken);
    }

    public async Task<string[]?> GetAllowedFieldsAsync(string userId, int resourceId, CancellationToken cancellationToken = default)
    {
        if (await IsOwnerAsync(userId, resourceId, cancellationToken))
            return null;

        return Array.Empty<string>();
    }

    public Task<int> GrantPermissionAsync(string userId, int resourceId, string permissionName, string grantedBy, DateTimeOffset? expiresAt = null, string[]? fieldsAllowed = null, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("GrantPermissionAsync: grants are not persisted in the current model.");
        return Task.FromResult(0);
    }

    public Task<bool> RevokePermissionAsync(string userId, int resourceId, string permissionName, CancellationToken cancellationToken = default)
        => Task.FromResult(false);

    public Task<int> RevokeAllPermissionsAsync(string userId, int resourceId, CancellationToken cancellationToken = default)
        => Task.FromResult(0);

    public Task<bool> DeactivateGrantAsync(int grantId, CancellationToken cancellationToken = default)
        => Task.FromResult(false);

    public async Task<List<string>> GetUserPermissionsAsync(string userId, int resourceId, CancellationToken cancellationToken = default)
    {
        if (await IsOwnerAsync(userId, resourceId, cancellationToken))
            return ["read", "write", "delete"];

        return [];
    }

    public async Task<List<int>> GetUserResourcesAsync(string userId, string? permissionName = null, CancellationToken cancellationToken = default)
    {
        return await _context.Resources
            .AsNoTracking()
            .Where(r => r.OwnerId == userId)
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<string>> GetResourceUsersAsync(int resourceId, string? permissionName = null, CancellationToken cancellationToken = default)
    {
        var owner = await _context.Resources
            .AsNoTracking()
            .Where(r => r.Id == resourceId)
            .Select(r => r.OwnerId)
            .FirstOrDefaultAsync(cancellationToken);

        return owner != null ? [owner] : [];
    }

    public Task<int> CleanupExpiredGrantsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(0);
}
