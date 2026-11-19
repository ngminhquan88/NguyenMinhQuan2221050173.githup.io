using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace StudentManagement.Authorization;

public class PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    public const string PolicyName = "DynamicPermission";

    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName == PolicyName)
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement())
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return base.GetPolicyAsync(policyName);
    }
}

public sealed class PermissionRequirement : IAuthorizationRequirement;