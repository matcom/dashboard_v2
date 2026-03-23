using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;

namespace Dashboard_v2.Application.Auth.Commands.Login;

public record LoginCommand : IRequest<(Result Result, LoginResponse? Response)>
{
    public string Email { get; init; } = default!;
    public string Password { get; init; } = default!;
    public string? SelectedRole { get; init; }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, (Result Result, LoginResponse? Response)>
{
    private readonly IIdentityService _identityService;

    public LoginCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<(Result Result, LoginResponse? Response)> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.LoginAsync(request.Email, request.Password, request.SelectedRole);
    }
}
