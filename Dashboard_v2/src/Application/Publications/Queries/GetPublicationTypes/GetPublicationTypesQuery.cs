using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Application.Publications.Queries.GetPublicationTypes;

public record PublicationTypeDto
{
    public int Id { get; init; }
    public string Name { get; init; } = default!;
}

public record GetPublicationTypesQuery : IRequest<List<PublicationTypeDto>>;

public class GetPublicationTypesQueryHandler : IRequestHandler<GetPublicationTypesQuery, List<PublicationTypeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPublicationTypesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PublicationTypeDto>> Handle(GetPublicationTypesQuery request, CancellationToken cancellationToken)
    {
        return await _context.PublicationTypes
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new PublicationTypeDto { Id = t.Id, Name = t.Name })
            .ToListAsync(cancellationToken);
    }
}
