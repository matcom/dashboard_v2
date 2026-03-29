using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Dashboard_v2.Domain.Entities;

namespace Dashboard_v2.Application.Awards.Commands.UpdateAward;

/// <summary>
/// Actualiza un registro de premio del usuario autenticado.<br/>
/// Solo el propietario del registro (UserAwarded.UserId == currentUser) puede modificarlo.
/// </summary>
public record UpdateAwardCommand : IRequest<Result>
{
    /// <summary>Id de la fila UserAwarded a actualizar.</summary>
    public int Id { get; init; }
    public string AwardName { get; init; } = default!;
    /// <summary>Valor numérico del enum AwardType (0–7).</summary>
    public int AwardType { get; init; }
    public int Year { get; init; }
    public DateTime AwardedAt { get; init; }
}

public class UpdateAwardCommandHandler : IRequestHandler<UpdateAwardCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public UpdateAwardCommandHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateAwardCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AwardName))
            return Result.Failure(["El nombre del premio es obligatorio."]);

        if (!Enum.IsDefined(typeof(AwardType), request.AwardType))
            return Result.Failure(["Tipo de premio inválido."]);

        var userAwarded = await _context.UserAwardeds
            .Include(ua => ua.Award)
            .FirstOrDefaultAsync(ua => ua.Id == request.Id, cancellationToken);

        if (userAwarded is null)
            return Result.Failure(["Premio no encontrado."]);

        if (userAwarded.UserId != _currentUser.Id)
            return Result.Failure(["No tienes permiso para modificar este premio."]);

        userAwarded.Award.Name = request.AwardName.Trim();
        userAwarded.Award.AwardType = (AwardType)request.AwardType;
        userAwarded.Year = request.Year;
        userAwarded.AwardedAt = request.AwardedAt;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
