using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;

namespace Dashboard_v2.Application.Universidades.Commands.UpdateUniversidad;

public record UpdateUniversidadCommand : IRequest<Result>
{
    public string Id { get; init; } = default!;
    public string Nombre { get; init; } = default!;
}

public class UpdateUniversidadCommandHandler : IRequestHandler<UpdateUniversidadCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateUniversidadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateUniversidadCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result.Failure(["El nombre es obligatorio."]);

        var universidad = await _context.Universidades
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (universidad is null)
            return Result.Failure(["Universidad no encontrada."]);

        universidad.Nombre = request.Nombre.Trim();
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
