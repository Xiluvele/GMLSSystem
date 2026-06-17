using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace GMLSSystem.Tests.Integration
{
    public static class TestJwtHandler
    {
        public static string GenerateTestToken()
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("GLMSSystem2026SuperSecretJWTKeyForPart3Assignment123!"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("userId", "1")
            };

            var token = new JwtSecurityToken(
                issuer: "GLMSAPI",
                audience: "GLMSWebApp",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}