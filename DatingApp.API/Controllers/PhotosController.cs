
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [Route("api/users/{userId}/photos")]
    [ApiController]
    [Authorize]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        public PhotosController(IDatingRepository repo, IMapper mapper, IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _cloudinaryConfig = cloudinaryConfig;
            _mapper = mapper;
            _repo = repo;

            Account acc = new Account(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);

        }

        [HttpGet("{id}",Name="GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo = await _repo.GetPhoto(id);
            var photo = _mapper.Map<PhotoForReturnDto>(photoFromRepo);
            return Ok(photo);

        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm]PhotoForCreationDto photoForCreationDto)
        {
            // Check if user who is authorized is updating their own profile
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var userFromRepo = await _repo.GetUser(userId);

            var file = photoForCreationDto.File;

            var uploadResult = new ImageUploadResult();

            if(file.Length > 0)
            {
                using(var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };

                    uploadResult = _cloudinary.Upload(uploadParams);
                }

                photoForCreationDto.PublicId = uploadResult.PublicId;
                photoForCreationDto.Url = uploadResult.Uri.ToString();

                var photo = _mapper.Map<Photo>(photoForCreationDto);

                if(!userFromRepo.Photos.Any(u => u.IsMain))
                {
                    photo.IsMain = true;
                }

                userFromRepo.Photos.Add(photo);

                if(await _repo.SaveAll())
                {
                    var photoReturn = _mapper.Map<PhotoForReturnDto>(photo);
                    return CreatedAtRoute("GetPhoto", new {id = photo.Id, userId = photo.UserId},photoReturn);
                }

            }
            else
            {
                return BadRequest("Photo cannot be uploaded. Invalid File.");
            }

            return BadRequest("Photo could not be uploaded");
        }

        [HttpPost("{id}/SetMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            // Check if user who is authorized is updating their own profile
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var user = await _repo.GetUser(userId);

            // Check if the photo belongs to the user
            if(!user.Photos.Any(p => p.Id == id))
            {
                return Unauthorized();
            }

            var photoFromRepo = await _repo.GetPhoto(id);

            if(photoFromRepo.IsMain)
            {
                return BadRequest("The photo is already set as main");
            }


            var currentMainPhoto = await _repo.GetMainPhotoForUser(userId);

            currentMainPhoto.IsMain = false;

            photoFromRepo.IsMain = true;

            if(await _repo.SaveAll())
            {
                return NoContent();
            }


            return BadRequest("Photo cannot be set as main");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto (int userId, int id)
        {
            // Check if user who is authorized is updating their own profile
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var user = await _repo.GetUser(userId);

            // Check if the photo belongs to the user
            if(!user.Photos.Any(p => p.Id == id))
            {
                return Unauthorized();
            }

            var photoFromRepo = await _repo.GetPhoto(id);

            if(photoFromRepo.IsMain)
            {
                return BadRequest("Cannot delete the main photo.");
            }

            

            if(!string.IsNullOrEmpty(photoFromRepo.PublicId))
            {
                var deleteParams = new DeletionParams(photoFromRepo.PublicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);
                if(result.Result == "ok")
                {
                    _repo.Delete(photoFromRepo);
                }
            }
            else
            {
                _repo.Delete(photoFromRepo);
            }

            if(await _repo.SaveAll())
            {
                return Ok();
            }
            
            return BadRequest("Cannot delete photo.");
        }

    }
}