using System.Reflection;
using Dashboard_v2.Application.Common.Exceptions;
using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Security;

namespace Dashboard_v2.Application.Common.Behaviours;

public class AuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : notnull
{
    private readonly IUser _user;
    private readonly IIdentityService _identityService;
    private readonly IPermissionService _permissionService;

    public AuthorizationBehaviour(
        IUser user,
        IIdentityService identityService,
        IPermissionService permissionService)
    {
        _user = user;
        _identityService = identityService;
        _permissionService = permissionService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>();

        if (authorizeAttributes.Any())
        {
            // Must be authenticated user
            if (_user.Id == null)
            {
                throw new UnauthorizedAccessException();
            }

            // Role-based authorization
            var authorizeAttributesWithRoles = authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Roles));

            if (authorizeAttributesWithRoles.Any())
            {
                var authorized = false;

                foreach (var roles in authorizeAttributesWithRoles.Select(a => a.Roles.Split(',')))
                {
                    foreach (var role in roles)
                    {
                        var isInRole = _user.Roles?.Any(x => role.Trim() == x) ?? false;
                        if (isInRole)
                        {
                            authorized = true;
                            break;
                        }
                    }
                }

                if (!authorized)
                {
                    throw new ForbiddenAccessException();
                }
            }

            // Policy-based authorization
            var authorizeAttributesWithPolicies = authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Policy));
            if (authorizeAttributesWithPolicies.Any())
            {
                foreach (var policy in authorizeAttributesWithPolicies.Select(a => a.Policy))
                {
                    var authorized = await _identityService.AuthorizeAsync(_user.Id, policy);

                    if (!authorized)
                    {
                        throw new ForbiddenAccessException();
                    }
                }
            }

            // System-permission-based authorization
            // Each attribute with SystemPermission is checked independently (AND logic across attributes).
            // Within a single attribute, comma-separated permissions are OR logic.
            var attributesWithSystemPerms = authorizeAttributes
                .Where(a => !string.IsNullOrWhiteSpace(a.SystemPermission));

            foreach (var attr in attributesWithSystemPerms)
            {
                var permissionsInAttr = attr.SystemPermission
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                var authorized = false;
                foreach (var perm in permissionsInAttr)
                {
                    if (await _permissionService.HasSystemPermissionAsync(_user.Id, perm, cancellationToken))
                    {
                        authorized = true;
                        break;
                    }
                }

                if (!authorized)
                {
                    throw new ForbiddenAccessException();
                }
            }
        }

        // User is authorized / authorization not required
        return await next();
    }
}
