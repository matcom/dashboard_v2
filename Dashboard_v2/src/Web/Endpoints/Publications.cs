using Dashboard_v2.Application.Publications.Commands.CreatePublication;
using Dashboard_v2.Application.Publications.Commands.DeletePublication;
using Dashboard_v2.Application.Publications.Commands.UpdatePublication;
using Dashboard_v2.Application.Publications.Queries.GetPublicationById;
using Dashboard_v2.Application.Publications.Queries.GetPublications;
using Dashboard_v2.Application.Publications.Queries.GetPublicationTypes;
using Dashboard_v2.Application.Common.Models;
using Dashboard_v2.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard_v2.Web.Endpoints;

public class Publications : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("types", GetTypes)
            .WithName("GetPublicationTypes")
            .Produces<List<PublicationTypeDto>>(200);

        groupBuilder.MapGet("", GetList)
            .WithName("GetPublications")
            .Produces<PaginatedList<PublicationDto>>(200);

        groupBuilder.MapGet("{id}", GetById)
            .WithName("GetPublicationById")
            .Produces<PublicationDetailDto>(200)
            .ProducesProblem(404);

        groupBuilder.MapPost("", Create)
            .WithName("CreatePublication")
            .Produces<int>(201)
            .ProducesProblem(400)
            .ProducesProblem(401);

        groupBuilder.MapPut("{id}", Update)
            .WithName("UpdatePublication")
            .Produces(204)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(401);

        groupBuilder.MapDelete("{id}", Delete)
            .WithName("DeletePublication")
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(401);
    }

    private async Task<IResult> GetTypes(ISender sender)
    {
        var types = await sender.Send(new GetPublicationTypesQuery());
        return Results.Ok(types);
    }

    private async Task<IResult> GetList(
        ISender sender,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        var result = await sender.Send(new GetPublicationsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search
        });
        return Results.Ok(result);
    }

    private async Task<IResult> GetById(ISender sender, int id)
    {
        var result = await sender.Send(new GetPublicationByIdQuery(id));
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private async Task<IResult> Create(ISender sender, CreatePublicationCommand command)
    {
        var id = await sender.Send(command);
        return Results.Created($"/api/Publications/{id}", id);
    }

    private async Task<IResult> Update(ISender sender, int id, UpdatePublicationCommand command)
    {
        if (id != command.Id) return Results.BadRequest("El ID de la ruta no coincide con el del cuerpo.");
        await sender.Send(command);
        return Results.NoContent();
    }

    private async Task<IResult> Delete(ISender sender, int id)
    {
        await sender.Send(new DeletePublicationCommand(id));
        return Results.NoContent();
    }
}
