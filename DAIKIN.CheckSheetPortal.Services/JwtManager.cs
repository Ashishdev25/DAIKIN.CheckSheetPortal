using DAIKIN.CheckSheetPortal.DTO;
using DAIKIN.CheckSheetPortal.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DAIKIN.CheckSheetPortal.Services
{
    public class JwtManager
    {
        private readonly JwtOptions jwtOptions;
        public JwtManager(IOptions<JwtOptions> jwtOptions)
        {
            this.jwtOptions = jwtOptions.Value;
        }
        public TokenDTO GenerateToken<TUser>(TUser user) where TUser : UserDTO
        {
            var token = new TokenDTO
            {
                expires_in = jwtOptions.AccessValidFor.TotalMilliseconds,
                access_token = CreateToken(user, jwtOptions.AccessExpiration, jwtOptions.AccessSigningCredentials),
                refresh_token = CreateToken(user, jwtOptions.RefreshExpiration, jwtOptions.RefreshSigningCredentials)
            };

            return token;
        }
        private string CreateToken<TUser>(TUser user, DateTime expiration, SigningCredentials credentials) where TUser : UserDTO
        {
            var identity = GenerateClaimsIdentity(user);
            var jwt = new JwtSecurityToken(
                issuer: jwtOptions.Issuer,
                audience: jwtOptions.Audience,
                claims: identity.Claims,
                notBefore: jwtOptions.NotBefore,
                expires: expiration,
                signingCredentials: credentials);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }
        private ClaimsIdentity GenerateClaimsIdentity<TUser>(TUser user) where TUser : UserDTO
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("name", user.FullName),
                new Claim("plant", user.Plant ?? ""),
                new Claim("department", user.Department ?? ""),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };
            return new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
        }
        public ClaimsPrincipal GetPrincipal(string token, bool isAccessToken = true)
        {
            var key = new SymmetricSecurityKey(isAccessToken ? jwtOptions.AccessSecret : jwtOptions.RefreshSecret);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }
}
