using APIGateWay.Data;
using APIGateWay.Dtos;
using APIGateWay.Entities;
using APIGateWay.Extensions;
using APIGateWay.Helpers;
using APIGateWay.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace APIGateWay.Controllers
{
    [Authorize]
    public class UserController :BaseApiController //ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UserController(IUserRepository userRepository,IMapper mapper,IPhotoService photoService)
        {           
            this._userRepository = userRepository;
            this._mapper = mapper;
            this._photoService = photoService;
        }
        [HttpGet]
        public async Task<ActionResult<PagedList<MemberDto>>> GetUsers([FromQuery] UserParams prams)
        {
           // var Users = await _userRepository.GetMembersAsync(prams);

            var currentUser = await _userRepository.GetUserByNameAsync(User.GetUsername());
            prams.CurrentUsername = currentUser.UserName;
            if (string.IsNullOrEmpty(prams.Gender))
            {
                prams.Gender = currentUser.Gender == "male" ? "female" : "male";
            }
            var Users = await _userRepository.GetMembersAsync(prams);
            Response.AddPaginationHeader(new PaginationHeader(Users.CurrentPage,
                Users.PageSize, Users.TotalCount, Users.TotalPages));
            return Ok(Users);
        }
        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username) 
        {
          return await _userRepository.GetMemberAsync(username);
           
        }
        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto member)
        {
            var user = await _userRepository.GetUserByNameAsync(User.GetUsername());
            if (user == null) return NotFound();
            _mapper.Map(member, user);
            if (await _userRepository.SaveAllAsync()) return NoContent();
            return BadRequest("Failed to update user ");
        }
        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await _userRepository.GetUserByNameAsync(User.GetUsername());

            if (user == null) return NotFound();

            var result = await _photoService.AddPhotoAsync(file);

            if(result.Error!=null)
            {
                return BadRequest(result.Error.Message);
            }
            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,
            };
            if(user.Photos.Count==0)
            {
                photo.IsMain = true;
            }
            user.Photos.Add(photo);

            if(await _userRepository.SaveAllAsync())
            {
                return CreatedAtAction(nameof(GetUser),
                     new { username = user.UserName }, _mapper.Map<PhotoDto>(photo));
            }
            return BadRequest("Problem Adding Photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<IActionResult> SetMainPhoto(int photoId)
        {
            var user = await _userRepository.GetUserByNameAsync(User.GetUsername());

            if (user == null) return NotFound();

            var photo=user.Photos.FirstOrDefault(x=>x.Id==photoId);

            if (photo == null) return NotFound();

            if(photo.IsMain)
            {
                return BadRequest("this is already main photo");
            }

            var currentMain=user.Photos.FirstOrDefault(x=>x.IsMain);

            if (currentMain != null) currentMain.IsMain = false;

            photo.IsMain=true;

            if(await _userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Problem setting the main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<IActionResult> DeletePhoto(int photoId)
        {
            var user = await _userRepository.GetUserByNameAsync(User.GetUsername());
            var photo=user.Photos.FirstOrDefault( x=>x.Id==photoId);
            if (photo == null) return NotFound();
            if (photo.IsMain) return BadRequest("You cannot delete your main photo");
            if (photo.PublicId != null)
            {
                var result= await _photoService.Update_OR_Delete_PhotoAsync(photo.PublicId);
                if (result.Error != null)
                {
                    return BadRequest(result.Error.Message);
                }
            }
            user.Photos.Remove(photo);
            if(await _userRepository.SaveAllAsync())  return Ok();
            return BadRequest("Problem deleting photo");
        }
    }
}
