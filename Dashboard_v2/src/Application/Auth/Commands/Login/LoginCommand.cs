using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;

namespace Dashboard_v2.Application.Auth.Commands.Login;

/// <summary>
/// Comando MediatR para autenticar a un usuario.<br/>
/// Enviar este comando dispara el flujo: validación de credenciales → verificación de roles → generación de JWT.<br/>
/// El campo <see cref="SelectedRole"/> es opcional: solo se usa cuando el usuario ya eligió un rol
/// en una respuesta previa de tipo <c>requiresRoleSelection</c>.
/// </summary>
public record LoginCommand : IRequest<(Result Result, LoginResponse? Response)>
{
    public string Email { get; init; } = default!;
    public string Password { get; init; } = default!;
    public string? SelectedRole { get; init; }
}

/// <summary>
/// Manejador del comando Login. Delega toda la lógica en <see cref="IIdentityService.LoginAsync"/>.<br/>
/// Este patrón CQRS (Command + Handler) desacopla la capa Web de la capa de Infraestructura:
/// el endpoint no sabe cómo funciona el login, solo lanza el comando.
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, (Result Result, LoginResponse? Response)>
{
    private readonly IIdentityService _identityService;

    public LoginCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    /// <summary>Pasa email, contraseña y rol seleccionado al servicio de identidad.</summary>
    public async Task<(Result Result, LoginResponse? Response)> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.LoginAsync(request.Email, request.Password, request.SelectedRole);
    }
}
