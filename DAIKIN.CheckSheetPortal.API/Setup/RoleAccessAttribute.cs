using Microsoft.AspNetCore.Authorization;

namespace DAIKIN.CheckSheetPortal.API.Setup
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RoleAccessAttribute : AuthorizeAttribute
    {
        public RoleAccessAttribute(params string[] roles)
        {
            Roles = string.Join(",", roles);
        }
    }
}
