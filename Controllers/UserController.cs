using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DSP_API.Configurations.Filters;
using DSP_API.Models.Entity;
using DSP_API.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DSP_API.Controllers
{
    [ApiController]
    public class UserController : BaseController
    {
        private readonly DspApiContext _context;

        public UserController(DspApiContext context)
        {
            _context = context;
        }
        [IsLogin()]
        [HttpGet]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == _UserId);
            var returnUser = new
            {
                UserName = user.Username,
                Img = user.Img,
                Name = user.Name,
                Description = user.Description,
                Job = user.JobTitle
            };
            return Ok(returnUser);
        }
        [IsLogin()]
        [HttpPut]
        public async Task<IActionResult> UpdateImgUser(IFormFile? Img)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == _UserId);
            if (user == null)
            {
                return BadRequest("0. Something went wrong");
            }
            if (Img == null)
            {
                return BadRequest("0. Img is null");
            }
            if (!Img.ContentType.Contains("image"))
            {
                return BadRequest("0. The file is not image");
            }
            if (user.Img != "/Uploads/Defaults/avtuser.jpg")
            {
                string pathOldImg = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.Img);
                await DeleteFileAsync(pathOldImg);
            }
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", _Username);
            var result = await UploadFile(Img, path);

            user.Img = $"Uploads/{_Username}/{Img.FileName}";
            await _context.SaveChangesAsync();

            return Ok("1. Successfully");
        }
        [IsLogin()]
        [HttpPut]
        public async Task<IActionResult> UpdateUserInfor([FromForm] UserInfo userInfo)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == _UserId);
            if (user == null)
            {
                return BadRequest("0. Something went wrong");
            }
            user.Name = userInfo.Name;
            user.JobTitle = userInfo.JobTitle;
            user.Description = userInfo.Description;
            await _context.SaveChangesAsync();
            return Ok("1. Successfully");
        }
        [HttpPut]
        [IsLogin()]
        public async Task<IActionResult> RePass([FromForm] RePass rePass)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == _UserId);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (user.Password != rePass.PassOld)
            {
                return BadRequest("0. You pass is wrong");
            }
            if (rePass.PassNew1 != rePass.PassNew2)
            {
                return BadRequest("0. Repass is wrong");
            }

            return Ok("1. Successfully");
        }
        private async Task<IActionResult> UploadFile(IFormFile file, string path)
        {

            var fileName = System.IO.Path.GetFileName(file.FileName);
            var filePath = Path.Combine(path, fileName);

            // Check If file with same name exists and delete it
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            using (var localFile = System.IO.File.OpenWrite(filePath))
            using (var uploadedFile = file.OpenReadStream())
            {
                uploadedFile.CopyTo(localFile);
            }
            return Ok("1. Success");
        }

        private async Task<IActionResult> DeleteFileAsync(string path)
        {
            System.IO.File.Delete(path);
            return Ok("1. Delete successfully");

        }

    }
    public class RePass
    {
        [Required(ErrorMessage = "Empty {0}")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Longer than {1} and smaller than {2}")]
        public string PassOld { get; set; }
        [Required(ErrorMessage = "Empty {0}")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Longer than {1} and smaller than {2}")]
        public string PassNew1 { get; set; }
        [Required(ErrorMessage = "Empty {0}")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Longer than {1} and smaller than {2}")]
        public string PassNew2 { get; set; }
    }
    public class UserInfo
    {
        public string? Name { get; set; }
        public string? JobTitle { get; set; }
        public string? Description { get; set; }
    }
}