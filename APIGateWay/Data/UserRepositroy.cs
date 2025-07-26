using APIGateWay.Dtos;
using APIGateWay.Entities;
using APIGateWay.Helpers;
using APIGateWay.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace APIGateWay.Data
{
    public class UserRepositroy : IUserRepository
    {
        private readonly DataContextClass _context;
        private readonly IMapper _mapper;

        public UserRepositroy(DataContextClass context, IMapper mapper)
        {
            this._context = context;
            this._mapper = mapper;
        }

        public async Task<MemberDto> GetMemberAsync(string username, bool isCurrentUser)
        {
            var query = _context.Users
            .Where(x => x.UserName == username)
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .AsQueryable();
            if (isCurrentUser) query = query.IgnoreQueryFilters();
            return await query.FirstOrDefaultAsync();
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query = _context.Users.AsQueryable();

            query = query.Where(u => u.UserName != userParams.CurrentUsername);
            query = query.Where(u => u.Gender == userParams.Gender);

            var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));
            var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));

            query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(c => c.Created),
                _ => query.OrderByDescending(l => l.LastActive),
            };

            return await PagedList<MemberDto>.CreateAsync(
                query.AsNoTracking().ProjectTo<MemberDto>(_mapper.ConfigurationProvider),
                userParams.PageNumber,
                userParams.PageSize);
        }
        public async Task<IEnumerable<App_User>> GetUserAsync()
        {
            return await _context.Users.Include(p => p.Photos).ToListAsync();
        }

        public async Task<App_User> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<App_User> GetUserByNameAsync(string username)
        {
            return await _context.Users
                 .Include(p => p.Photos)
                 .SingleOrDefaultAsync(x => x.UserName == username);
        }

        public async Task<App_User> GetUserByPhotoId(int photoId)
        {
            return await _context.Users
                 .Include(p => p.Photos)
                 .IgnoreQueryFilters()
                 .Where(p => p.Photos.Any(p => p.Id == photoId))
                 .FirstOrDefaultAsync();
        }

        public async Task<string> GetUserGender(string username)
        {
            return await _context.Users
                .Where(x => x.UserName == username)
                .Select(x => x.Gender)
                .FirstOrDefaultAsync();
        }

        //public async Task<bool> SaveAllAsync()
        //{
        //    return await _context.SaveChangesAsync() > 0;
        //}


        public void Update(App_User user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }
    }
}
