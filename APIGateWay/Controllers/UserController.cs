using APIGateWay.Data;
using APIGateWay.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIGateWay.Controllers
{
    [Authorize]
    public class UserController :BaseApiController //ControllerBase
    {
        private readonly DataContextClass _context;

        public UserController(DataContextClass context)
        {
            _context = context;
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task< ActionResult<IEnumerable<App_User>>> GetUsers()
        {
            var users =await _context.Users.ToListAsync();
            return users;
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<App_User>> GetUser(int id) 
        {
            var user =await _context.Users.FindAsync(id);
            return user;
        }
    }
}
