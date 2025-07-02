using APIGateWay.Entities;

namespace APIGateWay.Interfaces
{
    public interface ITokenService
    {
       Task<string> CreateToken(App_User user);
    }
}
