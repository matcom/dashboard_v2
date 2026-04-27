using Dashboard_v2.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Web.Endpoints;

public class Registros : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("", GetRegistros)
            .RequireAuthorization()
            .WithName("GetRegistros")
            .Produces<List<RegistroDto>>(200);

        groupBuilder.MapPost("", CreateRegistro)
            .RequireAuthorization(p => p.RequireRole("Superuser"))
            .WithName("CreateRegistro")
            .Produces(201)
            .ProducesProblem(400);

        groupBuilder.MapPut("{id}", UpdateRegistro)
            .RequireAuthorization(p => p.RequireRole("Superuser"))
            .WithName("UpdateRegistro")
            .Produces(200)
            .ProducesProblem(400)
            .ProducesProblem(404);

        groupBuilder.MapDelete("{id}", DeleteRegistro)
            .RequireAuthorization(p => p.RequireRole("Superuser"))
            .WithName("DeleteRegistro")
            .Produces(200)
            .ProducesProblem(404);
    }

    private async Task<IResult> GetRegistros(IApplicationDbContext db)
    {
        var list = await db.Registros
            .Include(r => r.Country)
            .Include(r => r.Institution)
            .Select(r => new RegistroDto(
                r.Id,
                r.Titulo,
                r.NumeroCertificado,
                r.EsInformatico,
                r.CountryId,
                r.Country.Name,
                r.InstitutionId,
                r.Institution.Nombre))
            .ToListAsync();

        return Results.Ok(list);
    }

    private async Task<IResult> CreateRegistro(IApplicationDbContext db, CreateRegistroBody body)
    {
        var registro = new Dashboard_v2.Domain.Entities.Registro
        {
            Titulo = body.Titulo,
            NumeroCertificado = body.NumeroCertificado,
            EsInformatico = body.EsInformatico,
            CountryId = body.CountryId,
            InstitutionId = body.InstitutionId
        };

        db.Registros.Add(registro);
        await db.SaveChangesAsync(CancellationToken.None);

        return Results.Created($"/api/Registros/{registro.Id}", new { id = registro.Id });
    }

    private async Task<IResult> UpdateRegistro(IApplicationDbContext db, string id, UpdateRegistroBody body)
    {
        var registro = await db.Registros.FindAsync(new object[] { id }, CancellationToken.None);
        if (registro == null)
            return Results.NotFound(new { errors = new[] { "Registro no encontrado." } });

        registro.Titulo = body.Titulo;
        registro.NumeroCertificado = body.NumeroCertificado;
        registro.EsInformatico = body.EsInformatico;
        registro.CountryId = body.CountryId;
        registro.InstitutionId = body.InstitutionId;

        await db.SaveChangesAsync(CancellationToken.None);
        return Results.Ok(new { message = "Registro actualizado." });
    }

    private async Task<IResult> DeleteRegistro(IApplicationDbContext db, string id)
    {
        var registro = await db.Registros.FindAsync(new object[] { id }, CancellationToken.None);
        if (registro == null)
            return Results.NotFound(new { errors = new[] { "Registro no encontrado." } });

        db.Registros.Remove(registro);
        await db.SaveChangesAsync(CancellationToken.None);
        return Results.Ok(new { message = "Registro eliminado." });
    }
}

public record RegistroDto(string Id, string Titulo, string NumeroCertificado, bool EsInformatico, int CountryId, string CountryName, string InstitutionId, string InstitutionNombre);
public record CreateRegistroBody(string Titulo, string NumeroCertificado, bool EsInformatico, int CountryId, string InstitutionId);
public record UpdateRegistroBody(string Titulo, string NumeroCertificado, bool EsInformatico, int CountryId, string InstitutionId);
