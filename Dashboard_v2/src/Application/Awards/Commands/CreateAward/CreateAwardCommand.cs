using Dashboard_v2.Application.Awards;
using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Dashboard_v2.Domain.Entities;

namespace Dashboard_v2.Application.Awards.Commands.CreateAward;

/// <summary>
/// Registra un nuevo premio para el usuario autenticado.<br/>
/// Crea una fila en Awards y otra en UserAwarded vinculada al usuario actual.
/// </summary>
public record CreateAwardCommand : IRequest<(Result Result, int? AwardedId)>
{
    public string AwardName { get; init; } = default!;
    /// <summary>Valor numérico del enum AwardType (0–7).</summary>
    public int AwardType { get; init; }
    public int Year { get; init; }
    public DateTime AwardedAt { get; init; }
}

public class CreateAwardCommandHandler : IRequestHandler<CreateAwardCommand, (Result Result, int? AwardedId)>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public CreateAwardCommandHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<(Result Result, int? AwardedId)> Handle(
        CreateAwardCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AwardName))
            return (Result.Failure(["El nombre del premio es obligatorio."]), null);

        if (!Enum.IsDefined(typeof(AwardType), request.AwardType))
            return (Result.Failure(["Tipo de premio inválido."]), null);

        var award = new Award
        {
            Name = request.AwardName.Trim(),
            AwardType = (AwardType)request.AwardType,
        };
        _context.Awards.Add(award);
        await _context.SaveChangesAsync(cancellationToken);

        var userAwarded = new UserAwarded
        {
            UserId = _currentUser.Id!,
            AwardId = award.Id,
            Year = request.Year,
            AwardedAt = request.AwardedAt,
        };
        _context.UserAwardeds.Add(userAwarded);
        await _context.SaveChangesAsync(cancellationToken);

        return (Result.Success(), userAwarded.Id);
    }
}
