using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using RolesEnum = global::Dashboard_v2.Domain.Enums.Roles;

namespace Dashboard_v2.Application.Users.Commands.AssignRole;

public record AssignRoleCommand : IRequest<Result>
{
    public string UserId { get; init; } = default!;
    public string RoleName { get; init; } = default!;
}

public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, Result>
{
    private readonly IUserService _service;

    public AssignRoleCommandHandler(IUserService service)
    {
        _service = service;
    }

    public Task<Result> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
        => _service.AssignRoleAsync(request.UserId, request.RoleName, cancellationToken);
}
