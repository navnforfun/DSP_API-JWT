using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using App;
using DSP_API.Models.Entity;
using DSP_API.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using App;
using DSP_API.Configurations.Filters;
using Microsoft.AspNetCore.Authorization;
using NuGet.Protocol;

namespace DSP_API.Controllers
{

    [ApiController]
    public class AccountController : BaseController
    {
        private readonly DspApiContext _context;
        private IConfiguration _config;
        IHttpContextAccessor _httpContextAccessor;



        public AccountController(DspApiContext context, IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _config = config;
        }
        [HttpGet]
        public IActionResult Test()
        {
            // System.Console.WriteLine(_Username);
            // System.Console.WriteLine(_UserId);
            // var username = _httpContextAccessor.HttpContext.User.Identity.Name;
            // var x =  _httpContextAccessor.HttpContext.User;
            var user = _httpContextAccessor.HttpContext.User;
            var userName = user.GetLoggedInUserName();
            var id = user.GetLoggedInUserId<int>();
            System.Console.WriteLine(id);
            //    var z =  HttpContext.User;
            return Ok("Ok thanh cong");
        }
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] AccountLogin accountLogin)
        {

            var userLogin = await _context.Users.Where(u => u.Username == accountLogin.UserName).Include(u => u.Roles).FirstOrDefaultAsync();
            if (userLogin == null)
            {
                return BadRequest("Khong co user");
                // return StatusCode(412,"Not valid");
            }
            if (userLogin.Password != accountLogin.PassWord)
            {

                return BadRequest("sai pass");
            }
            if (userLogin.BanEnabled == true)
            {
                return BadRequest("The account was baned by admin!");
            }
            var token = Generate(userLogin);
            _UserId = userLogin.Id;
            _Username = userLogin.Username;
            _UserRole = string.Concat(userLogin.Roles.Select(x => x.Name).ToList());
            _let.print("Login ok - " + _UserId + " - " + _Username + " - " + _UserRole);
            // return Ok("Login ok - " + _UserId + " - " + _Username + " - " + _UserRole + "\nToken:" +token);
            return Ok(token);
        }
        [HttpPost]
        [IsTime()]
        [AllowAnonymous]
        public async Task<IActionResult> LoginByToken()
        {

            var Username = HttpContext.User.GetLoggedInUserName();
            if(Username == null){
                return BadRequest("You token is not valid");
            }
            var tokenUser = await _context.Users.Where(u => u.Username == Username).Include(u => u.Roles).FirstOrDefaultAsync();
            if (tokenUser == null)
            {
                return BadRequest("Something went wrong when login by token user");
            }
            else
            {
                System.Console.WriteLine("Login by token ok");
                if (tokenUser == null)
                {
                    return BadRequest("Khong co user");
                }

                if (tokenUser.BanEnabled == true)
                {
                    return BadRequest("The account was baned by admin!");
                }

                _UserId = tokenUser.Id;
                _Username = tokenUser.Username;
                _UserRole = string.Concat(tokenUser.Roles.Select(x => x.Name).ToList());
                _let.print("Login ok - " + _UserId + " - " + _Username + " - " + _UserRole);
                // return Ok("Login ok - " + _UserId + " - " + _Username + " - " + _UserRole + "\nToken:" +token);
                return Ok(Generate(tokenUser));
            }

        }
        [HttpPost]
        public IActionResult Register([FromBody] AccountRegister accountRegister)
        {
            System.Console.WriteLine("da vo");
            if (!_let.CheckNumberLetter(accountRegister.UserName) || !_let.CheckNumberLetter(accountRegister.PassWord))
            {
                return BadRequest("only letter and number are accept");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (accountRegister.PassWord != accountRegister.RePass)
            {
                return BadRequest("Repass is different from");
            }
            if (_context.Users.Any(u => u.Username == accountRegister.UserName))
            {
                return BadRequest("The username is already exists");

            }
            var newUser = new User()
            {
                Username = accountRegister.UserName,
                Name = accountRegister.UserName,
                Img = "Uploads/Defaults/avtuser.jpg",
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
            return Ok("Logout success");
        }
        private string Generate(User? user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]{

                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                // new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, string.Concat(user.Roles.Select(x => x.Name).ToList())),
                new Claim("id",user.Id.ToString()),
                new Claim("userName",user.Username),  
                new Claim("name",user.Name),
                new Claim("img",user.Img),
                new Claim ("email",user.Email== null?"":user.Email),
                new Claim("roles",string.Concat(user.Roles.Select(x => x.Name).ToList()) )
            };
            var token = new JwtSecurityToken(_config["Jwt:Issuer"], _config["Jwt:Audience"], claims, expires: DateTime.Now.AddDays(30), signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
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