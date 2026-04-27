using Dashboard_v2.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Web.Endpoints;

public class ProductosComercializados : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("", GetAll)
            .RequireAuthorization()
            .WithName("GetProductosComercializados")
            .Produces<List<ProductoDto>>(200);

        groupBuilder.MapPost("", Create)
            .RequireAuthorization(p => p.RequireRole("Superuser"))
            .WithName("CreateProductoComercializado")
            .Produces(201)
            .ProducesProblem(400);

        groupBuilder.MapPut("{id}", Update)
            .RequireAuthorization(p => p.RequireRole("Superuser"))
            .WithName("UpdateProductoComercializado")
            .Produces(200)
            .ProducesProblem(400)
            .ProducesProblem(404);

        groupBuilder.MapDelete("{id}", Delete)
            .RequireAuthorization(p => p.RequireRole("Superuser"))
            .WithName("DeleteProductoComercializado")
            .Produces(200)
            .ProducesProblem(404);
    }

    private async Task<IResult> GetAll(IApplicationDbContext db)
    {
        var list = await db.ProductosComercializados
            .Include(p => p.TipoProductoComercializado)
            .Include(p => p.Institution)
            .Select(p => new ProductoDto(
                p.Id,
                p.Titulo,
                p.TipoProductoComercializadoId,
                p.TipoProductoComercializado.Nombre,
                p.InstitutionId,
                p.Institution.Nombre))
            .ToListAsync();

        return Results.Ok(list);
    }

    private async Task<IResult> Create(IApplicationDbContext db, CreateProductoBody body)
    {
        var item = new Dashboard_v2.Domain.Entities.ProductoComercializado
        {
            Titulo = body.Titulo,
            TipoProductoComercializadoId = body.TipoProductoComercializadoId,
            InstitutionId = body.InstitutionId
        };

        db.ProductosComercializados.Add(item);
        await db.SaveChangesAsync(CancellationToken.None);

        return Results.Created($"/api/ProductosComercializados/{item.Id}", new { id = item.Id });
    }

    private async Task<IResult> Update(IApplicationDbContext db, string id, UpdateProductoBody body)
    {
        var item = await db.ProductosComercializados.FindAsync(new object[] { id }, CancellationToken.None);
        if (item == null)
            return Results.NotFound(new { errors = new[] { "Producto no encontrado." } });

        item.Titulo = body.Titulo;
        item.TipoProductoComercializadoId = body.TipoProductoComercializadoId;
        item.InstitutionId = body.InstitutionId;

        await db.SaveChangesAsync(CancellationToken.None);
        return Results.Ok(new { message = "Producto actualizado." });
    }

    private async Task<IResult> Delete(IApplicationDbContext db, string id)
    {
        var item = await db.ProductosComercializados.FindAsync(new object[] { id }, CancellationToken.None);
        if (item == null)
            return Results.NotFound(new { errors = new[] { "Producto no encontrado." } });

        db.ProductosComercializados.Remove(item);
        await db.SaveChangesAsync(CancellationToken.None);
        return Results.Ok(new { message = "Producto eliminado." });
    }
}

public record ProductoDto(string Id, string Titulo, string TipoProductoComercializadoId, string TipoProductoComercializadoNombre, string InstitutionId, string InstitutionNombre);
public record CreateProductoBody(string Titulo, string TipoProductoComercializadoId, string InstitutionId);
public record UpdateProductoBody(string Titulo, string TipoProductoComercializadoId, string InstitutionId);
