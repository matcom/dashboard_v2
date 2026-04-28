using Dashboard_v2.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Web.Endpoints;

public class Redes : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("", GetRedes)
            .RequireAuthorization()
            .WithName("GetRedes")
            .Produces<List<RedDto>>(200);

        groupBuilder.MapPost("", CreateRed)
            .RequireAuthorization(p => p.RequireRole("Superuser"))
            .WithName("CreateRed")
            .Produces(201)
            .ProducesProblem(400);

        groupBuilder.MapPut("{id}", UpdateRed)
            .RequireAuthorization(p => p.RequireRole("Superuser"))
            .WithName("UpdateRed")
            .Produces(200)
            .ProducesProblem(400)
            .ProducesProblem(404);

        groupBuilder.MapDelete("{id}", DeleteRed)
            .RequireAuthorization(p => p.RequireRole("Superuser"))
            .WithName("DeleteRed")
            .Produces(200)
            .ProducesProblem(404);
    }

    private async Task<IResult> GetRedes(IApplicationDbContext db)
    {
        var list = await db.Reds
            .Select(r => new RedDto(r.Id, r.Nombre, r.EsNacional, r.CantidadProfesores))
            .ToListAsync();

        return Results.Ok(list);
    }

    private async Task<IResult> CreateRed(IApplicationDbContext db, CreateRedBody body)
    {
        var entity = new Dashboard_v2.Domain.Entities.Red
        {
            Nombre = body.Nombre,
            EsNacional = body.EsNacional,
            CantidadProfesores = body.CantidadProfesores,
        };

        db.Reds.Add(entity);
        await db.SaveChangesAsync(CancellationToken.None);

        return Results.Created($"/api/Redes/{entity.Id}", new { id = entity.Id });
    }

    private async Task<IResult> UpdateRed(IApplicationDbContext db, string id, UpdateRedBody body)
    {
        var e = await db.Reds.FindAsync(new object[] { id }, CancellationToken.None);
        if (e == null)
            return Results.NotFound(new { errors = new[] { "Red no encontrada." } });

        e.Nombre = body.Nombre;
        e.EsNacional = body.EsNacional;
        e.CantidadProfesores = body.CantidadProfesores;

        await db.SaveChangesAsync(CancellationToken.None);
        return Results.Ok(new { message = "Red actualizada." });
    }

    private async Task<IResult> DeleteRed(IApplicationDbContext db, string id)
    {
        var e = await db.Reds.FindAsync(new object[] { id }, CancellationToken.None);
        if (e == null)
            return Results.NotFound(new { errors = new[] { "Red no encontrada." } });

        db.Reds.Remove(e);
        await db.SaveChangesAsync(CancellationToken.None);
        return Results.Ok(new { message = "Red eliminada." });
    }
}

public record RedDto(string Id, string Nombre, bool EsNacional, int CantidadProfesores);
public record CreateRedBody(string Nombre, bool EsNacional, int CantidadProfesores);
public record UpdateRedBody(string Nombre, bool EsNacional, int CantidadProfesores);
