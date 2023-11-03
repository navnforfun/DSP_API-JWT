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

namespace DSP_API.Controllers
{

    [ApiController]
    public class AccountController : BaseController
    {
        private readonly DspApiContext _context;
        private IConfiguration _config;

        public AccountController(DspApiContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        [HttpGet]
        public IActionResult Test(){
            System.Console.WriteLine(_Username);
            System.Console.WriteLine(_UserId);
            return Ok("Ok thanh cong");
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
            if (userLogin.BanEnabled == true)
            {
                return BadRequest("0. The account was baned by admin!");
            }
            var token = Generate(userLogin);
            _UserId = userLogin.Id;
            _Username = userLogin.Username;
            _UserRole = string.Concat(userLogin.Roles.Select(x => x.Name).ToList());
            _let.print("Login ok - " + _UserId + " - " + _Username + " - " + _UserRole);
            // return Ok("1. Login ok - " + _UserId + " - " + _Username + " - " + _UserRole + "\nToken:" +token);
            return Ok(token);
        }
        [HttpPost]
        [IsTime()]
        public async Task<IActionResult> LoginByToken()
        {
            var token = (HttpContext.Request.Headers[HeaderNames.Authorization]).ToString();
            _let.print(token);
            if (!string.IsNullOrWhiteSpace(token))
            {
                // System.Console.WriteLine(token);
                if (!_let.CheckTokenIsValid(token.Substring(7)))
                {

                    return BadRequest("Your token time is expired or invalid!");
                }
                else
                {
                    var identity = HttpContext.User.Identity as ClaimsIdentity;
                    if (identity != null)
                    {
                        var userClaims = identity.Claims;


                        var Username = userClaims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier)?.Value;
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
                                return BadRequest("0. Khong co user");
                            }

                            if (tokenUser.BanEnabled == true)
                            {
                                return BadRequest("0. The account was baned by admin!");
                            }

                            _UserId = tokenUser.Id;
                            _Username = tokenUser.Username;
                            _UserRole = string.Concat(tokenUser.Roles.Select(x => x.Name).ToList());
                            _let.print("Login ok - " + _UserId + " - " + _Username + " - " + _UserRole);
                            // return Ok("1. Login ok - " + _UserId + " - " + _Username + " - " + _UserRole + "\nToken:" +token);
                            return Ok(Generate(tokenUser));
                        }


                    }
                }

            }
            return BadRequest("Token is null or empty");
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
        private string Generate(User? user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]{
                new Claim(ClaimTypes.NameIdentifier, user.Username),
                // new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.Name),
                new Claim(ClaimTypes.Role, string.Concat(user.Roles.Select(x => x.Name).ToList()))
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