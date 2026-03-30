using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Events;

namespace Dashboard_v2.Application.Events.Queries.GetCountries;

public record GetCountriesQuery : IRequest<List<CountryDto>>;

public class GetCountriesQueryHandler : IRequestHandler<GetCountriesQuery, List<CountryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCountriesQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<List<CountryDto>> Handle(GetCountriesQuery request, CancellationToken cancellationToken)
        => await _context.Countries
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CountryDto(c.Id, c.Name))
            .ToListAsync(cancellationToken);
}
