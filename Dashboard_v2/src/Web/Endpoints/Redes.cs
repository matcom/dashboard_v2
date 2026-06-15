using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Domain.Entities;
using Dashboard_v2.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Web.Endpoints;

public class Redes : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("", GetRedes)
            .RequireAuthorization(p => p.RequireRole("Superuser", "Jefe_de_Redes", "Profesor", "Vicedecano_de_investigacion"))
            .WithName("GetRedes")
            .Produces<List<RedDto>>(200);

        groupBuilder.MapGet("mis-redes", GetMisRedes)
            .RequireAuthorization(p => p.RequireRole("Jefe_de_Redes", "Profesor", "Superuser"))
            .WithName("GetMisRedes")
            .Produces<List<RedConCoordinadorDto>>(200);

        groupBuilder.MapPut("{id}/coordinador", SetCoordinador)
            .RequireAuthorization(p => p.RequireRole("Jefe_de_Redes", "Superuser"))
            .WithName("SetCoordinador")
            .Produces(200)
            .ProducesProblem(400)
            .ProducesProblem(404);

        groupBuilder.MapGet("{id}/participantes", GetParticipantes)
            .RequireAuthorization(p => p.RequireRole("Jefe_de_Redes", "Superuser"))
            .WithName("GetParticipantesRed")
            .Produces<List<ParticipanteRedDto>>(200)
            .ProducesProblem(404);

        groupBuilder.MapPost("{id}/participantes/{authorId}", AddParticipante)
            .RequireAuthorization(p => p.RequireRole("Jefe_de_Redes", "Superuser"))
            .WithName("AddParticipanteRed")
            .Produces(204)
            .ProducesProblem(400)
            .ProducesProblem(404);

        groupBuilder.MapDelete("{id}/participantes/{authorId}", RemoveParticipante)
            .RequireAuthorization(p => p.RequireRole("Jefe_de_Redes", "Superuser"))
            .WithName("RemoveParticipanteRed")
            .Produces(204)
            .ProducesProblem(404);

        groupBuilder.MapPost("", CreateRed)
            .RequireAuthorization(p => p.RequireRole("Superuser", "Jefe_de_Redes"))
            .WithName("CreateRed")
            .Produces(201)
            .ProducesProblem(400);

        groupBuilder.MapPut("{id}", UpdateRed)
            .RequireAuthorization(p => p.RequireRole("Superuser", "Jefe_de_Redes"))
            .WithName("UpdateRed")
            .Produces(200)
            .ProducesProblem(400)
            .ProducesProblem(404);

        groupBuilder.MapDelete("{id}", DeleteRed)
            .RequireAuthorization(p => p.RequireRole("Superuser", "Jefe_de_Redes"))
            .WithName("DeleteRed")
            .Produces(200)
            .ProducesProblem(404);

        groupBuilder.MapGet("{id}/events", GetEventsForRed)
            .RequireAuthorization(p => p.RequireRole("Superuser", "Jefe_de_Redes"))
            .WithName("GetEventsForRed")
            .Produces<List<EventForRedDto>>(200);

        groupBuilder.MapPost("{id}/events", SetEventsForRed)
            .RequireAuthorization(p => p.RequireRole("Superuser", "Jefe_de_Redes"))
            .WithName("SetEventsForRed")
            .Produces(200)
            .ProducesProblem(400)
            .ProducesProblem(404);
    }

    private async Task<IResult> GetMisRedes(IApplicationDbContext db, IUser currentUser, HttpContext http)
    {
        IQueryable<Red> query;

        if (http.User.IsInRole("Superuser"))
        {
            query = db.Reds.AsNoTracking();
        }
        else if (http.User.IsInRole("Jefe_de_Redes"))
        {
            var dbUser = await db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == currentUser.Id);
            if (dbUser?.AreaId == null)
                return Results.Ok(new List<RedConCoordinadorDto>());

            var jefeAreaId = dbUser.AreaId;
            query = db.Reds.AsNoTracking()
                .Where(r =>
                    (r.Coordinador != null && r.Coordinador.AreaId == jefeAreaId) ||
                    r.Participaciones.Any(p => p.Author.User != null && p.Author.User.AreaId == jefeAreaId));
        }
        else
        {
            // Profesor: reds they coordinate OR participate in
            query = db.Reds.AsNoTracking().Where(r =>
                r.CoordinadorId == currentUser.Id ||
                r.Participaciones.Any(p => p.Author.UserId == currentUser.Id));
        }

        var reds = await query
            .Include(r => r.Coordinador)
            .Include(r => r.Participaciones).ThenInclude(p => p.Author)
            .Include(r => r.Country)
            .OrderBy(r => r.Nombre)
            .ToListAsync();

        var result = reds.Select(r => new RedConCoordinadorDto(
            r.Id,
            r.Nombre,
            (int)r.Tipo,
            r.Country?.Name,
            r.CantidadProfesores,
            r.CoordinadorId,
            r.Coordinador != null ? $"{r.Coordinador.UserName} {r.Coordinador.UserLastName1}" : null,
            r.Coordinador?.Email,
            r.Participaciones.Select(p => new ParticipanteRedDto(p.AuthorId, p.Author.Name)).ToList()
        )).ToList();

        return Results.Ok(result);
    }

    private async Task<IResult> SetCoordinador(IApplicationDbContext db, string id, SetCoordinadorBody body)
    {
        var red = await db.Reds.FindAsync(new object[] { id }, CancellationToken.None);
        if (red == null)
            return Results.NotFound(new { errors = new[] { "Red no encontrada." } });

        if (body.CoordinadorId != null)
        {
            var existe = await db.Users.AnyAsync(u => u.Id == body.CoordinadorId);
            if (!existe)
                return Results.BadRequest(new { errors = new[] { "Usuario coordinador no encontrado." } });
        }

        red.CoordinadorId = body.CoordinadorId;
        await db.SaveChangesAsync(CancellationToken.None);
        return Results.Ok(new { message = "Coordinador actualizado." });
    }

    private async Task<IResult> GetParticipantes(IApplicationDbContext db, string id)
    {
        if (!await db.Reds.AnyAsync(r => r.Id == id))
            return Results.NotFound(new { errors = new[] { "Red no encontrada." } });

        var list = await db.ParticipacionesEnRed
            .AsNoTracking()
            .Where(p => p.RedId == id)
            .Include(p => p.Author)
            .Select(p => new ParticipanteRedDto(p.AuthorId, p.Author.Name))
            .ToListAsync();

        return Results.Ok(list);
    }

    private async Task<IResult> AddParticipante(IApplicationDbContext db, string id, string authorId)
    {
        if (!await db.Reds.AnyAsync(r => r.Id == id))
            return Results.NotFound(new { errors = new[] { "Red no encontrada." } });
        if (!await db.Authors.AnyAsync(a => a.Id == authorId))
            return Results.NotFound(new { errors = new[] { "Autor no encontrado." } });
        if (await db.ParticipacionesEnRed.AnyAsync(p => p.RedId == id && p.AuthorId == authorId))
            return Results.BadRequest(new { errors = new[] { "El autor ya es participante de esta red." } });

        db.ParticipacionesEnRed.Add(new ParticipacionEnRed { RedId = id, AuthorId = authorId });
        await db.SaveChangesAsync(CancellationToken.None);
        return Results.NoContent();
    }

    private async Task<IResult> RemoveParticipante(IApplicationDbContext db, string id, string authorId)
    {
        var p = await db.ParticipacionesEnRed
            .FirstOrDefaultAsync(x => x.RedId == id && x.AuthorId == authorId);
        if (p == null)
            return Results.NotFound(new { errors = new[] { "El autor no es participante de esta red." } });

        db.ParticipacionesEnRed.Remove(p);
        await db.SaveChangesAsync(CancellationToken.None);
        return Results.NoContent();
    }

    private async Task<IResult> GetRedes(IApplicationDbContext db, IUser currentUser, HttpContext http)
    {
        IQueryable<Red> query = db.Reds.AsNoTracking();

        if (http.User.IsInRole("Vicedecano_de_investigacion"))
        {
            var areaId = await db.Users.AsNoTracking()
                .Where(u => u.Id == currentUser.Id)
                .Select(u => u.AreaId)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(areaId))
            {
                query = query.Where(r =>
                    (r.CoordinadorId != null && r.Coordinador!.AreaId == areaId) ||
                    r.Participaciones.Any(p => p.Author.User != null && p.Author.User.AreaId == areaId));
            }
        }

        var list = await query
            .Select(r => new RedDto(r.Id, r.Nombre, r.CountryId, r.Country != null ? r.Country.Name : null, r.CantidadProfesores, (int)r.Tipo))
            .ToListAsync();

        return Results.Ok(list);
    }

    private async Task<IResult> CreateRed(IApplicationDbContext db, CreateRedBody body)
    {
        if (!Enum.IsDefined(typeof(TipoRed), body.Tipo))
            return Results.BadRequest(new { errors = new[] { "Tipo de red no válido." } });

        var entity = new Red
        {
            Nombre = body.Nombre,
            CountryId = body.CountryId,
            CantidadProfesores = body.CantidadProfesores,
            Tipo = (TipoRed)body.Tipo,
        };

        db.Reds.Add(entity);
        await db.SaveChangesAsync(CancellationToken.None);

        return Results.Created($"/api/Redes/{entity.Id}", new { id = entity.Id });
    }

    private async Task<IResult> UpdateRed(IApplicationDbContext db, string id, UpdateRedBody body)
    {
        if (!Enum.IsDefined(typeof(TipoRed), body.Tipo))
            return Results.BadRequest(new { errors = new[] { "Tipo de red no válido." } });

        var e = await db.Reds.FindAsync(new object[] { id }, CancellationToken.None);
        if (e == null)
            return Results.NotFound(new { errors = new[] { "Red no encontrada." } });

        e.Nombre = body.Nombre;
        e.CountryId = body.CountryId;
        e.CantidadProfesores = body.CantidadProfesores;
        e.Tipo = (TipoRed)body.Tipo;

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

    private async Task<IResult> GetEventsForRed(IApplicationDbContext db, string id)
    {
        var list = await db.Events
            .AsNoTracking()
            .OrderBy(e => e.Name)
            .Select(e => new EventForRedDto(e.Id, e.Name, e.RedId == id))
            .ToListAsync();

        return Results.Ok(list);
    }

    private async Task<IResult> SetEventsForRed(IApplicationDbContext db, string id, SetEventsBody body)
    {
        var red = await db.Reds.FindAsync(new object[] { id }, CancellationToken.None);
        if (red == null)
            return Results.NotFound(new { errors = new[] { "Red no encontrada." } });

        var eventIds = (body?.EventIds ?? new List<int>()).Distinct().ToList();

        var events = await db.Events.Where(e => eventIds.Contains(e.Id)).ToListAsync();
        if (events.Count != eventIds.Count)
            return Results.BadRequest(new { errors = new[] { "Uno o más eventos no existen." } });

        var currentlyAssigned = await db.Events.Where(e => e.RedId == id).ToListAsync();
        var toUnassign = currentlyAssigned.Where(e => !eventIds.Contains(e.Id)).ToList();
        foreach (var e in toUnassign) e.RedId = null;
        foreach (var e in events) e.RedId = id;

        await db.SaveChangesAsync(CancellationToken.None);
        return Results.Ok(new { message = "Eventos actualizados." });
    }
}

public record RedDto(string Id, string Nombre, int? CountryId, string? CountryName, int CantidadProfesores, int Tipo);
public record RedConCoordinadorDto(
    string Id,
    string Nombre,
    int Tipo,
    string? CountryName,
    int CantidadProfesores,
    string? CoordinadorId,
    string? CoordinadorNombre,
    string? CoordinadorEmail,
    List<ParticipanteRedDto> Participantes);
public record ParticipanteRedDto(string AuthorId, string AuthorNombre);
public record SetCoordinadorBody(string? CoordinadorId);
public record CreateRedBody(string Nombre, int CountryId, int CantidadProfesores, int Tipo);
public record UpdateRedBody(string Nombre, int CountryId, int CantidadProfesores, int Tipo);
public record EventForRedDto(int Id, string Name, bool Assigned);
public record SetEventsBody(List<int> EventIds);
