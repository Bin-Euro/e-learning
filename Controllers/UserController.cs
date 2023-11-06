using Cursus.DTO.User;
using Cursus.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cursus.Controllers
{
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _userService.GetAll();
            return StatusCode(result._statusCode, result);
        }

        [HttpGet("get-user-profile")]
        [Authorize]
        public async Task<IActionResult> GetUserProfile()
        {
            var result = await _userService.GetUserProfile();
            return StatusCode(result._statusCode, result);
        }

        [HttpPut("update-user-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UserProfileUpdateDTO user)
        {
            var result = await _userService.UpdateUserProfile(user);
            return StatusCode(result._statusCode, result);
        }

        [HttpPut("update-user-status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DisabledUser([FromBody] UpdateUserStatusDTO user)
        {
            var result = await _userService.UpdateUserStatus(user.Id, user.Status);
            return StatusCode(result._statusCode, result);
        }
    }
}