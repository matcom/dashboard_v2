using Dashboard_v2.Application.Common.Exceptions;
using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Security;
using Dashboard_v2.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Application.Users.Commands.ToggleUserActive;

[Authorize(SystemPermission = SystemPermissions.ManageUsers)]
public record ToggleUserActiveCommand(string UserId, bool IsActive) : IRequest;

public class ToggleUserActiveCommandHandler : IRequestHandler<ToggleUserActiveCommand>
{
    private readonly IApplicationDbContext _context;

    public ToggleUserActiveCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ToggleUserActiveCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        user.IsActive = request.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
