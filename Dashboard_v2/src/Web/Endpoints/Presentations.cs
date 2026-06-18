using Dashboard_v2.Application.Events;
using Dashboard_v2.Application.Events.Commands.CreatePresentation;
using Dashboard_v2.Application.Events.Commands.DeletePresentation;
using Dashboard_v2.Application.Events.Commands.UpdatePresentation;
using Dashboard_v2.Application.Events.Queries.GetMyPresentations;

namespace Dashboard_v2.Web.Endpoints;

public class Presentations : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("", GetMyPresentations)
            .RequireAuthorization(p => p.RequireRole("Profesor"))
            .WithName("GetMyPresentations")
            .Produces<List<PresentationDto>>(200);

        groupBuilder.MapPost("", CreatePresentation)
            .RequireAuthorization(p => p.RequireRole("Profesor"))
            .WithName("CreatePresentation")
            .Produces(201)
            .ProducesProblem(400);

        groupBuilder.MapPut("{id}", UpdatePresentation)
            .RequireAuthorization(p => p.RequireRole("Profesor"))
            .WithName("UpdatePresentation")
            .Produces(200)
            .ProducesProblem(400);

        groupBuilder.MapDelete("{id}", DeletePresentation)
            .RequireAuthorization(p => p.RequireRole("Profesor"))
            .WithName("DeletePresentation")
            .Produces(200)
            .ProducesProblem(400);
    }

    private async Task<IResult> GetMyPresentations(ISender sender)
        => Results.Ok(await sender.Send(new GetMyPresentationsQuery()));

    private async Task<IResult> CreatePresentation(ISender sender, CreatePresentationBody body)
    {
        var (result, id) = await sender.Send(new CreatePresentationCommand
        {
            Name = body.Name,
            EventId = body.EventId,
            CoauthorIds = body.CoauthorIds,
            CoauthorNames = body.CoauthorNames,
        });

        if (!result.Succeeded)
            return Results.BadRequest(new { errors = result.Errors });

        return Results.Created($"/api/Presentations/{id}", new { id });
    }

    private async Task<IResult> UpdatePresentation(ISender sender, int id, UpdatePresentationBody body)
    {
        var result = await sender.Send(new UpdatePresentationCommand
        {
            Id = id,
            Name = body.Name,
            EventId = body.EventId,
            CoauthorIds = body.CoauthorIds,
            CoauthorNames = body.CoauthorNames,
        });

        if (!result.Succeeded)
            return Results.BadRequest(new { errors = result.Errors });

        return Results.Ok(new { message = "Presentación actualizada." });
    }

    private async Task<IResult> DeletePresentation(ISender sender, int id)
    {
        var result = await sender.Send(new DeletePresentationCommand(id));

        if (!result.Succeeded)
            return Results.BadRequest(new { errors = result.Errors });

        return Results.Ok(new { message = "Presentación eliminada." });
    }
}

public record CreatePresentationBody(
    string Name,
    int EventId,
    List<string> CoauthorIds,
    List<string> CoauthorNames);

public record UpdatePresentationBody(
    string Name,
    int EventId,
    List<string> CoauthorIds,
    List<string> CoauthorNames);
