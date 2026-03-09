using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Security;
using Dashboard_v2.Domain.Constants;
using Dashboard_v2.Domain.Entities;
using FluentValidation.Results;
using ValidationException = Dashboard_v2.Application.Common.Exceptions.ValidationException;

namespace Dashboard_v2.Application.Users.Commands.CreateRole;

public record CreateRoleCommand(string Name) : IRequest<string>;

[Authorize(SystemPermission = SystemPermissions.CreateRoles)]
public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, string>
{
    private readonly IApplicationDbContext _context;

    public CreateRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrEmpty(name))
            throw new ValidationException([new ValidationFailure("Name", "El nombre del rol no puede estar vacío.")]);

        if (_context.Roles.Any(r => r.Name == name))
            throw new ValidationException([new ValidationFailure("Name", "Ya existe un rol con ese nombre.")]);

        var role = new Role { Id = Guid.NewGuid().ToString(), Name = name };
        _context.Roles.Add(role);
        await _context.SaveChangesAsync(cancellationToken);
        return role.Id;
    }
}
