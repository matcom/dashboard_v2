using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Dashboard_v2.Domain.Entities;

namespace Dashboard_v2.Application.Universidades.Commands.CreateUniversidad;

public record CreateUniversidadCommand : IRequest<(Result Result, string? Id)>
{
    public string Nombre { get; init; } = default!;
}

public class CreateUniversidadCommandHandler : IRequestHandler<CreateUniversidadCommand, (Result Result, string? Id)>
{
    private readonly IApplicationDbContext _context;

    public CreateUniversidadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(Result Result, string? Id)> Handle(CreateUniversidadCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return (Result.Failure(["El nombre es obligatorio."]), null);

        var universidad = new Universidad { Nombre = request.Nombre.Trim() };
        _context.Universidades.Add(universidad);
        await _context.SaveChangesAsync(cancellationToken);

        return (Result.Success(), universidad.Id);
    }
}
