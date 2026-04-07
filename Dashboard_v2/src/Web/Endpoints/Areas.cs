using Dashboard_v2.Application.Areas;
using Dashboard_v2.Application.Areas.Commands.CreateArea;
using Dashboard_v2.Application.Areas.Commands.DeleteArea;
using Dashboard_v2.Application.Areas.Commands.UpdateArea;
using Dashboard_v2.Application.Areas.Queries.GetAreas;
using Dashboard_v2.Web.Infrastructure;

namespace Dashboard_v2.Web.Endpoints;

/// <summary>
/// CRUD de Áreas bajo /api/Areas. Solo Superuser.
/// El campo <c>universidadId</c> en Create/Update relaciona el Área con una Universidad.
/// Pasar <c>null</c> en Update desvincula el Área de su Universidad.
/// </summary>
public class Areas : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("", GetAreas)
            .RequireAuthorization(p => p.RequireRole("Superuser"))
            .WithName("GetAreas")
            .Produces<List<AreaDto>>(200);

        groupBuilder.MapPost("", CreateArea)
            .RequireAuthorization(p => p.RequireRole("Superuser"))
            .WithName("CreateArea")
            .Produces(201)
            .ProducesProblem(400);

        groupBuilder.MapPut("{id}", UpdateArea)
            .RequireAuthorization(p => p.RequireRole("Superuser"))
            .WithName("UpdateArea")
            .Produces(200)
            .ProducesProblem(400)
            .ProducesProblem(404);

        groupBuilder.MapDelete("{id}", DeleteArea)
            .RequireAuthorization(p => p.RequireRole("Superuser"))
            .WithName("DeleteArea")
            .Produces(200)
            .ProducesProblem(404);
    }

    private async Task<IResult> GetAreas(ISender sender)
    {
        var list = await sender.Send(new GetAreasQuery());
        return Results.Ok(list);
    }

    private async Task<IResult> CreateArea(ISender sender, CreateAreaBody body)
    {
        var (result, id) = await sender.Send(new CreateAreaCommand
        {
            Nombre = body.Nombre,
            Descripcion = body.Descripcion,
            UniversidadId = body.UniversidadId,
            AreasDelConocimientoIds = body.AreasDelConocimientoIds ?? []
        });

        if (!result.Succeeded)
            return Results.BadRequest(new { errors = result.Errors });

        return Results.Created($"/api/Areas/{id}", new { id });
    }

    private async Task<IResult> UpdateArea(ISender sender, string id, UpdateAreaBody body)
    {
        var result = await sender.Send(new UpdateAreaCommand
        {
            Id = id,
            Nombre = body.Nombre,
            Descripcion = body.Descripcion,
            UniversidadId = body.UniversidadId,
            AreasDelConocimientoIds = body.AreasDelConocimientoIds ?? []
        });

        if (!result.Succeeded)
            return result.Errors.Contains("Área no encontrada.")
                ? Results.NotFound(new { errors = result.Errors })
                : Results.BadRequest(new { errors = result.Errors });

        return Results.Ok(new { message = "Área actualizada." });
    }

    private async Task<IResult> DeleteArea(ISender sender, string id)
    {
        var result = await sender.Send(new DeleteAreaCommand(id));

        if (!result.Succeeded)
            return Results.NotFound(new { errors = result.Errors });

        return Results.Ok(new { message = "Área eliminada." });
    }
}

public record CreateAreaBody(string Nombre, string? Descripcion, string? UniversidadId, IList<string>? AreasDelConocimientoIds);
public record UpdateAreaBody(string Nombre, string? Descripcion, string? UniversidadId, IList<string>? AreasDelConocimientoIds);
