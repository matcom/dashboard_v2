using Dashboard_v2.Application.GruposDeInvestigacion;
using Dashboard_v2.Application.GruposDeInvestigacion.Commands.CreateGrupoDeInvestigacion;
using Dashboard_v2.Application.GruposDeInvestigacion.Commands.DeleteGrupoDeInvestigacion;
using Dashboard_v2.Application.GruposDeInvestigacion.Commands.UpdateGrupoDeInvestigacion;
using Dashboard_v2.Application.GruposDeInvestigacion.Queries.GetGruposDeInvestigacion;
using Dashboard_v2.Web.Infrastructure;

namespace Dashboard_v2.Web.Endpoints;

/// <summary>
/// CRUD de Grupos de Investigación bajo /api/GruposDeInvestigacion. Solo Superuser.
/// El campo <c>areaId</c> en Create/Update relaciona el Grupo con un Área.
/// </summary>
public class GruposDeInvestigacion : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("", GetGruposDeInvestigacion)
            .RequireAuthorization(p => p.RequireRole("Superuser"))
            .WithName("GetGruposDeInvestigacion")
            .Produces<List<GrupoDeInvestigacionDto>>(200);

        groupBuilder.MapPost("", CreateGrupoDeInvestigacion)
            .RequireAuthorization(p => p.RequireRole("Superuser"))
            .WithName("CreateGrupoDeInvestigacion")
            .Produces(201)
            .ProducesProblem(400);

        groupBuilder.MapPut("{id}", UpdateGrupoDeInvestigacion)
            .RequireAuthorization(p => p.RequireRole("Superuser"))
            .WithName("UpdateGrupoDeInvestigacion")
            .Produces(200)
            .ProducesProblem(400)
            .ProducesProblem(404);

        groupBuilder.MapDelete("{id}", DeleteGrupoDeInvestigacion)
            .RequireAuthorization(p => p.RequireRole("Superuser"))
            .WithName("DeleteGrupoDeInvestigacion")
            .Produces(200)
            .ProducesProblem(404);
    }

    private async Task<IResult> GetGruposDeInvestigacion(ISender sender)
    {
        var list = await sender.Send(new GetGruposDeInvestigacionQuery());
        return Results.Ok(list);
    }

    private async Task<IResult> CreateGrupoDeInvestigacion(ISender sender, CreateGrupoDeInvestigacionBody body)
    {
        var (result, id) = await sender.Send(new CreateGrupoDeInvestigacionCommand
        {
            Nombre = body.Nombre,
            AreaId = body.AreaId,
            LineasDeInvestigacionIds = body.LineasDeInvestigacionIds ?? []
        });

        if (!result.Succeeded)
            return Results.BadRequest(new { errors = result.Errors });

        return Results.Created($"/api/GruposDeInvestigacion/{id}", new { id });
    }

    private async Task<IResult> UpdateGrupoDeInvestigacion(ISender sender, string id, UpdateGrupoDeInvestigacionBody body)
    {
        var result = await sender.Send(new UpdateGrupoDeInvestigacionCommand
        {
            Id = id,
            Nombre = body.Nombre,
            AreaId = body.AreaId,
            LineasDeInvestigacionIds = body.LineasDeInvestigacionIds ?? []
        });

        if (!result.Succeeded)
            return result.Errors.Contains("Grupo de investigación no encontrado.")
                ? Results.NotFound(new { errors = result.Errors })
                : Results.BadRequest(new { errors = result.Errors });

        return Results.Ok(new { message = "Grupo de investigación actualizado." });
    }

    private async Task<IResult> DeleteGrupoDeInvestigacion(ISender sender, string id)
    {
        var result = await sender.Send(new DeleteGrupoDeInvestigacionCommand(id));

        if (!result.Succeeded)
            return Results.NotFound(new { errors = result.Errors });

        return Results.Ok(new { message = "Grupo de investigación eliminado." });
    }
}

public record CreateGrupoDeInvestigacionBody(string Nombre, string AreaId, IList<string>? LineasDeInvestigacionIds);
public record UpdateGrupoDeInvestigacionBody(string Nombre, string AreaId, IList<string>? LineasDeInvestigacionIds);
