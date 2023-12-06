using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DSP_API.Models.Entity;
using DSP_API.Util;
using DSP_API.Configurations.Filters;
using System.ComponentModel.DataAnnotations;

namespace DSP_API.Controllers
{
    [IsLogin()]
    [IsAdmin()]
    public class RolesController : BaseController
    {
        private readonly DspApiContext _context;

        public RolesController(DspApiContext context)
        {
            _context = context;
        }

        // GET: api/Roles
        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            if (_context.Roles == null)
            {
                return NotFound();
            }
            var roles = await _context.Roles.ToListAsync();
            return Ok(roles);
        }

        // GET: api/Roles/5
        [HttpGet()]
        public async Task<ActionResult> GetRole(int id)
        {
            if (_context.Roles == null)
            {
                return NotFound();
            }
            var role = await _context.Roles.FindAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            return Ok(role);
        }

        // PUT: api/Roles/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut()]
        public async Task<IActionResult> UpdateRole(int id, [FromForm] RoleCreate roleCreate)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id);
            if (role == null)
            {
                return BadRequest("Role is not exitst");
            };
            role.Name = roleCreate.Name;
    
            await _context.SaveChangesAsync();

            return Ok("Update successfully");
        }

        // POST: api/Roles
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult> CreateRole([FromForm] RoleCreate roleCreate)
        {
            if (_context.Roles == null)
            {
                return Problem("Entity set 'DspApiContext.Roles'  is null.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var role = new Role()
            {
                Name = roleCreate.Name,
            };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            // return CreatedAtAction("GetRole", new { id = role.Id }, role);
            return Ok("Create successfully");

        }

        // DELETE: api/Roles/5
        [HttpDelete()]
        public async Task<IActionResult> DeleteRole(int id)
        {
            if (_context.Roles == null)
            {
                return NotFound();
            }
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return Ok("Delete successfully");
        }
        [HttpGet]
        public async Task<IActionResult> GetRoleByUserId(int userId)
        {
            var user = await _context.Users.Where(u => u.Id == userId).Include(u => u.Roles).FirstOrDefaultAsync();
            if (user == null)
            {
                return BadRequest("Khong co user");
            }
            var listRole = user.Roles.Select(r => r.Name).ToList();
            return Ok(listRole);
        }
        [HttpPost]
        public async Task<IActionResult> CreateUserRole(int userId, int roleId)
        {
            var user = await _context.Users.Where(u => u.Id == userId).Include(u => u.Roles).FirstOrDefaultAsync();
            if (user == null)
            {
                return BadRequest("Khong co user");
            }
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);

            if (role == null)
            {
                return BadRequest("Role is null");
            }
            if (user.Roles.Contains(role))
            {
                return BadRequest("Role is exitst");
            }
            user.Roles.Add(role);
            await _context.SaveChangesAsync();
            return Ok("Add role successfully");
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteUserRole(int userId, int roleId)
        {
            var user = await _context.Users.Where(u => u.Id == userId).Include(u => u.Roles).FirstOrDefaultAsync();
            if (user == null)
            {
                return BadRequest("Khong co user");
            }
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);

            if (role == null)
            {
                return BadRequest("Role is null");
            }
            if (!user.Roles.Contains(role))
            {
                return BadRequest("Role is not exitst");
            }
            user.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return Ok("Remove role successfully");
        }
        private bool RoleExists(int id)
        {
            return (_context.Roles?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
    public class RoleCreate
    {
        [Required(ErrorMessage = ("Name can't be null"))]
        [MinLength(3)]
        public string Name { get; set; }
    };
}   