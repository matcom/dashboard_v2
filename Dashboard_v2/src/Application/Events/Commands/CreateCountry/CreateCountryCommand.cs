using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Dashboard_v2.Application.Events;
using Dashboard_v2.Domain.Entities;

namespace Dashboard_v2.Application.Events.Commands.CreateCountry;

public record CreateCountryCommand(string Name) : IRequest<(Result Result, CountryDto? Country)>;

public class CreateCountryCommandHandler
    : IRequestHandler<CreateCountryCommand, (Result Result, CountryDto? Country)>
{
    private readonly IApplicationDbContext _context;

    public CreateCountryCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<(Result Result, CountryDto? Country)> Handle(
        CreateCountryCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return (Result.Failure(["El nombre del país es obligatorio."]), null);

        var exists = await _context.Countries
            .AnyAsync(c => c.Name.ToLower() == name.ToLower(), cancellationToken);
        if (exists)
            return (Result.Failure([$"El país '{name}' ya existe en el sistema."]), null);

        var country = new Country { Name = name };
        _context.Countries.Add(country);
        await _context.SaveChangesAsync(cancellationToken);

        return (Result.Success(), new CountryDto(country.Id, country.Name));
    }
}
