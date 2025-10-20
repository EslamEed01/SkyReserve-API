﻿using Learnova.Business.DTOs.Contract.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyReserve.Application.Consts;
using SkyReserve.Application.Interfaces;
using SkyReserve.Infrastructure.Authorization;

namespace SkyReserve.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class UsersController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        /// <summary>
        /// Get all users  
        /// </summary>
        [HttpGet("")]
        [HasPermission(Permissions.Users.View)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            return Ok(await _userService.GetAllAsync(cancellationToken));
        }

        /// <summary>
        /// Get a specific user by id  
        /// </summary>
        [HttpGet("{id}")]
        [HasPermission(Permissions.Users.View)]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            var result = await _userService.GetAsync(id);
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }

        /// <summary>
        /// Create a new user  
        /// </summary>
        [HttpPost("")]
        [HasPermission(Permissions.Users.Create)]
        public async Task<IActionResult> Add([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
        {
            var result = await _userService.AddAsync(request, cancellationToken);
            return result.IsSuccess ? CreatedAtAction(nameof(Get), new { result.Value.Id }, result.Value) : result.ToProblem();
        }

        /// <summary>
        /// Update a user by id  
        /// </summary>
        [HttpPut("{id}")]
        [HasPermission(Permissions.Users.Update)]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
        {
            var result = await _userService.UpdateAsync(id, request, cancellationToken);
            Console.WriteLine($"Update called with ID: {id}");
            return result.IsSuccess ? NoContent() : result.ToProblem();
        }

        /// <summary>
        /// Toggle user status (inactive) by id  
        /// </summary>
        [HttpPut("{id}/toggle-status")]
        [HasPermission(Permissions.Users.Disable)]
        public async Task<IActionResult> ToggleStatus([FromRoute] string id)
        {
            var result = await _userService.ToggleStatus(id);
            return result.IsSuccess ? NoContent() : result.ToProblem();
        }

        /// <summary>
        /// Unlock user account by id  
        /// </summary>
        [HttpPut("{id}/unlock")]
        [HasPermission(Permissions.Users.Enable)]
        public async Task<IActionResult> Unlock([FromRoute] string id)
        {
            var result = await _userService.Unlock(id);
            return result.IsSuccess ? NoContent() : result.ToProblem();
        }
    }
}
