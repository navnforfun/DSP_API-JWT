using System.ComponentModel.DataAnnotations;
using App;
using DSP_API.Models.Entity;
using DSP_API.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DSP_API.Controllers
{

    [ApiController]
    public class AccountController : BaseController
    {
        private readonly DspApiContext _context;

        public AccountController(DspApiContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] AccountLogin accountLogin)
        {
            var userLogin = await _context.Users.Where(u => u.Username == accountLogin.UserName).Include(u => u.Roles).FirstOrDefaultAsync();
            if (userLogin == null)
            {
                return BadRequest("0. Khong co user");
            }
            if (userLogin.Password != accountLogin.PassWord)
            {

                return BadRequest("0. sai pass");
            }
            if(userLogin.BanEnabled == true){
                return BadRequest("0. The account was baned by admin!");
            }
            _UserId = userLogin.Id;
            _Username = userLogin.Username;
            _UserRole = string.Concat(userLogin.Roles.Select(x => x.Name).ToList());
            _let.print("Login ok - " + _UserId + " - " + _Username + " - " + _UserRole);
            return Ok("1. Login ok - " + _UserId + " - " + _Username + " - " + _UserRole);
        }
        [HttpPost]
        public IActionResult Register([FromBody] AccountRegister accountRegister)
        {
            if (!_let.CheckNumberLetter(accountRegister.UserName) || !_let.CheckNumberLetter(accountRegister.PassWord))
            {
                return BadRequest("0. only letter and number are accept");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (accountRegister.PassWord != accountRegister.RePass)
            {
                return BadRequest("0. Repass is different from");
            }
            if (_context.Users.Any(u => u.Username == accountRegister.UserName))
            {
                return BadRequest("0. The username is already exists");

            }
            var newUser = new User()
            {
                Username = accountRegister.UserName,
                Name = accountRegister.UserName,
                Img = "/Uploads/Defaults/avtuser.jpg",
                Password = accountRegister.PassWord,
                BanEnabled = false
            };
            _context.Add(newUser);
            _context.SaveChanges();
            return Ok(newUser);
        }
        [HttpGet]
        public IActionResult Logout()
        {
            _UserId = 0;
            _Username = "";
            _UserRole = "";
            return Ok("1. Logout success");
        }

    }



    public class AccountLogin
    {
        public string UserName { get; set; }
        public string PassWord { get; set; }
    }
    public class AccountRegister
    {
        [Required(ErrorMessage = "Empty {0}")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Longer than {1} and smaller than {2}")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Empty {0}")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Longer than {1} and smaller than {2}")]
        public string PassWord { get; set; }
        [Required(ErrorMessage = "Empty {0}")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Longer than {1} and smaller than {2}")]
        public string RePass { get; set; }
    }

}