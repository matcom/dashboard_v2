using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;

namespace Dashboard_v2.Application.GruposDeInvestigacion.Commands.UpdateGrupoDeInvestigacion;

public record UpdateGrupoDeInvestigacionCommand : IRequest<Result>
{
    public string Id { get; init; } = default!;
    public string Nombre { get; init; } = default!;
    /// <summary>Cambiar el Área a la que pertenece el grupo.</summary>
    public string AreaId { get; init; } = default!;
    /// <summary>Ids de las Líneas de Investigación que estudia este grupo (N:N). Reemplaza la selección anterior.</summary>
    public IList<string> LineasDeInvestigacionIds { get; init; } = [];
}

public class UpdateGrupoDeInvestigacionCommandHandler : IRequestHandler<UpdateGrupoDeInvestigacionCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public UpdateGrupoDeInvestigacionCommandHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateGrupoDeInvestigacionCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result.Failure(["El nombre es obligatorio."]);

        var grupo = await _context.GruposDeInvestigacion
            .Include(g => g.LineasDeInvestigacion)
            .Include(g => g.Usuarios)
            .FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);

        if (grupo is null)
            return Result.Failure(["Grupo de investigación no encontrado."]);

        // Si no es Superuser, verificar que el usuario actual es miembro del grupo
        var isSuperuser = _currentUser.Roles?.Contains("Superuser") == true;
        if (!isSuperuser)
        {
            var userId = _currentUser.Id;
            if (!grupo.Usuarios.Any(u => u.Id == userId))
                return Result.Failure(["No tienes permisos para editar este grupo."]);
        }

        if (!await _context.Areas.AnyAsync(a => a.Id == request.AreaId, cancellationToken))
            return Result.Failure(["El área indicada no existe."]);

        grupo.Nombre = request.Nombre.Trim();
        grupo.AreaId = request.AreaId;

        var newLineas = await _context.LineasDeInvestigacion
            .Where(l => request.LineasDeInvestigacionIds.Contains(l.Id))
            .ToListAsync(cancellationToken);
        grupo.LineasDeInvestigacion.Clear();
        foreach (var linea in newLineas)
            grupo.LineasDeInvestigacion.Add(linea);

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
