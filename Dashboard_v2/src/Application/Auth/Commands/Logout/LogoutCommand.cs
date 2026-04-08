using Dashboard_v2.Application.Common.Models;

namespace Dashboard_v2.Application.Auth.Commands.Logout;

// JWT es stateless: el logout se maneja en el cliente descartando el token.
// Este comando existe para mantener compatibilidad con el endpoint.
public record LogoutCommand : IRequest<Result>;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    public Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success());
    }
}
