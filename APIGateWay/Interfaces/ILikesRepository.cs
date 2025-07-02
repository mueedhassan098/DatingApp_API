using APIGateWay.Dtos;
using APIGateWay.Entities;
using APIGateWay.Helpers;

namespace APIGateWay.Interfaces
{
    public interface ILikesRepository
    {
        Task<UserLike> GetUserLike(int sourceUserId, int targetUserId);
        Task<App_User> GetUserWithLikes(int userId);
        Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams);
    }
}
