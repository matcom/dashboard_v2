using Dashboard_v2.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using RolesEnum = Dashboard_v2.Domain.Enums.Roles;

namespace Dashboard_v2.Web.Endpoints;

public class ProductosComercializados : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("", GetAll)
            .RequireAuthorization()
            .WithName("GetProductosComercializados")
            .Produces<List<ProductoDto>>(200);

        groupBuilder.MapGet("mis", GetMis)
            .RequireAuthorization(p => p.RequireRole(nameof(RolesEnum.Profesor), nameof(RolesEnum.Jefe_de_Proyecto), nameof(RolesEnum.Superuser)))
            .WithName("GetMisProductosComercializados")
            .Produces<List<ProductoDto>>(200);

        groupBuilder.MapPost("", Create)
            .RequireAuthorization(p => p.RequireRole(nameof(RolesEnum.Profesor), nameof(RolesEnum.Jefe_de_Proyecto), nameof(RolesEnum.Superuser)))
            .WithName("CreateProductoComercializado")
            .Produces(201)
            .ProducesProblem(400);

        groupBuilder.MapPut("{id}", Update)
            .RequireAuthorization(p => p.RequireRole(nameof(RolesEnum.Profesor), nameof(RolesEnum.Jefe_de_Proyecto), nameof(RolesEnum.Superuser)))
            .WithName("UpdateProductoComercializado")
            .Produces(200)
            .ProducesProblem(400)
            .ProducesProblem(404);

        groupBuilder.MapDelete("{id}", Delete)
            .RequireAuthorization(p => p.RequireRole(nameof(RolesEnum.Profesor), nameof(RolesEnum.Jefe_de_Proyecto), nameof(RolesEnum.Superuser)))
            .WithName("DeleteProductoComercializado")
            .Produces(200)
            .ProducesProblem(404);
    }

    private async Task<IResult> GetAll(IApplicationDbContext db)
    {
        var list = await db.ProductosComercializados
            .Include(p => p.TipoProductoComercializado)
            .Include(p => p.Institution)
            .Include(p => p.Creadores).ThenInclude(c => c.User)
            .Select(p => new ProductoDto(
                p.Id, p.Titulo,
                p.TipoProductoComercializadoId, p.TipoProductoComercializado.Nombre,
                p.InstitutionId, p.Institution.Nombre,
                p.Creadores.Select(c => c.User.UserName + " " + c.User.UserLastName1).ToList()))
            .ToListAsync();
        return Results.Ok(list);
    }

    private static async Task<IResult> GetMis(IApplicationDbContext db, IUser currentUser)
    {
        var userId = currentUser.Id;
        var list = await db.UserProductosComercializados
            .Where(up => up.UserId == userId)
            .Include(up => up.ProductoComercializado).ThenInclude(p => p.TipoProductoComercializado)
            .Include(up => up.ProductoComercializado).ThenInclude(p => p.Institution)
            .Include(up => up.ProductoComercializado).ThenInclude(p => p.Creadores).ThenInclude(c => c.User)
            .Select(up => new ProductoDto(
                up.ProductoComercializado.Id, up.ProductoComercializado.Titulo,
                up.ProductoComercializado.TipoProductoComercializadoId,
                up.ProductoComercializado.TipoProductoComercializado.Nombre,
                up.ProductoComercializado.InstitutionId, up.ProductoComercializado.Institution.Nombre,
                up.ProductoComercializado.Creadores.Select(c => c.User.UserName + " " + c.User.UserLastName1).ToList()))
            .ToListAsync();
        return Results.Ok(list);
    }

    private async Task<IResult> Create(IApplicationDbContext db, IUser currentUser, CreateProductoBody body)
    {
        var item = new Dashboard_v2.Domain.Entities.ProductoComercializado
        {
            Titulo = body.Titulo,
            TipoProductoComercializadoId = body.TipoProductoComercializadoId,
            InstitutionId = body.InstitutionId
        };
        db.ProductosComercializados.Add(item);
        db.UserProductosComercializados.Add(new Dashboard_v2.Domain.Entities.UserProductoComercializado
        {
            UserId = currentUser.Id!,
            ProductoComercializadoId = item.Id
        });
        await db.SaveChangesAsync(CancellationToken.None);
        return Results.Created($"/api/ProductosComercializados/{item.Id}", new { id = item.Id });
    }

    private async Task<IResult> Update(IApplicationDbContext db, IUser currentUser, string id, UpdateProductoBody body)
    {
        var item = await db.ProductosComercializados.FindAsync(new object[] { id }, CancellationToken.None);
        if (item == null)
            return Results.NotFound(new { errors = new[] { "Producto no encontrado." } });

        var roles = currentUser.Roles ?? [];
        if (!roles.Contains(nameof(RolesEnum.Superuser)) && !roles.Contains(nameof(RolesEnum.Jefe_de_Proyecto)))
        {
            var esCreador = await db.UserProductosComercializados.AnyAsync(up => up.ProductoComercializadoId == id && up.UserId == currentUser.Id);
            if (!esCreador)
                return Results.Forbid();
        }

        item.Titulo = body.Titulo;
        item.TipoProductoComercializadoId = body.TipoProductoComercializadoId;
        item.InstitutionId = body.InstitutionId;
        await db.SaveChangesAsync(CancellationToken.None);
        return Results.Ok(new { message = "Producto actualizado." });
    }

    private async Task<IResult> Delete(IApplicationDbContext db, IUser currentUser, string id)
    {
        var item = await db.ProductosComercializados.FindAsync(new object[] { id }, CancellationToken.None);
        if (item == null)
            return Results.NotFound(new { errors = new[] { "Producto no encontrado." } });

        var roles = currentUser.Roles ?? [];
        if (!roles.Contains(nameof(RolesEnum.Superuser)) && !roles.Contains(nameof(RolesEnum.Jefe_de_Proyecto)))
        {
            var esCreador = await db.UserProductosComercializados.AnyAsync(up => up.ProductoComercializadoId == id && up.UserId == currentUser.Id);
            if (!esCreador)
                return Results.Forbid();
        }

        db.ProductosComercializados.Remove(item);
        await db.SaveChangesAsync(CancellationToken.None);
        return Results.Ok(new { message = "Producto eliminado." });
    }
}

public record ProductoDto(string Id, string Titulo, string TipoProductoComercializadoId, string TipoProductoComercializadoNombre, string InstitutionId, string InstitutionNombre, List<string> Creadores);
public record CreateProductoBody(string Titulo, string TipoProductoComercializadoId, string InstitutionId);
public record UpdateProductoBody(string Titulo, string TipoProductoComercializadoId, string InstitutionId);
