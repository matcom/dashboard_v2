using Dashboard_v2.Application.Common.Interfaces;

namespace Dashboard_v2.Application.Auth.Queries.GetCurrentUser;

public record UserDto
{
    public string Id { get; init; } = default!;
    public string UserName { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string? Role { get; init; }
}

public record GetCurrentUserQuery : IRequest<UserDto?>;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserDto?>
{
    private readonly IUser _currentUser;
    private readonly IIdentityService _identityService;

    public GetCurrentUserQueryHandler(IUser currentUser, IIdentityService identityService)
    {
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<UserDto?> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return null;
        }

        var (userId, userName, email) = await _identityService.GetUserDetailsAsync(_currentUser.Id);

        if (userId == null)
        {
            return null;
        }

        return new UserDto
        {
            Id = userId,
            UserName = userName ?? string.Empty,
            Email = email ?? string.Empty,
            Role = _currentUser.Roles?.FirstOrDefault()
        };
    }
}
