using DAIKIN.CheckSheetPortal.DTO;
using DAIKIN.CheckSheetPortal.Entities;
using DAIKIN.CheckSheetPortal.Infrastructure;
using DAIKIN.CheckSheetPortal.Infrastructure.Services;
using DAIKIN.CheckSheetPortal.Utils;
using System.Security.Claims;

namespace DAIKIN.CheckSheetPortal.Services
{
    public class AuthenticationService<TUser> : IAuthenticationService
            where TUser : User, new()
    {
        protected readonly IUserService userService;
        protected readonly JwtManager jwtManager;
        public AuthenticationService(IUserService _userService, JwtManager _jwtManager)
        {
            this.userService = _userService;
            this.jwtManager = _jwtManager;
        }
        public async Task<IOperationResponse> LoginAsync(LoginDTO loginDto)
        {
            if (loginDto == null || string.IsNullOrEmpty(loginDto.Login) || string.IsNullOrEmpty(loginDto.Password))
                return OperationResponse.Error(Common.Util.LoginIdAndPasswordRequired);
            var user = await userService.GetByEmailAsync(loginDto.Login);
            if (user != null)
            {
                if (user.Password == loginDto.Password)
                {
                    if (!user.IsActive) return OperationResponse.Error(Common.Util.UserNotActiveContactAdministrator);
                    var token = jwtManager.GenerateToken(user);
                    return OperationResponse<TokenDTO>.Success(token);
                }
                else
                {
                    return OperationResponse.Error(Common.Util.IncorrectLoginIdOrPassword);
                }
            }
            else
            {
                return OperationResponse.Error(Common.Util.UserNotFound);
            }            
        }
        public async Task<AuthResult<TokenDTO>> RefreshTokenAsync(RefreshTokenDTO refreshTokenDto)
        {
            var refreshToken = refreshTokenDto?.Token?.refresh_token;
            if (string.IsNullOrEmpty(refreshToken)) return AuthResult<TokenDTO>.UnvalidatedResult;

            try
            {
                var principal = jwtManager.GetPrincipal(refreshToken, isAccessToken: false);
                var id = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var name = principal.FindFirst("name")?.Value;
                var plant = principal.FindFirst("plant")?.Value;
                var department = principal.FindFirst("department")?.Value;
                var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                var phonenumber = principal.FindFirst("phonenumber")?.Value;
                var role = principal.FindFirst(ClaimTypes.Role)?.Value;
                var user = new UserDTO
                {
                    Id = id,
                    Email = email,
                    FullName = name,
                    PhoneNumber = phonenumber,
                    Role = role,
                    Plant = plant,
                    Department = department
                };

                if (user != null)
                {
                    var token = jwtManager.GenerateToken(user);
                    return AuthResult<TokenDTO>.TokenResult(token);
                }
            }
            catch
            {
                return AuthResult<TokenDTO>.UnauthorizedResult;
            }

            return AuthResult<TokenDTO>.UnauthorizedResult;
        }
    }
}
