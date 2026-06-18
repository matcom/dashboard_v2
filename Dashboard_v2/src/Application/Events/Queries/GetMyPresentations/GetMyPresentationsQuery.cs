using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Events;

namespace Dashboard_v2.Application.Events.Queries.GetMyPresentations;

/// <summary>Retorna las presentaciones del usuario autenticado (donde es autor).</summary>
public record GetMyPresentationsQuery : IRequest<List<PresentationDto>>;

public class GetMyPresentationsQueryHandler : IRequestHandler<GetMyPresentationsQuery, List<PresentationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public GetMyPresentationsQueryHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<PresentationDto>> Handle(GetMyPresentationsQuery request, CancellationToken cancellationToken)
    {
        var authorId = await _context.Authors
            .AsNoTracking()
            .Where(a => a.UserId == _currentUser.Id)
            .Select(a => a.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (authorId is null)
            return [];

        return await _context.Presentations
            .AsNoTracking()
            .Where(p => p.AuthorPresentations.Any(ap => ap.AuthorId == authorId))
            .Select(p => new PresentationDto
            {
                Id = p.Id,
                Name = p.Name,
                EventId = p.EventId,
                EventName = p.Event.Name,
                Authors = p.AuthorPresentations
                    .Select(ap => ap.Author.Name)
                    .ToList(),
            })
            .OrderBy(p => p.EventName)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }
}
