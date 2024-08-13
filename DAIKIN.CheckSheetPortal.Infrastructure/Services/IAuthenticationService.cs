using DAIKIN.CheckSheetPortal.DTO;
using DAIKIN.CheckSheetPortal.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAIKIN.CheckSheetPortal.Infrastructure.Services
{
    public interface IAuthenticationService
    {
        Task<IOperationResponse> LoginAsync(LoginDTO loginDto);
        Task<AuthResult<TokenDTO>> RefreshTokenAsync(RefreshTokenDTO refreshTokenDto);
    }
}
