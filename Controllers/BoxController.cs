using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using App;
using AutoMapper;
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

        private readonly IMapper _mapper;
        public BoxController(DspApiContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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
                SharedStatus = boxCreate.SharedStatus,
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

            var boxDto = _mapper.Map<BoxDto>(newBox);
            return Ok(boxDto);
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


            if (box.SharedStatus == false)
            {
                var listUserShare = _context.BoxShares.Where(b => b.BoxId == box.Id).Select(b => b.UserId).ToArray();
                if (!listUserShare.Contains(_UserId) && !(box.UserId == _UserId))
                {
                    return BadRequest("0. The box is not available");

                }
            }
            box.View++;

            _context.Update(box);
            await _context.SaveChangesAsync();
            var boxDto = _mapper.Map<BoxDto>(box);
            // System.Console.WriteLine(boxDto.Title);
            return Ok(boxDto);
        }
        [HttpPut]
        [IsLogin()]
        public async Task<IActionResult> UpdateBox(int Id, [FromForm] BoxCreate boxUpdate, IFormFile? Img)
        {

            var updateBox = await _context.Boxs.Where(b => b.Id == Id).Include(b => b.User).FirstOrDefaultAsync();


            if (updateBox == null)
            {
                return BadRequest("0. The box is not exists");
            }

            if (updateBox.UserId != _UserId)
            {
                if (IsInShareEdit(updateBox))
                {
                    return BadRequest("You have not permission");
                }
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (Img != null)
            {
                var oldAvtName = updateBox.Img.Substring(updateBox.Img.LastIndexOf("/") +1);
                var deleteAvt = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads",  updateBox.User.Username,updateBox.Url, "avt",oldAvtName);
                try{
                    System.IO.File.Delete(deleteAvt);
                }catch{
                    _let.print("This box use default img");
                }
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", updateBox.User.Username, updateBox.Url, "avt");
                await UploadFile(Img, path);
                updateBox.Img = $"Uploads/{_Username}/{updateBox.Url}/avt/{Img.FileName}";
                
            }

            updateBox.Content = boxUpdate.Content;
            updateBox.Title = boxUpdate.Title;
            updateBox.SharedStatus = boxUpdate.SharedStatus;


            _context.Update(updateBox);
            await _context.SaveChangesAsync();

            return Ok("Success");
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

            if (box.AdminBan == true)
            {
                return BadRequest("0. Box is ban by admin");
            }
            if (box.SharedStatus == false)
            {
                if (!IsAuth(box.UserId) && !IsInShare(box))
                {
                    return BadRequest("0. Box is not available");

                }
            }
            var listFile = _context.Files.Where(f => f.BoxId == boxId).Select(lf => new { lf.Id, lf.Name, lf.Size, lf.View });
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
            if (!IsAuth(box.Id) && !IsInShareEdit(box))
            {
                return BadRequest("You have not permission!");
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
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", box.User.Username, box.Url);
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



        private void Empty(System.IO.DirectoryInfo directory)
        {
            foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
            foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }


        private bool IsAuth(int? possession)
        {
            if (possession == _UserId)
            {
                return true;
            }
            return false;
        }
        private bool IsInShareEdit(Box box)
        {
            var listUserShare = _context.BoxShares.Where(b => b.BoxId == box.Id).Select(b => b.UserId);
            if (listUserShare.Contains(_UserId))
            {
                var userShare = _context.BoxShares.FirstOrDefault(b => b.BoxId == box.Id && b.UserId == _UserId);
                if (userShare == null)
                {
                    return false;
                }
                if (userShare.EditAccess == true)
                {
                    return true;
                }
            }
            return false;
        }
        private bool IsInShare(Box box)
        {
            var listUserShare = _context.BoxShares.Where(b => b.BoxId == box.Id).Select(b => b.UserId);
            if (listUserShare.Contains(_UserId))
            {
                return true;
            }
            return false;
        }
    }
    public class BoxCreate
    {
        [Required(ErrorMessage = "Empty {0}")]
        [StringLength(200, MinimumLength = 6, ErrorMessage = "Longer than {1} and smaller than {2}")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Empty {0}")]
        public string? Content { get; set; }

        public bool? SharedStatus { get; set; }

    }

}