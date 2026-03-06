using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Dashboard_v2.Domain.Constants;
using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Infrastructure.Identity;

public class AuthService : IIdentityService
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public AuthService(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);
        return user?.UserName;
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        return await _context.UserRoles
            .AsNoTracking()
            .Include(ur => ur.Role)
            .AnyAsync(ur => ur.UserId == userId && ur.Role.Name == role);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        // La única política actual requiere el rol Administrator
        if (policyName == Policies.CanPurge)
            return await IsInRoleAsync(userId, Roles.Administrator);

        return false;
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password)
    {
        return await CreateUserAsync(userName, userName, password);
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string email, string password)
    {
        if (await _context.Users.AnyAsync(u => u.Email == email))
            return (Result.Failure(["El email ya está en uso."]), string.Empty);

        if (await _context.Users.AnyAsync(u => u.UserName == userName))
            return (Result.Failure(["El nombre de usuario ya está en uso."]), string.Empty);

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            UserName = userName,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(CancellationToken.None);

        return (Result.Success(), user.Id);
    }

    public async Task<(Result Result, string? Token)> LoginAsync(string email, string password)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || !user.IsActive)
            return (Result.Failure(["Credenciales inválidas."]), null);

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return (Result.Failure(["Credenciales inválidas."]), null);

        var roles = await _context.UserRoles
            .AsNoTracking()
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.Role.Name)
            .ToListAsync();

        var token = _jwtService.GenerateToken(user.Id, user.UserName, user.Email, roles);

        return (Result.Success(), token);
    }

    public async Task<(string? UserId, string? UserName, string? Email)> GetUserDetailsAsync(string userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        return (user?.Id, user?.UserName, user?.Email);
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return Result.Success();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(CancellationToken.None);
        return Result.Success();
    }
}
