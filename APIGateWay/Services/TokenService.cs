using APIGateWay.Entities;
using APIGateWay.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace APIGateWay.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _Key;
        public TokenService(IConfiguration config)
        {
            _Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));  
        }
        public string CreateToken(App_User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId,user.UserName)
            };
            var creds = new SigningCredentials(_Key, SecurityAlgorithms.HmacSha512Signature);
            var tokendescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(10),
                SigningCredentials = creds
            };
            var tokenhandler = new JwtSecurityTokenHandler();
            var token=tokenhandler.CreateToken(tokendescriptor);
            return tokenhandler.WriteToken(token); 
        }
    }
}
