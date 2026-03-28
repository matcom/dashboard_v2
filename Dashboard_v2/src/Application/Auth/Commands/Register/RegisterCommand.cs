using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Dashboard_v2.Domain.Entities;

namespace Dashboard_v2.Application.Auth.Commands.Register;

public record RegisterCommand : IRequest<Result>
{
    public string UserName { get; init; } = default!;
    public string UserLastName { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string Password { get; init; } = default!;
    public DateTime BirthDate { get; init; }
    public bool IsTrained { get; init; }
    public TeachingCategory TeachingCategory { get; init; } = TeachingCategory.None;
    public ScientificCategory ScientificCategory { get; init; } = ScientificCategory.None;
    public InvestigationCategory InvestigationCategory { get; init; } = InvestigationCategory.None;
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result>
{
    private readonly IIdentityService _identityService;

    public RegisterCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var (result, userId) = await _identityService.CreateUserAsync(
            request.UserName,
            request.UserLastName,
            request.Email,
            request.Password,
            request.BirthDate,
            request.IsTrained,
            request.TeachingCategory,
            request.ScientificCategory,
            request.InvestigationCategory);

        return result;
    }
}
