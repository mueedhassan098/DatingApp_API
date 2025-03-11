using APIGateWay.Dtos;
using APIGateWay.Entities;

namespace APIGateWay.Interfaces
{
    public interface IUserRepository
    {
        void Update(App_User user);
        Task<bool> SaveAllAsync(App_User user);
        Task<IEnumerable<App_User>> GetUserAsync();
        Task<App_User> GetUserByIdAsync(int id);
        Task<App_User> GetUserByNameAsync(string username);
        Task<IEnumerable<MemberDto>> GetMembersAsync(); 
        Task<MemberDto> GetMemberAsync(string username)
    }
}
