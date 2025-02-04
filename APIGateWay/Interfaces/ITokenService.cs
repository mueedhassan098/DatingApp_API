using APIGateWay.Entities;

namespace APIGateWay.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(App_User user);
    }
}
