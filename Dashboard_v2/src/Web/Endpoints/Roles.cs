using Dashboard_v2.Application.Roles.Queries.GetRoles;
using Dashboard_v2.Web.Infrastructure;

namespace Dashboard_v2.Web.Endpoints;

public class Roles : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("", GetRoles)
            .RequireAuthorization(policy => policy.RequireRole("Superuser"))
            .WithName("GetRoles")
            .Produces<List<RoleDto>>(200);
    }

    private async Task<IResult> GetRoles(ISender sender)
    {
        var roles = await sender.Send(new GetRolesQuery());
        return Results.Ok(roles);
    }
}
