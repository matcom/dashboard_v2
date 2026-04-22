using Dashboard_v2.Application.Common.Interfaces;
using RolesEnum = global::Dashboard_v2.Domain.Enums.Roles;
using Microsoft.EntityFrameworkCore;
using Dashboard_v2.Application.Users;

namespace Dashboard_v2.Application.Users.Queries.GetJefesDeProyecto;

/// <summary>
/// Consulta MediatR que retorna todos los usuarios activos con el rol <c>Jefe_de_Proyecto</c>.
/// Usada por el frontend para poblar el selector de jefe al crear o editar un proyecto.
/// </summary>
public record GetJefesDeProyectoQuery : IRequest<List<JefeDeProyectoDto>>;

/// <summary>
/// Retorna los usuarios activos con rol <c>Jefe_de_Proyecto</c>, ordenados por nombre.
/// La autorización se aplica en el endpoint Web (Superuser y Jefe_de_Proyecto pueden consultar).
/// </summary>
public class GetJefesDeProyectoQueryHandler : IRequestHandler<GetJefesDeProyectoQuery, List<JefeDeProyectoDto>>
{
    private readonly IUserService _service;

    public GetJefesDeProyectoQueryHandler(IUserService service) => _service = service;

    public Task<List<JefeDeProyectoDto>> Handle(GetJefesDeProyectoQuery request, CancellationToken cancellationToken)
        => _service.GetJefesDeProyectoAsync(cancellationToken);
}
