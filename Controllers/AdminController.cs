using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DSP_API.Configurations.Filters;
using DSP_API.Models.Entity;
using DSP_API.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DSP_API.Controllers
{
    [ApiController]
    [IsLogin()]
    [IsEdit()]
    public class AdminController : BaseController
    {
        private readonly DspApiContext _context;
          private readonly IMapper _mapper;

        public AdminController(DspApiContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> GetListUser()
        {
            var listUser = await _context.Users.ToListAsync();
            var listUserDto = _mapper.Map<List<User>,List<UserDto>>(listUser);
            return Ok(listUserDto);
        }
        [HttpPut]
        public async Task<IActionResult> BanUser(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return BadRequest("0. User is null");
            }
            user.BanEnabled = !user.BanEnabled;
            await _context.SaveChangesAsync();
            return Ok("1. Ban revert successfully");
        }
        [HttpPut]
        public async Task<IActionResult> BanBox(int boxId)
        {
            var box = await _context.Boxs.FirstOrDefaultAsync(b => b.Id == boxId);
            if (box == null)
            {
                return BadRequest("0. Box is not exists");
            }
            box.AdminBan = !box.AdminBan;
            await _context.SaveChangesAsync();
            return Ok("0. BanBox successfully");
        }
        [HttpGet]
        public async Task<IActionResult> GetALLBox()
        {

            var boxs = await _context.Boxs.ToListAsync();
            var boxsDio = _mapper.Map<List<Box>,List<BoxDto>>(boxs);
            return Ok(boxsDio);
        }
        [HttpGet]
        public async Task<IActionResult> GetBoxsByIdUser(int Id)
        {
            if (!_UserRole.Contains("Edit"))
            {
                return BadRequest("0. You have not permisstion");
            }
            var boxs = await _context.Boxs.Where(b => b.UserId == Id).ToListAsync();
            return Ok(boxs);
        }


    }
}