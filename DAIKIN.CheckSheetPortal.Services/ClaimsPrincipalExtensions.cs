using DAIKIN.CheckSheetPortal.DTO;
using System.Security.Claims;

namespace DAIKIN.CheckSheetPortal.Services
{
    public static class ClaimsPrincipalExtensions
    {
        public static UserDTO GetUser(this ClaimsPrincipal principal)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            var id = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var name = principal.FindFirst("name")?.Value;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            var role = principal.FindFirst(ClaimTypes.Role).Value;
            var plant = principal.FindFirst("plant")?.Value;
            var department = principal.FindFirst("department")?.Value;
            return new UserDTO
            {
                Id = id,
                Email = email,
                FullName = name,
                PhoneNumber = "",
                Role = role,
                Plant = plant,
                Department = department
            };
        }
    }
}
