using Dashboard_v2.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Web.Endpoints;

public class Patentes : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("", GetPatentes)
            .RequireAuthorization()
            .WithName("GetPatentes")
            .Produces<List<PatenteDto>>(200);

        groupBuilder.MapPost("", CreatePatente)
            .RequireAuthorization(p => p.RequireRole("Superuser"))
            .WithName("CreatePatente")
            .Produces(201)
            .ProducesProblem(400);

        groupBuilder.MapPut("{id}", UpdatePatente)
            .RequireAuthorization(p => p.RequireRole("Superuser"))
            .WithName("UpdatePatente")
            .Produces(200)
            .ProducesProblem(400)
            .ProducesProblem(404);

        groupBuilder.MapDelete("{id}", DeletePatente)
            .RequireAuthorization(p => p.RequireRole("Superuser"))
            .WithName("DeletePatente")
            .Produces(200)
            .ProducesProblem(404);
    }

    private async Task<IResult> GetPatentes(IApplicationDbContext db)
    {
        var list = await db.Patentes
            .Select(p => new PatenteDto(p.Id, p.Titulo, p.NumeroSolicitudConcesion, p.EsNacional))
            .ToListAsync();

        return Results.Ok(list);
    }

    private async Task<IResult> CreatePatente(IApplicationDbContext db, CreatePatenteBody body)
    {
        var p = new Dashboard_v2.Domain.Entities.Patente
        {
            Titulo = body.Titulo,
            NumeroSolicitudConcesion = body.NumeroSolicitudConcesion,
            EsNacional = body.EsNacional
        };

        db.Patentes.Add(p);
        await db.SaveChangesAsync(CancellationToken.None);

        return Results.Created($"/api/Patentes/{p.Id}", new { id = p.Id });
    }

    private async Task<IResult> UpdatePatente(IApplicationDbContext db, string id, UpdatePatenteBody body)
    {
        var p = await db.Patentes.FindAsync(new object[] { id }, CancellationToken.None);
        if (p == null)
            return Results.NotFound(new { errors = new[] { "Patente no encontrada." } });

        p.Titulo = body.Titulo;
        p.NumeroSolicitudConcesion = body.NumeroSolicitudConcesion;
        p.EsNacional = body.EsNacional;

        await db.SaveChangesAsync(CancellationToken.None);
        return Results.Ok(new { message = "Patente actualizada." });
    }

    private async Task<IResult> DeletePatente(IApplicationDbContext db, string id)
    {
        var p = await db.Patentes.FindAsync(new object[] { id }, CancellationToken.None);
        if (p == null)
            return Results.NotFound(new { errors = new[] { "Patente no encontrada." } });

        db.Patentes.Remove(p);
        await db.SaveChangesAsync(CancellationToken.None);
        return Results.Ok(new { message = "Patente eliminada." });
    }
}

public record PatenteDto(string Id, string Titulo, string NumeroSolicitudConcesion, bool EsNacional);
public record CreatePatenteBody(string Titulo, string NumeroSolicitudConcesion, bool EsNacional);
public record UpdatePatenteBody(string Titulo, string NumeroSolicitudConcesion, bool EsNacional);
