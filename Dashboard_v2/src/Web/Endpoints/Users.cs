using Dashboard_v2.Application.Users.Commands.CreateUser;
using Dashboard_v2.Application.Users.Commands.GrantPermission;
using Dashboard_v2.Application.Users.Commands.GrantSystemPermission;
using Dashboard_v2.Application.Users.Commands.RevokePermission;
using Dashboard_v2.Application.Users.Commands.RevokeSystemGrant;
using Dashboard_v2.Application.Users.Commands.ToggleUserActive;
using Dashboard_v2.Application.Users.Commands.UpdateUserRoles;
using Dashboard_v2.Application.Users.Queries.GetRoles;
using Dashboard_v2.Application.Users.Queries.GetUserResourceGrants;
using Dashboard_v2.Application.Users.Queries.GetUserSystemGrants;
using Dashboard_v2.Application.Users.Queries.GetUsers;
using Dashboard_v2.Web.Infrastructure;

namespace Dashboard_v2.Web.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("", GetUsers)
            .WithName("GetUsers")
            .Produces<List<UserListDto>>(200);

        groupBuilder.MapGet("roles", GetRoles)
            .WithName("GetRoles")
            .Produces<List<RoleDto>>(200);

        groupBuilder.MapGet("system-permissions", GetSystemPermissions)
            .WithName("GetSystemPermissions")
            .Produces<List<object>>(200);

        groupBuilder.MapGet("{userId}/grants", GetGrants)
            .WithName("GetUserGrants")
            .Produces<List<ResourceGrantDto>>(200);

        groupBuilder.MapGet("{userId}/system-grants", GetSystemGrants)
            .WithName("GetUserSystemGrants")
            .Produces<List<SystemGrantDto>>(200);

        groupBuilder.MapPost("", CreateUser)
            .WithName("CreateUser")
            .Produces(201)
            .ProducesProblem(400)
            .ProducesProblem(401);

        groupBuilder.MapPut("{userId}/roles", UpdateRoles)
            .WithName("UpdateUserRoles")
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(401);

        groupBuilder.MapPut("{userId}/active", ToggleActive)
            .WithName("ToggleUserActive")
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(401);

        groupBuilder.MapPost("grants", GrantPermission)
            .WithName("GrantPermission")
            .Produces<int>(201)
            .ProducesProblem(400)
            .ProducesProblem(401);

        groupBuilder.MapDelete("grants/{grantId}", RevokePermission)
            .WithName("RevokePermission")
            .Produces(204)
            .ProducesProblem(401);

        groupBuilder.MapPost("system-grants", GrantSystemPermission)
            .WithName("GrantSystemPermission")
            .Produces<int>(201)
            .ProducesProblem(400)
            .ProducesProblem(401);

        groupBuilder.MapDelete("system-grants/{grantId}", RevokeSystemGrant)
            .WithName("RevokeSystemGrant")
            .Produces(204)
            .ProducesProblem(401);
    }

    private async Task<IResult> GetUsers(ISender sender)
        => Results.Ok(await sender.Send(new GetUsersQuery()));

    private async Task<IResult> GetRoles(ISender sender)
        => Results.Ok(await sender.Send(new GetRolesQuery()));

    /// <summary>Devuelve la lista de todos los permisos de sistema disponibles con su etiqueta legible.</summary>
    private IResult GetSystemPermissions()
    {
        var perms = Dashboard_v2.Domain.Constants.SystemPermissions.AllPermissions
            .Select(p => new { key = p })
            .ToList();
        return Results.Ok(perms);
    }

    private async Task<IResult> GetGrants(ISender sender, string userId)
        => Results.Ok(await sender.Send(new GetUserResourceGrantsQuery(userId)));

    private async Task<IResult> GetSystemGrants(ISender sender, string userId)
        => Results.Ok(await sender.Send(new GetUserSystemGrantsQuery(userId)));

    private async Task<IResult> CreateUser(ISender sender, CreateUserCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(new { errors = result.Errors });
        return Results.Created("/api/Users", null);
    }

    private async Task<IResult> UpdateRoles(ISender sender, string userId, UpdateUserRolesCommand command)
    {
        if (userId != command.UserId)
            return Results.BadRequest("El userId de la ruta no coincide.");
        await sender.Send(command);
        return Results.NoContent();
    }

    private async Task<IResult> ToggleActive(ISender sender, string userId, ToggleActiveRequest body)
    {
        await sender.Send(new ToggleUserActiveCommand(userId, body.IsActive));
        return Results.NoContent();
    }

    private async Task<IResult> GrantPermission(ISender sender, GrantPermissionCommand command)
    {
        var grantId = await sender.Send(command);
        return Results.Created($"/api/Users/grants/{grantId}", grantId);
    }

    private async Task<IResult> RevokePermission(ISender sender, int grantId)
    {
        await sender.Send(new RevokePermissionCommand(grantId));
        return Results.NoContent();
    }

    private async Task<IResult> GrantSystemPermission(ISender sender, GrantSystemPermissionCommand command)
    {
        var grantId = await sender.Send(command);
        return Results.Created($"/api/Users/system-grants/{grantId}", grantId);
    }

    private async Task<IResult> RevokeSystemGrant(ISender sender, int grantId)
    {
        await sender.Send(new RevokeSystemGrantCommand(grantId));
        return Results.NoContent();
    }
}

public record ToggleActiveRequest(bool IsActive);
