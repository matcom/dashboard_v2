using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dashboard_v2.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Dashboard_v2.Infrastructure.Services;

/// <summary>
/// Servicio que genera tokens JWT firmados con HMAC-SHA256 (HS256).<br/>
/// La configuración se lee de <c>appsettings.json</c> bajo la sección <c>Jwt:</c>:
/// <c>Secret</c> (clave privada), <c>Issuer</c>, <c>Audience</c> y <c>ExpiryMinutes</c>.
/// </summary>
public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Genera un token JWT que identifica al usuario en cada petición.<br/>
    /// Claims incluidos: <c>sub</c> (userId), <c>unique_name</c> (userName), <c>email</c>,
    /// <c>jti</c> (UUID único del token), y uno o más claims <c>role</c>.<br/>
    /// El token está firmado con la clave secreta del servidor — el cliente no puede alterarlo
    /// sin invalidar la firma. La cookie HttpOnly evita que JavaScript lo lea.
    /// </summary>
    public string GenerateToken(string userId, string userName, string email, IEnumerable<string> roles)
    {
        var secret = _configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT Secret no configurado.");
        var issuer = _configuration["Jwt:Issuer"] ?? "Dashboard_v2";
        var audience = _configuration["Jwt:Audience"] ?? "Dashboard_v2";
        var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "1440");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.UniqueName, userName),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
