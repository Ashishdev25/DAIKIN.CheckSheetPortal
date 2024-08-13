using DAIKIN.CheckSheetPortal.DTO;
using DAIKIN.CheckSheetPortal.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DAIKIN.CheckSheetPortal.API.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        protected readonly IAuthenticationService authService;
        public AuthController(IAuthenticationService authService)
        {
            this.authService = authService;
        }
        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDTO loginDto)
        {
            var result = await authService.LoginAsync(loginDto);

             return Ok(result);
        }
        [HttpPost]
        [Authorize]
        [Route("sign-out")]
        public async Task<IActionResult> SignOutAsync()
        {
            return Ok();
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("refresh-token")]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenDTO refreshTokenDTO)
        {
            var result = await authService.RefreshTokenAsync(refreshTokenDTO);

            if (result.Succeeded)
                return Ok(new { token = result.Data });

            return BadRequest();
        }
    }
}
