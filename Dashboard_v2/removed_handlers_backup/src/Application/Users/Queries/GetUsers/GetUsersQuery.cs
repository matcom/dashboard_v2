using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Dashboard_v2.Application.Users;

namespace Dashboard_v2.Application.Users.Queries.GetUsers;

/// <summary>Consulta MediatR sin parámetros que retorna todos los usuarios del sistema con sus roles.</summary>
public record GetUsersQuery : IRequest<List<UserWithRolesDto>>;

/// <summary>
/// Obtiene todos los usuarios junto con sus roles, ordenados por nombre.<br/>
/// Solo accesible para Superusers (la autorización se aplica en el endpoint Web).
/// </summary>
public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserWithRolesDto>>
{
    private readonly IUserService _service;

    public GetUsersQueryHandler(IUserService service)
    {
        _service = service;
    }

    public Task<List<UserWithRolesDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        => _service.GetAllAsync(cancellationToken);
}
