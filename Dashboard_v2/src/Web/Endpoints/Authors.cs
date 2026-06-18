using Dashboard_v2.Application.Authors.Commands.LinkAuthorToUser;
using Dashboard_v2.Application.Authors.Queries.GetPotentialAuthorMatches;
using Dashboard_v2.Application.Authors.Queries.SearchAuthors;
using Dashboard_v2.Application.Authors.Queries.SearchCoauthors;

namespace Dashboard_v2.Web.Endpoints;

/// <summary>
/// Endpoints de gestión de autores académicos bajo /api/Authors.
/// </summary>
public class Authors : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        // GET /api/Authors/search?q=...
        // Búsqueda de autores por nombre (autocompletado).
        groupBuilder.MapGet("search", SearchAuthors)
            .RequireAuthorization()
            .WithName("SearchAuthors")
            .Produces<List<AuthorSearchDto>>(200);

        // GET /api/Authors/search-coauthors?q=...
        // Búsqueda unificada: autores existentes + usuarios sin perfil de autor.
        groupBuilder.MapGet("search-coauthors", SearchCoauthors)
            .RequireAuthorization()
            .WithName("SearchCoauthors")
            .Produces<List<CoauthorSearchDto>>(200);

        // GET /api/Authors/potential-matches
        // Devuelve autores sin cuenta cuyo nombre coincide con el usuario autenticado.
        groupBuilder.MapGet("potential-matches", GetPotentialMatches)
            .RequireAuthorization()
            .WithName("GetPotentialAuthorMatches")
            .Produces<PotentialAuthorMatchesDto>(200);

        // POST /api/Authors/{id}/link-to-me
        // Vincula el autor indicado al usuario autenticado (previa confirmación del usuario).
        groupBuilder.MapPost("{id}/link-to-me", LinkToMe)
            .RequireAuthorization()
            .WithName("LinkAuthorToMe")
            .Produces(200)
            .ProducesProblem(400);
    }

    private async Task<IResult> SearchAuthors(ISender sender, string? q)
    {
        var results = await sender.Send(new SearchAuthorsQuery(q ?? string.Empty));
        return Results.Ok(results);
    }

    private async Task<IResult> SearchCoauthors(ISender sender, string? q)
    {
        var results = await sender.Send(new SearchCoauthorsQuery(q ?? string.Empty));
        return Results.Ok(results);
    }

    private async Task<IResult> GetPotentialMatches(ISender sender)
    {
        var result = await sender.Send(new GetPotentialAuthorMatchesQuery());
        return Results.Ok(result);
    }

    private async Task<IResult> LinkToMe(ISender sender, string id)
    {
        var result = await sender.Send(new LinkAuthorToUserCommand(id));
        if (!result.Succeeded)
            return Results.BadRequest(new { errors = result.Errors });
        return Results.Ok(new { message = "Autor vinculado correctamente." });
    }
}
