using Dashboard_v2.Application.Awards;
using Dashboard_v2.Application.Awards.Commands.CreateAward;
using Dashboard_v2.Application.Awards.Commands.DeleteAward;
using Dashboard_v2.Application.Awards.Commands.UpdateAward;
using Dashboard_v2.Application.Awards.Queries.GetMyAwards;

namespace Dashboard_v2.Web.Endpoints;

/// <summary>
/// Endpoints de gestión de premios bajo /api/Awards.
/// Todos requieren el rol "Profesor".
/// </summary>
public class Awards : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        // GET /api/Awards — premios del usuario autenticado
        groupBuilder.MapGet("", GetMyAwards)
            .RequireAuthorization(p => p.RequireRole("Profesor"))
            .WithName("GetMyAwards")
            .Produces<List<AwardDto>>(200);

        // POST /api/Awards
        groupBuilder.MapPost("", CreateAward)
            .RequireAuthorization(p => p.RequireRole("Profesor"))
            .WithName("CreateAward")
            .Produces(201)
            .ProducesProblem(400);

        // PUT /api/Awards/{id}
        groupBuilder.MapPut("{id}", UpdateAward)
            .RequireAuthorization(p => p.RequireRole("Profesor"))
            .WithName("UpdateAward")
            .Produces(200)
            .ProducesProblem(400)
            .ProducesProblem(404);

        // DELETE /api/Awards/{id}
        groupBuilder.MapDelete("{id}", DeleteAward)
            .RequireAuthorization(p => p.RequireRole("Profesor"))
            .WithName("DeleteAward")
            .Produces(200)
            .ProducesProblem(400)
            .ProducesProblem(404);
    }

    private async Task<IResult> GetMyAwards(ISender sender)
    {
        var awards = await sender.Send(new GetMyAwardsQuery());
        return Results.Ok(awards);
    }

    private async Task<IResult> CreateAward(ISender sender, CreateAwardBody body)
    {
        var (result, id) = await sender.Send(new CreateAwardCommand
        {
            AwardName = body.AwardName,
            AwardType = body.AwardType,
            Year = body.Year,
            AwardedAt = body.AwardedAt,
        });

        if (!result.Succeeded)
            return Results.BadRequest(new { errors = result.Errors });

        return Results.Created($"/api/Awards/{id}", new { id });
    }

    private async Task<IResult> UpdateAward(ISender sender, int id, UpdateAwardBody body)
    {
        var result = await sender.Send(new UpdateAwardCommand
        {
            Id = id,
            AwardName = body.AwardName,
            AwardType = body.AwardType,
            Year = body.Year,
            AwardedAt = body.AwardedAt,
        });

        if (!result.Succeeded)
            return Results.BadRequest(new { errors = result.Errors });

        return Results.Ok(new { message = "Premio actualizado." });
    }

    private async Task<IResult> DeleteAward(ISender sender, int id)
    {
        var result = await sender.Send(new DeleteAwardCommand(id));

        if (!result.Succeeded)
            return Results.BadRequest(new { errors = result.Errors });

        return Results.Ok(new { message = "Premio eliminado." });
    }
}

public record CreateAwardBody(string AwardName, int AwardType, int Year, DateTime AwardedAt);
public record UpdateAwardBody(string AwardName, int AwardType, int Year, DateTime AwardedAt);
