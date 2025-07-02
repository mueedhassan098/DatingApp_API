using APIGateWay.Dtos;
using APIGateWay.Entities;
using APIGateWay.Extensions;
using APIGateWay.Helpers;
using APIGateWay.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace APIGateWay.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContextClass _dataContext;

        public LikesRepository(DataContextClass dataContext)
        {
            this._dataContext = dataContext;
        }
        public async Task<UserLike> GetUserLike(int sourceUserId, int targetUserId)
        {
           return await _dataContext.Likes.FindAsync(sourceUserId, targetUserId);
        }

        public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
        {
            var users=_dataContext.Users.OrderBy(u=>u.UserName).AsQueryable();
            var likes=_dataContext.Likes.AsQueryable();
            if (likesParams.Predicate == "liked")
            {
                likes=likes.Where(like=>like.SourceUserId == likesParams.UserId);
                users=likes.Select(like=>like.TargetUser);
            }
            if (likesParams.Predicate == "likedBy")
            {
                likes = likes.Where(like => like.TargetUserId == likesParams.UserId);
                users = likes.Select(like => like.SourceUser);
            }

            var likeUsers = users.Select(user => new LikeDto
            {
                UserName = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain).Url,
                City = user.City,
                Id = user.Id
            });
            return await PagedList<LikeDto>.CreateAsync(likeUsers,likesParams.PageSize,likesParams.PageNumber );
        }

        public async Task<App_User> GetUserWithLikes(int userId)
        {
            return await _dataContext.Users
                .Include(x => x.LikedUsers)
                .FirstOrDefaultAsync(x => x.Id == userId);
        }
    }
}
