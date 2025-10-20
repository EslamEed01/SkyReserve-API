using Learnova.Business.DTOs.Contract.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyReserve.Application.Consts;
using SkyReserve.Application.Interfaces;
using SkyReserve.Application.Services;
using SkyReserve.Infrastructure.Authorization;

namespace SkyReserve.API.Controllers
{
    [ApiController]
    [Route("me")]
    [Authorize]
    public class AccountController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        /// <summary>
        /// Get current user profile 
        /// </summary>
        [HttpGet("")]
        [HasPermission(Permissions.Users.ViewProfile)]
        public async Task<IActionResult> Info()
        {
            var result = await _userService.GetProfileAsync(User.GetUserId()!);
            return Ok(result.Value);
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        [HttpPut("info")]
        [HasPermission(Permissions.Users.UpdateOwn)]
        public async Task<IActionResult> Info([FromBody] UpdateProfileRequest request)
        {
            await _userService.UpdateProfileAsync(User.GetUserId()!, request);
            return NoContent();
        }

        /// <summary>
        /// Change password
        /// </summary>
        [HttpPut("change-password")]
        [HasPermission(Permissions.Users.UpdateOwn)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var result = await _userService.ChangePasswordAsync(User.GetUserId()!, request);
            return result.IsSuccess ? NoContent() : result.ToProblem();
        }
    }
}
