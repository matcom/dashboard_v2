namespace Dashboard_v2.Application.Common.Models;

public record LoginResponse
{
    public bool RequiresRoleSelection { get; init; }
    public IEnumerable<string> AvailableRoles { get; init; } = [];
    public string? Token { get; init; }
}
