using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using RolesEnum = global::Dashboard_v2.Domain.Enums.Roles;

namespace Dashboard_v2.Application.Users.Commands.RemoveRole;

public record RemoveRoleCommand : IRequest<Result>
{
    public string UserId { get; init; } = default!;
    public string RoleName { get; init; } = default!;
}

public class RemoveRoleCommandHandler : IRequestHandler<RemoveRoleCommand, Result>
{
    private readonly IUserService _service;

    public RemoveRoleCommandHandler(IUserService service)
    {
        _service = service;
    }

    public Task<Result> Handle(RemoveRoleCommand request, CancellationToken cancellationToken)
        => _service.RemoveRoleAsync(request.UserId, request.RoleName, cancellationToken);
}
