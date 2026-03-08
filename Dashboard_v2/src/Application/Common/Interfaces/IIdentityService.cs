using Dashboard_v2.Application.Common.Models;

namespace Dashboard_v2.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);

    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string email, string password);

    /// <summary>Autentica al usuario y devuelve un JWT si las credenciales son válidas.</summary>
    Task<(Result Result, string? Token)> LoginAsync(string email, string password);

    Task<(string? UserId, string? UserName, string? Email)> GetUserDetailsAsync(string userId);

    Task<Result> DeleteUserAsync(string userId);
}
