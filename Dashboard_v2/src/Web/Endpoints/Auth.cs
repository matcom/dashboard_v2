using Dashboard_v2.Application.Auth.Commands.Login;
using Dashboard_v2.Application.Auth.Commands.Logout;
using Dashboard_v2.Application.Auth.Commands.Register;
using Dashboard_v2.Application.Auth.Queries.GetCurrentUser;
using Dashboard_v2.Web.Infrastructure;

namespace Dashboard_v2.Web.Endpoints;

public class Auth : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost("register", Register)
            .AllowAnonymous()
            .WithName("Register")
            .Produces(200)
            .ProducesProblem(400);

        groupBuilder.MapPost("login", Login)
            .AllowAnonymous()
            .WithName("Login")
            .Produces(200)
            .ProducesProblem(400);

        groupBuilder.MapPost("logout", Logout)
            .WithName("Logout")
            .Produces(200);

        groupBuilder.MapGet("me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .Produces<UserDto>(200)
            .ProducesProblem(401);
    }

    private async Task<IResult> Register(ISender sender, RegisterCommand command)
    {
        var result = await sender.Send(command);

        if (!result.Succeeded)
        {
            return Results.BadRequest(new { errors = result.Errors });
        }

        return Results.Ok(new { message = "Usuario registrado exitosamente." });
    }

    private async Task<IResult> Login(ISender sender, LoginCommand command, HttpContext httpContext)
    {
        var (result, response) = await sender.Send(command);

        if (!result.Succeeded)
        {
            return Results.BadRequest(new { errors = result.Errors });
        }

        // El usuario tiene múltiples roles y debe seleccionar uno antes de obtener la cookie.
        if (response!.RequiresRoleSelection)
        {
            return Results.Ok(new { requiresRoleSelection = true, availableRoles = response.AvailableRoles });
        }

        // Login completo: guardar el token en una cookie HttpOnly.
        // Secure = true en HTTPS, SameSite=Strict evita ataques CSRF de otros orígenes.
        httpContext.Response.Cookies.Append("access_token", response.Token!, new CookieOptions
        {
            HttpOnly = true,
            Secure = httpContext.Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(8)
        });

        return Results.Ok(new { message = "Login exitoso." });
    }

    private async Task<IResult> Logout(ISender sender, HttpContext httpContext)
    {
        await sender.Send(new LogoutCommand());

        httpContext.Response.Cookies.Delete("access_token");

        return Results.Ok(new { message = "Sesión cerrada exitosamente." });
    }

    private async Task<IResult> GetCurrentUser(ISender sender)
    {
        var user = await sender.Send(new GetCurrentUserQuery());

        if (user == null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(user);
    }
}
