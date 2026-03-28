using Dashboard_v2.Application.Users.Commands.AssignRole;
using Dashboard_v2.Application.Users.Commands.RemoveRole;
using Dashboard_v2.Application.Users.Queries.GetUsers;
using Dashboard_v2.Web.Infrastructure;

namespace Dashboard_v2.Web.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("", GetUsers)
            .RequireAuthorization(policy => policy.RequireRole("Administrator"))
            .WithName("GetUsers")
            .Produces<List<UserWithRolesDto>>(200);

        groupBuilder.MapPost("{userId}/roles", AssignRole)
            .RequireAuthorization(policy => policy.RequireRole("Administrator"))
            .WithName("AssignRole")
            .Produces(200)
            .ProducesProblem(400);

        groupBuilder.MapDelete("{userId}/roles/{roleName}", RemoveRole)
            .RequireAuthorization(policy => policy.RequireRole("Administrator"))
            .WithName("RemoveRole")
            .Produces(200)
            .ProducesProblem(400);
    }

    private async Task<IResult> GetUsers(ISender sender)
    {
        var users = await sender.Send(new GetUsersQuery());
        return Results.Ok(users);
    }

    private async Task<IResult> AssignRole(ISender sender, string userId, AssignRoleRequest body)
    {
        var result = await sender.Send(new AssignRoleCommand { UserId = userId, RoleName = body.RoleName });
        return result.Succeeded
            ? Results.Ok(new { message = "Rol asignado correctamente." })
            : Results.BadRequest(new { errors = result.Errors });
    }

    private async Task<IResult> RemoveRole(ISender sender, string userId, string roleName)
    {
        var result = await sender.Send(new RemoveRoleCommand { UserId = userId, RoleName = roleName });
        return result.Succeeded
            ? Results.Ok(new { message = "Rol eliminado correctamente." })
            : Results.BadRequest(new { errors = result.Errors });
    }
}

public record AssignRoleRequest(string RoleName);
