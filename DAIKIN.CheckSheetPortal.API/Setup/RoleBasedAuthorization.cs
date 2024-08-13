using Microsoft.AspNetCore.Authorization;

namespace DAIKIN.CheckSheetPortal.API.Setup
{
    public static class RoleBasedAuthorization
    {
        public static AuthorizationPolicy RequireRoleAccess(params string[] roles)
        {
            var policyBuilder = new AuthorizationPolicyBuilder();
            policyBuilder.RequireAuthenticatedUser();
            policyBuilder.RequireRole(roles);
            return policyBuilder.Build();
        }
    }
}
