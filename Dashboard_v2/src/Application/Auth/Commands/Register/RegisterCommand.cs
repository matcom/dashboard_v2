using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Dashboard_v2.Domain.Entities;

namespace Dashboard_v2.Application.Auth.Commands.Register;

/// <summary>
/// Comando MediatR para registrar un nuevo usuario en el sistema.<br/>
/// Contiene todos los campos del perfil. El nuevo usuario queda activo pero sin roles:
/// un Superuser deberá asignárselos después desde <c>/api/Users/{id}/roles</c>.
/// </summary>
public record RegisterCommand : IRequest<Result>
{
    public string UserName { get; init; } = default!;
    public string UserLastName1 { get; init; } = default!;
    public string? UserLastName2 { get; init; }
    public string Email { get; init; } = default!;
    public string Password { get; init; } = default!;
    public DateTime BirthDate { get; init; }
    public bool IsTrained { get; init; }
    public TeachingCategory TeachingCategory { get; init; } = TeachingCategory.None;
    public ScientificCategory ScientificCategory { get; init; } = ScientificCategory.None;
    public InvestigationCategory InvestigationCategory { get; init; } = InvestigationCategory.None;
}

/// <summary>
/// Manejador del comando Register. Mapea los campos recibidos y delega
/// la creación en <see cref="IIdentityService.CreateUserAsync"/>.
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result>
{
    private readonly IIdentityService _identityService;

    public RegisterCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    /// <summary>Llama a CreateUserAsync pasando todos los campos del comando.</summary>
    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var (result, userId) = await _identityService.CreateUserAsync(
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
}
