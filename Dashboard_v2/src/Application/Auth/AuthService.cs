using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;

namespace Dashboard_v2.Application.Auth;

/// <summary>
/// Servicio que encapsula la lógica de autenticación sin exponer commands/queries a la capa Web.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly IIdentityService _identityService;
    private readonly IRequestValidationService _validationService;
    private readonly IUser _currentUser;

    public AuthService(
        IIdentityService identityService,
        IRequestValidationService validationService,
        IUser currentUser)
    {
        _identityService = identityService;
        _validationService = validationService;
        _currentUser = currentUser;
    }

    public async Task<Result> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        await _validationService.ValidateAndThrowAsync(request, ct);

        var (result, _) = await _identityService.CreateUserAsync(
            request.UserName,
            request.UserLastName1,
            request.UserLastName2,
            request.Email,
            request.Password,
            request.BirthDate,
            request.IsTrained,
            request.TeachingCategory,
            request.ScientificCategory,
            request.InvestigationCategory);

        return result;
    }

    public async Task<(Result Result, LoginResponse? Response)> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        await _validationService.ValidateAndThrowAsync(request, ct);
        return await _identityService.LoginAsync(request.Email, request.Password, request.SelectedRole);
    }

    public Task<Result> LogoutAsync(CancellationToken ct = default)
    {
        return Task.FromResult(Result.Success());
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.Id))
        {
            return null;
        }

        var (userId, userName, email) = await _identityService.GetUserDetailsAsync(_currentUser.Id);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return new CurrentUserDto
        {
            Id = userId,
            UserName = userName ?? string.Empty,
            Email = email ?? string.Empty,
            Role = _currentUser.Roles?.FirstOrDefault()
        };
    }
}
