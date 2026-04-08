using Dashboard_v2.Application.Events;
using Dashboard_v2.Application.Events.Commands.CreateCountry;
using Dashboard_v2.Application.Events.Commands.CreateEvent;
using Dashboard_v2.Application.Events.Commands.DeleteEvent;
using Dashboard_v2.Application.Events.Commands.UpdateEvent;
using Dashboard_v2.Application.Events.Queries.GetAllEvents;
using Dashboard_v2.Application.Events.Queries.GetCountries;
using Dashboard_v2.Application.Events.Queries.GetEventTypes;
using Dashboard_v2.Application.Events.Queries.GetMyEvents;

namespace Dashboard_v2.Web.Endpoints;

public class Events : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("", GetMyEvents)
            .RequireAuthorization(p => p.RequireRole("Profesor"))
            .WithName("GetMyEvents")
            .Produces<List<EventDto>>(200);

        groupBuilder.MapGet("all", GetAllEvents)
            .RequireAuthorization(p => p.RequireRole("Profesor"))
            .WithName("GetAllEvents")
            .Produces<List<EventDto>>(200);

        groupBuilder.MapGet("countries", GetCountries)
            .RequireAuthorization(p => p.RequireRole("Profesor"))
            .WithName("GetCountries")
            .Produces<List<CountryDto>>(200);

        groupBuilder.MapPost("countries", CreateCountry)
            .RequireAuthorization(p => p.RequireRole("Profesor"))
            .WithName("CreateCountry")
            .Produces<CountryDto>(201)
            .ProducesProblem(400);

        groupBuilder.MapGet("types", GetEventTypes)
            .RequireAuthorization(p => p.RequireRole("Profesor"))
            .WithName("GetEventTypes")
            .Produces<List<EventTypeDto>>(200);

        groupBuilder.MapPost("", CreateEvent)
            .RequireAuthorization(p => p.RequireRole("Profesor"))
            .WithName("CreateEvent")
            .Produces(201)
            .ProducesProblem(400);

        groupBuilder.MapPut("{id}", UpdateEvent)
            .RequireAuthorization(p => p.RequireRole("Profesor"))
            .WithName("UpdateEvent")
            .Produces(200)
            .ProducesProblem(400);

        groupBuilder.MapDelete("{id}", DeleteEvent)
            .RequireAuthorization(p => p.RequireRole("Profesor"))
            .WithName("DeleteEvent")
            .Produces(200)
            .ProducesProblem(400);
    }

    private async Task<IResult> GetMyEvents(ISender sender)
        => Results.Ok(await sender.Send(new GetMyEventsQuery()));

    private async Task<IResult> GetAllEvents(ISender sender)
        => Results.Ok(await sender.Send(new GetAllEventsQuery()));

    private async Task<IResult> GetCountries(ISender sender)
        => Results.Ok(await sender.Send(new GetCountriesQuery()));

    private async Task<IResult> CreateCountry(ISender sender, CreateCountryBody body)
    {
        var (result, country) = await sender.Send(new CreateCountryCommand(body.Name));

        if (!result.Succeeded)
            return Results.BadRequest(new { errors = result.Errors });

        return Results.Created($"/api/Events/countries/{country!.Id}", country);
    }

    private async Task<IResult> GetEventTypes(ISender sender)
        => Results.Ok(await sender.Send(new GetEventTypesQuery()));

    private async Task<IResult> CreateEvent(ISender sender, CreateEventBody body)
    {
        var (result, id) = await sender.Send(new CreateEventCommand
        {
            Name = body.Name,
            CountryId = body.CountryId,
            EventTypeId = body.EventType,
            Institutions = body.Institutions,
        });

        if (!result.Succeeded)
            return Results.BadRequest(new { errors = result.Errors });

        return Results.Created($"/api/Events/{id}", new { id });
    }

    private async Task<IResult> UpdateEvent(ISender sender, int id, UpdateEventBody body)
    {
        var result = await sender.Send(new UpdateEventCommand
        {
            Id = id,
            Name = body.Name,
            CountryId = body.CountryId,
            EventTypeId = body.EventType,
            Institutions = body.Institutions,
        });

        if (!result.Succeeded)
            return Results.BadRequest(new { errors = result.Errors });

        return Results.Ok(new { message = "Evento actualizado." });
    }

    private async Task<IResult> DeleteEvent(ISender sender, int id)
    {
        var result = await sender.Send(new DeleteEventCommand(id));

        if (!result.Succeeded)
            return Results.BadRequest(new { errors = result.Errors });

        return Results.Ok(new { message = "Evento eliminado." });
    }
}

public record CreateCountryBody(string Name);
public record CreateEventBody(string Name, int CountryId, int EventType, List<string> Institutions);
public record UpdateEventBody(string Name, int CountryId, int EventType, List<string> Institutions);
