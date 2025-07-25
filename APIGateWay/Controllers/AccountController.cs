﻿using APIGateWay.Data;
using APIGateWay.Dtos;
using APIGateWay.Entities;
using APIGateWay.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIGateWay.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContextClass _dataContextClass;

        private readonly UserManager<App_User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AccountController(UserManager<App_User> userManager, DataContextClass dataContextClass,
            ITokenService tokenService, IMapper mapper)
        {
            this._dataContextClass = dataContextClass;
            this._userManager = userManager;
            this._tokenService = tokenService;
            this._mapper = mapper;
        }
        [HttpPost("register")]//POST api/account/register?username=developer&password=pwd
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.Username)) return BadRequest("Username Is Already Taken");
       
            var user = _mapper.Map<App_User>(registerDto);
           
            user.UserName = registerDto.Username.ToLower();
            //_dataContextClass.Add(user);
            //await _dataContextClass.SaveChangesAsync();

            var result = await _userManager.CreateAsync(user, registerDto.Code);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "Member");

            if (!roleResult.Succeeded)
            {
                return BadRequest(roleResult.Errors);
            }

            return new UserDto
            {
                Username = user.UserName,
                Token =await _tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender= user.Gender,             

            };
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x =>x.UserName == loginDto.Username);

            if (user == null) return Unauthorized("invalid Username");

            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!result)
            {
                return Unauthorized("invalid password");
            }

            return new UserDto
            {
                Username = user.UserName,
                Token =await _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs= user.KnownAs,
                Gender= user.Gender         

            };
        }
        private async Task<bool> UserExists(string username)
        {
            return await _userManager.Users.AnyAsync(x => x.UserName == username.ToLower() );
        }
    }

}
