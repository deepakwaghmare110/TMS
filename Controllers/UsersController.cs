using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS.Data;
using TMS.Models;
using TMS.Services.Interfaces;

namespace TMS.Controllers
{
    [Route("users")]
    [Authorize]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IUser _userRepo;
        private readonly ApplicationDBContext _context;
        public UsersController(ILogger<UsersController> log, IUser user, ApplicationDBContext dBContext)
        {
            _logger = log;
            _userRepo = user;
            _context = dBContext;
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] User userDTO, [FromQuery] string userid)
        {
            User user = new()
            {
                UserName = userDTO.UserName,
                Password = userDTO.Password,
                Role = userDTO.Role
            };

            if (user == null && userDTO is null)
            {
                return BadRequest("User data is required.");
            }
            if (string.IsNullOrWhiteSpace(userid))
            {
                return BadRequest("User id is required.");
            }
            if (string.IsNullOrWhiteSpace(user.UserName) && string.IsNullOrWhiteSpace(userDTO.UserName))
            {
                return BadRequest("Username is required.");
            }

            if (string.IsNullOrWhiteSpace(user.Password) && string.IsNullOrWhiteSpace(userDTO.Password))
            {
                return BadRequest("Password is required.");
            }
            try
            {
                int uid = Int32.Parse(userid);
                var response = await _userRepo.UpdateUserAsync(user, uid);

                return Ok(response);

            }
            catch(Exception ex)
            {
                return StatusCode(500, "Server error while updaing user.");
                _logger.LogInformation(ex.Message);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> deleteUser([FromQuery] string userid)
        {
            
            if (userid is null)
            {
                return BadRequest("User id is required.");
            }

            try
            {
                int uid = Int32.Parse(userid);
                var response = await _userRepo.DeleteUser(uid);

                if (response)
                {
                    return Ok("User Deleted.");
                }
                else
                {
                    return StatusCode(404, "User id not found.");
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Server error while deleting user.");
                _logger.LogInformation(ex.Message);
            }
            
        }

    }
}
