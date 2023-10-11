using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using App;
using DSP_API.Configurations.Filters;
using DSP_API.Models.Entity;
using DSP_API.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace DSP_API.Controllers
{
    [ApiController]
    public class BoxController : BaseController
    {
        private readonly DspApiContext _context;

        public BoxController(DspApiContext context)
        {
            _context = context;
        }
        [HttpPost]
        [IsLogin()]
        public async Task<IActionResult> CreateBox([FromForm] BoxCreate boxCreate, IEnumerable<IFormFile>? Files, IFormFile? Img)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            string imageBox = "";
            string slug = _let.GenerateSlug(boxCreate.Title) + _let.Random(6);
            if (Img != null)
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", _Username, slug, "avt");
                await UploadFile(Img, path);
                imageBox = $"Uploads/{_Username}/{slug}/avt/{Img.FileName}";
            }
            else
            {
                imageBox = "Uploads/Defaults/boxImage.png";
            }
            var newBox = new Box()
            {
                Title = boxCreate.Title,
                Content = boxCreate.Content,
                Pass = boxCreate.Pass,
                IsAvailable = boxCreate.IsAvailable,
                UserId = _UserId,
                DateCreated = DateTime.Now,
                View = 0,
                AdminBan = false,
                Url = slug,
                Img = imageBox
            };
            await _context.AddAsync(newBox);
            await _context.SaveChangesAsync();
            foreach (var f in Files)
            {

                var x = await UploadBoxFile(f, newBox.Id);
            }

            return Ok("1. Create successfully");
        }
        [HttpGet]
        [IsLogin()]
        public async Task<IActionResult> GetBoxsCurrentUser()
        {
            var boxs = await _context.Boxs.Where(b => b.UserId == _UserId).ToListAsync();
            return Ok(boxs);
        }

        [HttpGet]
        public async Task<IActionResult> GetDetailBox(int boxId, string? pass)
        {
            var box = await _context.Boxs.FirstOrDefaultAsync(b => b.Id == boxId);
            if (box == null)
            {
                return BadRequest("Box is not exists");
            }
            if (box.AdminBan == true)
            {
                return BadRequest("0. The box id baned by admin");
            }
            if (box.IsAvailable == false)
            {
                return BadRequest("0. The box is not available");
            }
            if (!string.IsNullOrEmpty(box.Pass))
            {
                if (box.Pass != pass)
                {
                    return BadRequest("0. Missing pass");
                }
            }
            return Ok(box);
        }
        [HttpPut]
        [IsLogin()]
        public async Task<IActionResult> UpdateBox(int Id, [FromForm] BoxCreate boxUpdate)
        {

            var updateBox = await _context.Boxs.Where(b => b.Id == Id).Include(b => b.User).FirstOrDefaultAsync();


            if (updateBox == null)
            {
                return BadRequest("0. The box is not exists");
            }

            var listUserShare = updateBox.Users.Select(u => new { u.Id, u.Username, u.Img, u.Name }).ToList();
            if (updateBox.UserId != _UserId && !listUserShare.Any(l => l.Id == _UserId) && !_UserRole.Contains("Edit"))
            {
                return BadRequest("0. You have not permission");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            updateBox.Content = boxUpdate.Content;
            updateBox.Title = boxUpdate.Title;
            updateBox.IsAvailable = boxUpdate.IsAvailable;
            updateBox.Pass = boxUpdate.Pass;


            _context.Update(updateBox);
            await _context.SaveChangesAsync();
            return Ok("1. Update thanh cong");
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Write your summary here")]
        private async Task<IActionResult> TestAsync()
        {
            var path = "C:\\Users\\ngocanh\\Pictures\\47000\\47000.png";
            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, "application/octet-stream", Path.GetFileName(path));
        }
        [HttpGet]
        public async Task<IActionResult> GetListFileInBox(int boxId)
        {
            var box = await _context.Boxs.FirstOrDefaultAsync(b => b.Id == boxId);

            if (box == null)
            {
                return BadRequest("0. Box is null");
            }
            if (box.IsAvailable == false || box.AdminBan == true)
            {
                return BadRequest("0. Box is not available");
            }
            var listFile = _context.Files.Where(f => f.BoxId == boxId).Select(lf => new{lf.Id,lf.Name,lf.Size,lf.View});
            return Ok(listFile);
        }
        [HttpGet]
        public async Task<IActionResult> DowloadFile(int boxId, string fileName)
        {
            var box = await _context.Boxs.Where(b => b.Id == boxId).Include(b => b.User).FirstOrDefaultAsync();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", box.User.Username, box.Url, fileName);
            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, "application/octet-stream", fileName);
        }
        [HttpGet]
        public async Task<IActionResult> DowloadAllFile(int boxId)
        {
            var box = await _context.Boxs.Where(b => b.Id == boxId).Include(b => b.User).FirstOrDefaultAsync();
            var listFile = new List<string>() { };
            var listFileSql = _context.Files.Where(f => f.BoxId == boxId);
            if (listFileSql == null)
            {
                return Content("Chưa có  file");
            }
            foreach (var file in listFileSql)
            {
                listFile.Add(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", box.User.Username, box.Url, file.Name));
            }
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (ZipArchive zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (string filePath in listFile)
                    {

                        string fileName = Path.GetFileName(filePath);
                        ZipArchiveEntry entry = zipArchive.CreateEntry(fileName);

                        using (Stream entryStream = entry.Open())
                        using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                        {
                            fileStream.CopyTo(entryStream);
                        }
                    }
                }

                memoryStream.Position = 0;

                // Create a new MemoryStream to return as the File content
                var resultStream = new MemoryStream(memoryStream.ToArray());

                // Return the ZIP archive as a downloadable file
                return File(resultStream, "application/zip", "files.zip");
            }
        }

        [HttpPost]
        private async Task<IActionResult> UploadFile(IFormFile file, string path)
        {
            var fileName = System.IO.Path.GetFileName(file.FileName);
            var filePath = Path.Combine(path, fileName);

            //Create directory
            Directory.CreateDirectory(path);

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

        [HttpPost]
        [IsLogin()]
        public async Task<IActionResult> UploadBoxFile(IFormFile file, int boxId)
        {
            var box = await _context.Boxs.Where(b => b.Id == boxId).Include(b => b.User).FirstOrDefaultAsync();

            if (box == null)
            {
                return BadRequest("0. The box is not exists");
            }
            var listUserShare = box.Users.Select(u => new { u.Id, u.Username, u.Img, u.Name }).ToList();
            if (box.UserId != _UserId && !listUserShare.Any(l => l.Id == _UserId) && !_UserRole.Contains("Edit"))
            {
                return BadRequest("0. You have not permission");
            }
            //add file on db
            var fileName = System.IO.Path.GetFileName(file.FileName);
            var newFile = new Models.Entity.File()
            {
                Name = fileName,
                Size = (decimal)((file.Length / 1024.0) / 1024.0),
                BoxId = boxId,
                View = 0
            };
            await _context.AddAsync(newFile);
            await _context.SaveChangesAsync();


            // add file in system
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", _Username, box.Url);
            var filePath = Path.Combine(path, fileName);
            //Create directory
            Directory.CreateDirectory(path);

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
        [HttpDelete]
        [IsLogin()]
        public async Task<IActionResult> DeleteFileAsync(int Id, int boxId, string fileName, string userName)
        {
            var box = await _context.Boxs.Where(b => b.Id == boxId).Include(b => b.User).FirstOrDefaultAsync();

            if (box == null)
            {
                return BadRequest("0. The box is not exists");
            }
            var listUserShare = box.Users.Select(u => new { u.Id, u.Username, u.Img, u.Name }).ToList();
            if (box.UserId != _UserId && !listUserShare.Any(l => l.Id == _UserId) && !_UserRole.Contains("Edit"))
            {
                return BadRequest("0. You have not permission");
            }
            var file = await _context.Files.FirstOrDefaultAsync(f => f.Id == Id);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", userName, box.Url, fileName);
            System.IO.File.Delete(path);
            _context.Files.Remove(file);
            await _context.SaveChangesAsync();
            return Ok("1. Delete successfully");

        }
        [HttpDelete]
        [IsLogin()]
        public async Task<IActionResult> DeleteBox(int boxId)
        {

            var box = await _context.Boxs.Where(b => b.Id == boxId).Include(b => b.User).FirstOrDefaultAsync();
            if (box == null)
            {
                return BadRequest("0. The box is not exitsts");
            }
            var listUserShare = box.Users.Select(u => new { u.Id, u.Username, u.Img, u.Name }).ToList();
            if (box.UserId != _UserId && !listUserShare.Any(l => l.Id == _UserId) && !_UserRole.Contains("Edit"))
            {
                return BadRequest("0. You have not permission");
            }
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", box.User.Username, box.Url);
            if (Directory.Exists(path))
            {
                System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(path);

                Empty(directory);
                Directory.Delete(path);
            }
            _context.Remove(box);
            await _context.SaveChangesAsync();
            return Ok("1. Delete successfully");

        }
        [HttpGet]
        [SwaggerOperation(Summary = "Get list user have been share to the box ")]
        public async Task<IActionResult> GetUserShareInBox(int boxId)
        {
            
            var box = await _context.Boxs.Where(b => b.Id == boxId).Include(b => b.Users).FirstOrDefaultAsync();
            if (box == null)
            {
                return BadRequest("0. box is null");
            }
            if(box.UserId != _UserId){
                return BadRequest("0. You have not permission");
            }
            var listUserShare = box.Users.Select(u => new { u.Id, u.Username, u.Img, u.Name }).ToList();
            return Ok(listUserShare);
        }

        [HttpGet]
        [IsLogin()]
        [SwaggerOperation(Summary = "Get list box have been share to the User")]
        public async Task<IActionResult> GetBoxShareCurrentUser()
        {
           
            var user = await _context.Users.Where(u => u.Id == _UserId).Include(u => u.BoxesNavigation).FirstOrDefaultAsync();
            var listBoxShare = user.BoxesNavigation.Select(bn => new {bn.Id,bn.Title,bn.Content,bn.Img,bn.IsAvailable,bn.AdminBan,bn.DateCreated,bn.View,bn.Url});
            return Ok(listBoxShare);
        }
        [HttpPost]
        [IsLogin()]
        public async Task<IActionResult> AddBoxShare(int boxId, int userId)
        {
            var box = await _context.Boxs.Where(b => b.Id == boxId).Include(b => b.Users).FirstOrDefaultAsync();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (box == null || user == null)
            {
                return BadRequest("0. Box or User is null|");
            }
            var listUserShare = box.Users.Select(u => new { u.Id, u.Username, u.Img, u.Name }).ToList();
            if (box.UserId != _UserId && !listUserShare.Any(l => l.Id == _UserId))
            {
                return BadRequest("0. You have not permission");
            }
            if (listUserShare.Any(l => l.Id == userId))
            {
                return BadRequest("0. User is already exists ");
            };
            box.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok("1. Successfully");
        }
        [HttpDelete]
        [IsLogin()]
        public async Task<IActionResult> RemoveBoxShare(int boxId, int userId)
        {
            var box = await _context.Boxs.Where(b => b.Id == boxId).Include(b => b.Users).FirstOrDefaultAsync();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (box == null || user == null)
            {
                return BadRequest("0. Box or User is null|");
            }
            var listUserShare = box.Users.Select(u => new { u.Id, u.Username, u.Img, u.Name }).ToList();
            if (box.UserId != _UserId && !listUserShare.Any(l => l.Id == _UserId))
            {
                return BadRequest("0. You have not permission");
            }
            if (!listUserShare.Any(l => l.Id == userId))
            {
                return BadRequest("0. User is not already exists ");
            };
            box.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok("1. Successfully");
        }


        private async Task<bool> IsInBoxShare(int boxId)
        {
            var listIdUser = await _context.Boxs.Where(b => b.Id == boxId).Include(b => b.Users).Select(b => b.User.Id).ToListAsync();
            if (listIdUser.Contains(_UserId))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        private void Empty(System.IO.DirectoryInfo directory)
        {
            foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
            foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }
    }
    public class BoxCreate
    {
        [Required(ErrorMessage = "Empty {0}")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Longer than {1} and smaller than {2}")]
        public string Title { get; set; }

        public string? Content { get; set; }

        public string? Pass { get; set; }

        public bool? IsAvailable { get; set; }

    }

}