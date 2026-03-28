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

    public Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password)
        => throw new NotSupportedException("Use CreateUserAsync with full profile parameters.");

    public async Task<(Result Result, string UserId)> CreateUserAsync(
        string userName,
        string userLastName,
        string email,
        string password,
        DateTime birthDate,
        bool isTrained,
        TeachingCategory teachingCategory,
        ScientificCategory scientificCategory,
        InvestigationCategory investigationCategory)
    {
        if (await _context.Users.AnyAsync(u => u.Email == email))
            return (Result.Failure(["El email ya está en uso."]), string.Empty);

        if (await _context.Users.AnyAsync(u => u.UserName == userName))
            return (Result.Failure(["El nombre de usuario ya está en uso."]), string.Empty);

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            UserName = userName,
            UserLastName = userLastName,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            BirthDate = DateTime.SpecifyKind(birthDate, DateTimeKind.Utc),
            IsTrained = isTrained,
            TeachingCategory = teachingCategory,
            ScientificCategory = scientificCategory,
            InvestigationCategory = investigationCategory,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(CancellationToken.None);

        return (Result.Success(), user.Id);
    }

    public async Task<(Result Result, LoginResponse? Response)> LoginAsync(string email, string password, string? selectedRole = null)
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

        if (roles.Count == 0)
            return (Result.Failure(["El usuario no tiene roles asignados."]), null);

        string roleToUse;

        if (selectedRole != null)
        {
            if (!roles.Contains(selectedRole))
                return (Result.Failure(["El rol seleccionado no está asignado a este usuario."]), null);
            roleToUse = selectedRole;
        }
        else if (roles.Count == 1)
        {
            roleToUse = roles[0];
        }
        else
        {
            // Múltiples roles: el cliente debe elegir uno
            return (Result.Success(), new LoginResponse
            {
                RequiresRoleSelection = true,
                AvailableRoles = roles
            });
        }

        var token = _jwtService.GenerateToken(user.Id, user.UserName, user.Email, [roleToUse]);

        return (Result.Success(), new LoginResponse { Token = token });
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
