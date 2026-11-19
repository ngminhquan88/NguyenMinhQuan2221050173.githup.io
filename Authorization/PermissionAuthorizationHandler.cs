using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Data;

namespace StudentManagement.Authorization;

public class PermissionAuthorizationHandler(ApplicationDbContext db)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        var descriptor = (context.Resource as HttpContext)?.GetEndpoint()?
            .Metadata.GetMetadata<ControllerActionDescriptor>();
        if (descriptor is null)
            return;

        var roles = context.User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray();
        if (roles.Length == 0)
            return;

        var permissionKey = $"{descriptor.ControllerName}.{descriptor.ActionName}";
        if (await db.RolePermissions.AnyAsync(permission =>
                roles.Contains(permission.RoleName) && permission.PermissionKey == permissionKey))
            context.Succeed(requirement);
    }
}