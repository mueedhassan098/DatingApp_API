using APIGateWay.Dtos;
using APIGateWay.Entities;
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

        public UserRepositroy(DataContextClass context,IMapper mapper)
        {
            this._context = context;
            this._mapper = mapper;
        }

        public async Task<MemberDto> GetMemberAsync(string username)
        {
           return await _context.Users
                .Where(x=>x.UserName == username)
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
                
        }

        public async Task<IEnumerable<MemberDto>> GetMembersAsync()
        {
            return await _context   .Users.ProjectTo<MemberDto>(_mapper.ConfigurationProvider).ToListAsync();
        }

        public async Task<IEnumerable<App_User>> GetUserAsync()
        {
            return await _context.Users.Include(p=>p.Photos).ToListAsync(); 
        }

        public async Task<App_User> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id); 
        }

        public async Task<App_User> GetUserByNameAsync(string username)
        {
           return await _context.Users
                .Include(p=>p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == username);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }


        public void Update(App_User user)
        {
           _context.Entry(user).State = EntityState.Modified;
        }
    }
}
