﻿using APIGateWay.Entities;
using APIGateWay.Interfaces;
using APIGateWay.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIGateWay.Controllers
{
    public class AdminController : BaseApiController
    {
        private readonly UserManager<App_User> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoService _photoService;

        public AdminController(UserManager<App_User> userManager, IUnitOfWork unitOfWork, IPhotoService photoService)
        {
            this._userManager = userManager;
            this._unitOfWork = unitOfWork;
            this._photoService = photoService;
        }
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRole()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.UserName)
                .Select(u => new
                {
                    u.Id,
                    UserName=u.UserName.ToLower(),
                    Roles=u.UserRoles.Select(u => u.Role.Name).ToList()
                }).ToListAsync();
            return Ok(users);

        }


        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
        {
            if (string.IsNullOrEmpty(roles)) return BadRequest("You must select atleast one role");

            var selectedRoles = roles.Split(",").ToArray();
           // if (selectedRoles.Length == 0) return BadRequest("You must select atleast one role");
            var user = await _userManager.FindByNameAsync(username);

            if (user == null) return NotFound("User not found");

            var userRoles = await _userManager.GetRolesAsync(user);

            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if (!result.Succeeded) return BadRequest("Failed to add to roles");

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

            if (!result.Succeeded) return BadRequest("Failed to remove from roles");

            return Ok(await _userManager.GetRolesAsync(user));
        }



        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public async Task<ActionResult> GetPhotosForModeration()
        {
            var photos = await
           _unitOfWork.PhotoRepository.GetUnapprovedPhotos();
            return Ok(photos);
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("approve-photo/{photoId}")]
        public async Task<ActionResult> ApprovePhoto(int photoId)
        {
            var photo = await
           _unitOfWork.PhotoRepository.GetPhotoById(photoId);

            if (photo == null) return NotFound("Photo not found");

            photo.IsApproved = true;

            var user = await _unitOfWork.UserRepository.GetUserByPhotoId(photoId);
            if(!user.Photos.Any(x=>x.IsMain)) photo.IsMain = true;
            await _unitOfWork.Complete();
           // return Ok(new { message = " Photo Approved" });
            return Ok();

        }



        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("reject-photo/{photoId}")]
        public async Task<ActionResult> RejectPhoto(int photoId)
        {
            var photo = await
           _unitOfWork.PhotoRepository.GetPhotoById(photoId);
            if (photo.PublicId != null)
            {
                var result = await
               _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Result == "ok")
                {
                      _unitOfWork.PhotoRepository.RemovePhoto(photo);
                   // return BadRequest("Problem deleting photo from cloudinary");
                }
            }
            else
            {
                _unitOfWork.PhotoRepository.RemovePhoto(photo);
            }
            await _unitOfWork.Complete();
           // return Ok();

            return Ok();
        }
    }
}
