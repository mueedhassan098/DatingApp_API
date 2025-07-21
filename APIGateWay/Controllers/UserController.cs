using APIGateWay.Data;
using APIGateWay.Dtos;
using APIGateWay.Entities;
using APIGateWay.Extensions;
using APIGateWay.Helpers;
using APIGateWay.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIGateWay.Controllers
{
    [Authorize]
    public class UserController : BaseApiController //ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UserController(IUnitOfWork uow, IMapper mapper, IPhotoService photoService)
        {
            this._uow = uow;
            this._mapper = mapper;
            this._photoService = photoService;
        }
        [HttpGet]
        public async Task<ActionResult<PagedList<MemberDto>>> GetUsers([FromQuery] UserParams prams)
        {
            // var Users = await _userRepository.GetMembersAsync(prams);

            var gender = await _uow.UserRepository.GetUserGender(User.GetUsername());
            prams.CurrentUsername = User.GetUsername();
            if (string.IsNullOrEmpty(prams.Gender))
            {
                prams.Gender = gender == "male" ? "female" : "male";
            }
            var Users = await _uow.UserRepository.GetMembersAsync(prams);
            Response.AddPaginationHeader(new PaginationHeader(Users.CurrentPage,
                Users.PageSize, Users.TotalCount, Users.TotalPages));
            return Ok(Users);
        }
        //[Authorize(Roles ="Member")]
        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            //return await _uow.UserRepository.GetMemberAsync(username);
            var currentUsername = User.GetUsername();
            return await _uow.UserRepository.GetMemberAsync(username,
                           isCurrentUser: currentUsername == username
            );
        }
        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto member)
        {
            var user = await _uow.UserRepository.GetUserByNameAsync(User.GetUsername());
            if (user == null) return NotFound();
            _mapper.Map(member, user);
            if (await _uow.Complete()) return NoContent();
            return BadRequest("Failed to update user ");
        }
        //[HttpPost("add-photo")]
        //public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        //{
        //    var user = await _uow.UserRepository.GetUserByNameAsync(User.GetUsername());

        //    if (user == null) return NotFound();

        //    var result = await _photoService.AddPhotoAsync(file);

        //    if(result.Error!=null)
        //    {
        //        return BadRequest(result.Error.Message);
        //    }
        //    var photo = new Photo
        //    {
        //        Url = result.SecureUrl.AbsoluteUri,
        //        PublicId = result.PublicId,
        //    };
        //    if(user.Photos.Count==0)
        //    {
        //        photo.IsMain = true;
        //    }
        //    user.Photos.Add(photo);

        //    if(await _uow.Complete())
        //    {
        //        return CreatedAtAction(nameof(GetUser),
        //             new { username = user.UserName }, _mapper.Map<PhotoDto>(photo));
        //    }
        //    return BadRequest("Problem Adding Photo");

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await
           _uow.UserRepository.GetUserByNameAsync(User.GetUsername());
            var result = await _photoService.AddPhotoAsync(file);
            if (result.Error != null) return BadRequest(result.Error.Message);
            var photo = new Photo
            {
            Url= result.SecureUrl.AbsoluteUri,
            PublicId= result.PublicId
            };
            user.Photos.Add(photo);
            if (await _uow.Complete())
            {
                return CreatedAtRoute("GetUser", new
                {
                    username =
               user.UserName
                }, _mapper.Map<PhotoDto>(photo));
            }
            return BadRequest("Problem addding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<IActionResult> SetMainPhoto(int photoId)
        {
            var user = await _uow.UserRepository.GetUserByNameAsync(User.GetUsername());

            if (user == null) return NotFound();

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null) return NotFound();

            if (photo.IsMain)
            {
                return BadRequest("this is already main photo");
            }

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

            if (currentMain != null) currentMain.IsMain = false;

            photo.IsMain = true;

            if (await _uow.Complete()) return NoContent();

            return BadRequest("Problem setting the main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<IActionResult> DeletePhoto(int photoId)
        {
            var user = await
                _uow.UserRepository.GetUserByNameAsync(User.GetUsername());
            var photo = await
                _uow.PhotoRepository.GetPhotoById(photoId);

            //var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if (photo == null) return NotFound();
            //if (photo.IsMain) return BadRequest("You cannot delete your main photo");
            //if (photo.PublicId != null)
            //{
            //    var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            //    if (result.Error != null)
            //    {
            //        return BadRequest(result.Error.Message);
            //    }
            //}
            user.Photos.Remove(photo);
            if (await _uow.Complete()) return Ok();
            return BadRequest("Problem deleting photo");
        }
    }
}
