using Dashboard_v2.Application.Awards;
using Dashboard_v2.Application.Common.Interfaces;

namespace Dashboard_v2.Application.Awards.Queries.GetMyAwards;

/// <summary>Retorna todos los premios registrados por el usuario autenticado.</summary>
public record GetMyAwardsQuery : IRequest<List<AwardDto>>;

public class GetMyAwardsQueryHandler : IRequestHandler<GetMyAwardsQuery, List<AwardDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public GetMyAwardsQueryHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<AwardDto>> Handle(GetMyAwardsQuery request, CancellationToken cancellationToken)
    {
        return await _context.UserAwardeds
            .AsNoTracking()
            .Where(ua => ua.UserId == _currentUser.Id)
            .Select(ua => new AwardDto
            {
                Id = ua.Id,
                AwardName = ua.Award.Name,
                AwardTypeId = ua.Award.AwardTypeId,
                AwardTypeName = ua.Award.AwardType.Name,
                Year = ua.Year,
                AwardedAt = ua.AwardedAt,
            })
            .OrderByDescending(a => a.Year)
            .ThenByDescending(a => a.AwardedAt)
            .ToListAsync(cancellationToken);
    }
}
