using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;

namespace Dashboard_v2.Application.Authors.Commands.LinkAuthorToUser;

/// <summary>
/// Vincula un <see cref="Dashboard_v2.Domain.Entities.Author"/> existente (sin cuenta) al usuario
/// actualmente autenticado.<br/>
/// Verifica que: el autor existe, no está ya vinculado a ningún usuario y el usuario tampoco
/// tiene ya un perfil de autor propio.
/// </summary>
public record LinkAuthorToUserCommand(string AuthorId) : IRequest<Result>;

public class LinkAuthorToUserCommandHandler : IRequestHandler<LinkAuthorToUserCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public LinkAuthorToUserCommandHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(LinkAuthorToUserCommand request, CancellationToken cancellationToken)
    {
        // El usuario no debería ya tener un Author vinculado
        var userAlreadyLinked = await _context.Authors
            .AnyAsync(a => a.UserId == _currentUser.Id, cancellationToken);

        if (userAlreadyLinked)
            return Result.Failure(["Ya tienes un perfil de autor vinculado a tu cuenta."]);

        var author = await _context.Authors
            .FirstOrDefaultAsync(a => a.Id == request.AuthorId, cancellationToken);

        if (author == null)
            return Result.Failure(["Autor no encontrado."]);

        if (author.UserId != null)
            return Result.Failure(["Este autor ya está vinculado a otra cuenta."]);

        author.UserId = _currentUser.Id;
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
