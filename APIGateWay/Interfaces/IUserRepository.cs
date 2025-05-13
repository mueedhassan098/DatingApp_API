using APIGateWay.Dtos;
using APIGateWay.Entities;
using APIGateWay.Helpers;

namespace APIGateWay.Interfaces
{
    public interface IUserRepository
    {
        void Update(App_User user);
        // Task<bool> SaveAllAsync(App_User user);
        Task<bool> SaveAllAsync();

        Task<IEnumerable<App_User>> GetUserAsync();
        Task<App_User> GetUserByIdAsync(int id);
        Task<App_User> GetUserByNameAsync(string username);
        Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams);
       // Task<IEnumerable<MemberDto>> GetMembersAsync();

        Task<MemberDto> GetMemberAsync(string username);
    }
}
