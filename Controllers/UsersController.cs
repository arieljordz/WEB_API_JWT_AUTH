using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WEB_API_JWT_AUTH.DTO;
using WEB_API_JWT_AUTH.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WEB_API_JWT_AUTH.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly JWTDBContext db;
        private readonly IConfiguration configuration;
        public UsersController(JWTDBContext context, IConfiguration configuration)
        {
            this.db = context;
            this.configuration = configuration;

        }

        [HttpPost]
        [Route("Register")]
        public IActionResult Register(UserDTO data)
        {
            if (data == null)
            {
                return BadRequest("User data is null");
            }

            var obj = new User
            {
                FirstName = data.FirstName,
                LastName = data.LastName,
                Email = data.Email,
                Password = data.Password
            };

            db.Users.Add(obj);
            db.SaveChanges();

            return Ok("User registered successfully");
        }

        [HttpPost]
        [Route("Login")]
        public IActionResult Login(LoginDTO data)
        {
            var user = db.Users.FirstOrDefault(x => x.Email == data.Email && x.Password == data.Password);
            if (user == null)
            {
                return NoContent();
            }

            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, configuration["Jwt:Subject"]),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim(ClaimTypes.Email, user.Email)
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(configuration["Jwt:ExpireMinutes"])),
                signingCredentials: creds);

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                token = jwtToken,
                expiration = token.ValidTo,
                user = new
                {
                    user.UserId,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.IsActive,
                    user.DateCreated
                }
            });
        }

        [Authorize]
        [HttpGet]
        [Route("GetUsers")]
        public IActionResult GetUsers()
        {
            return Ok(db.Users.ToList());
        }

        [Authorize]
        [HttpGet]
        [Route("GetUser")]
        public IActionResult GetUser(int id)
        {
            var user = db.Users.FirstOrDefault(x => x.UserId == id);
            if (user != null)
            {
                return Ok(user);
            }
            else
            {
                return NoContent();
            }

        }
    }
}
