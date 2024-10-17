using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMS.Data;
using TMS.Models;
using TMS.Services.Interfaces;

namespace TMS.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {

        private readonly ILogger<AdminController> _logger;
        private readonly IUser _userRepo;
        private readonly ICache _cache;
        private readonly ApplicationDBContext _context;
        public AdminController(ILogger<AdminController> log, IUser user, ICache cache, ApplicationDBContext dBContext) {
            _logger = log;
            _userRepo = user;
            _cache = cache;
            _context = dBContext;
        }

        [HttpGet("Users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var cacheData = _cache.GetData<IEnumerable<User>>("Users");
                if (cacheData != null && cacheData.Count() > 0)
                {
                    return Ok(cacheData);
                }

                //cacheData = await _context.users.ToListAsync();
                cacheData = await _userRepo.GetUsersAsync();

                var expiryTime = DateTimeOffset.Now.AddSeconds(60);
                _cache.SetData<IEnumerable<User>>("Users", cacheData, expiryTime);
                return Ok(cacheData);
            }
            catch (Exception ex) { 
            
                return StatusCode(500, "An unexpected error occurred while fetching Users. " + ex.Message);
            }
           
        }


    }
}
