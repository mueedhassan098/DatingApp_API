using APIGateWay.Entities;
using APIGateWay.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace APIGateWay.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _Key;
        private readonly UserManager<App_User> _userManager;

        public TokenService(IConfiguration config,UserManager<App_User> userManager)
        {
            _Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
            this._userManager = userManager;
        }
        public async Task<string> CreateToken(App_User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId,user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName,user.UserName)

            };
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

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
