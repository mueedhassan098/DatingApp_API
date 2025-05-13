using APIGateWay.Data;
using APIGateWay.Dtos;
using APIGateWay.Entities;
using APIGateWay.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace APIGateWay.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContextClass _dataContextClass;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AccountController(DataContextClass dataContextClass, ITokenService tokenService, IMapper mapper)
        {
            this._dataContextClass = dataContextClass;
            this._tokenService = tokenService;
            this._mapper = mapper;
        }
        [HttpPost("register")]//POST api/account/register?username=developer&password=pwd
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.Username))
            {
                return BadRequest("Username Is Already Taken");
            }
            var user = _mapper.Map<App_User>(registerDto);
            using var hmac = new HMACSHA512();

            user.UserName = registerDto.Username.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Code));
            user.PasswordSalt = hmac.Key;

            _dataContextClass.Users.Add(user);
            await _dataContextClass.SaveChangesAsync();
            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
                KnownAs = user.KnownAs
            };
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _dataContextClass.Users.Include(p => p.Photos)
                .FirstOrDefaultAsync(x =>
            x.UserName == loginDto.Username);
            if (user == null)
            {
                return Unauthorized("invalid Username");
            }
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedhash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
            for (int i = 0; i < computedhash.Length; i++)
            {
                if (computedhash[i] != user.PasswordHash[i])
                {
                    return Unauthorized("invalid password");
                }
            }
            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs= user.KnownAs
            };
        }
        private async Task<bool> UserExists(string username)
        {
            return await _dataContextClass.Users.AnyAsync(x => x.UserName == username);
        }
    }

}
