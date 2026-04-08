using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;

namespace Dashboard_v2.Application.GruposDeInvestigacion.Commands.DeleteGrupoDeInvestigacion;

public record DeleteGrupoDeInvestigacionCommand(string Id) : IRequest<Result>;

public class DeleteGrupoDeInvestigacionCommandHandler : IRequestHandler<DeleteGrupoDeInvestigacionCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public DeleteGrupoDeInvestigacionCommandHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DeleteGrupoDeInvestigacionCommand request, CancellationToken cancellationToken)
    {
        var grupo = await _context.GruposDeInvestigacion
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
                return Result.Failure(["No tienes permisos para eliminar este grupo."]);
        }

        _context.GruposDeInvestigacion.Remove(grupo);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
