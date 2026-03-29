using Dashboard_v2.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Application.Users.Queries.GetUsers;

/// <summary>DTO de respuesta que combina datos del usuario con sus roles como strings (para JSON).</summary>
public record UserWithRolesDto
{
    public string Id { get; init; } = default!;
    public string UserName { get; init; } = default!;
    public string Email { get; init; } = default!;
    public bool IsActive { get; init; }
    public List<string> Roles { get; init; } = [];
}

/// <summary>Consulta MediatR sin parámetros que retorna todos los usuarios del sistema con sus roles.</summary>
public record GetUsersQuery : IRequest<List<UserWithRolesDto>>;

/// <summary>
/// Obtiene todos los usuarios junto con sus roles, ordenados por nombre.<br/>
/// Solo accesible para Superusers (la autorización se aplica en el endpoint Web).
/// </summary>
public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserWithRolesDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Ejecuta la consulta: SELECT users + sus roles (enum → string), ordenado por UserName.</summary>
    public async Task<List<UserWithRolesDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Users
            .AsNoTracking()
            .Select(u => new UserWithRolesDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                IsActive = u.IsActive,
                Roles = u.UserRoles.Select(ur => ur.Role.ToString()).ToList()
            })
            .OrderBy(u => u.UserName)
            .ToListAsync(cancellationToken);
    }
}
