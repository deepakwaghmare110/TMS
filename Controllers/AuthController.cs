using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using TMS.Models;
using TMS.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace TMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IUser _userRepo;
        private readonly IConfiguration _configuration;

        public AuthController(ILogger<AuthController> log, IUser user, IConfiguration configuration) {
            _logger = log;
            _userRepo = user;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User userDTO)
        {
            User user = new()
            {
                UserName = userDTO.UserName,
                Password = userDTO.Password,
                Id = null,
                ResponseMessage = null,
                Role = null
            };
            // Validate the input
            if (user == null)
            {
                return BadRequest("User data is required.");
            }

            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                return BadRequest("Username is required.");
            }

            if (string.IsNullOrWhiteSpace(user.Password))
            {
                return BadRequest("Password is required.");
            }

            try
            {
                string responseMessage = await _userRepo.CreateUserAsync(user);

                if (responseMessage == "User Registered successfully.")
                {
                    return Ok(responseMessage); // HTTP 200 OK
                }
                else if (responseMessage == "User already exists.")
                {
                    return Conflict(responseMessage); // HTTP 409 Conflict
                }
                else
                {
                    return StatusCode(500, "An unexpected error occurred."); // HTTP 500 Internal Server Error
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering user.");
                return StatusCode(500, "An unexpected error occurred."); // HTTP 500 Internal Server Error
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string UserName, string Password)
        {
            User login = new()
            {
                UserName = UserName,
                Password = Password
            };

            if (login == null)
            {
                return BadRequest("User data is required.");
            }

            if (string.IsNullOrWhiteSpace(login.UserName) && string.IsNullOrWhiteSpace(UserName))
            {
                return BadRequest("Username is required.");
            }

            if (string.IsNullOrWhiteSpace(login.Password) && string.IsNullOrWhiteSpace(Password))
            {
                return BadRequest("Password is required.");
            }

            try
            {
                var response = await _userRepo.LoginUserAsync(login);

                if (response.Role != null && response.Id != null && response.ResponseMessage == "Login successful.")
                {
                    var claims = new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("UserId", response.Id.ToString()!),
                    new Claim(ClaimTypes.Role, response.Role)
                };

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
                    var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var token = new JwtSecurityToken(
                        _configuration["Jwt:Issuer"],
                        _configuration["Jwt:Audience"],
                        claims,
                        expires: DateTime.UtcNow.AddMinutes(60),
                        signingCredentials: signIn
                        );
                    var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
                    return Ok(new { Token = tokenValue, RoleName = response.Role, UserId = response.Id });
                }
                else
                {
                    return Ok(response.ResponseMessage);
                }
            }
            catch(Exception ex){

                return StatusCode(500, "An unexpected error occurred. "+ex.Message);

            }
            
        }
    }
}
