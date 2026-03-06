namespace Dashboard_v2.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(string userId, string userName, string email, IEnumerable<string> roles);
}
