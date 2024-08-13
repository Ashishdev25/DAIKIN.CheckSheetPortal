using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAIKIN.CheckSheetPortal.DTO
{
    public class TokenDTO
    {
        public double expires_in { get; set; }

        public string access_token { get; set; }

        public string refresh_token { get; set; }
    }
    public class RefreshTokenDTO
    {
        public TokenDTO Token { get; set; }
    }
}
