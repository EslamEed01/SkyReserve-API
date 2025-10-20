using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyReserve.Application.Consts;
using SkyReserve.Application.Contract.Roles;
using SkyReserve.Application.Interfaces;
using SkyReserve.Infrastructure.Authorization;

namespace SkyReserve.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        /// <summary>
        /// Get all roles
        /// </summary>
        [HttpGet("")]
        [HasPermission(Permissions.Administration.ManageRoles)]
        public async Task<IActionResult> GetAll([FromQuery] bool includeDisabled, CancellationToken cancellationToken)
        {
            var roles = await _roleService.GetAllAsync(includeDisabled, cancellationToken);
            return Ok(roles);
        }

        /// <summary>
        /// Get a specific role by id
        /// </summary>
        [HttpGet("{id}")]
        [HasPermission(Permissions.Administration.ManageRoles)]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            var result = await _roleService.GetAsync(id);
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }

        /// <summary>
        /// Create a new role
        /// </summary>
        [HttpPost("")]
        [HasPermission(Permissions.Administration.ManageRoles)]
        public async Task<IActionResult> Add([FromBody] RoleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _roleService.AddAsync(request);
            return result.IsSuccess ? CreatedAtAction(nameof(Get), new { result.Value.Id }, result.Value) : result.ToProblem();
        }

        /// <summary>
        /// Update a specific role by id
        /// </summary>
        [HttpPut("{id}")]
        [HasPermission(Permissions.Administration.ManageRoles)]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] RoleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _roleService.UpdateAsync(id, request);
            return result.IsSuccess ? NoContent() : result.ToProblem();
        }

        /// <summary>
        /// Toggle status of a specific role by id
        /// </summary>
        [HttpPut("{id}/toggle-status")]
        [HasPermission(Permissions.Administration.ManageRoles)]
        public async Task<IActionResult> ToggleStatus([FromRoute] string id)
        {
            var result = await _roleService.ToggleStatusAsync(id);
            return result.IsSuccess ? NoContent() : result.ToProblem();
        }

        /// <summary>
        /// Get all available permissions
        /// </summary>
        [HttpGet("permissions")]
        [HasPermission(Permissions.Administration.ManagePermissions)]
        public IActionResult GetAllPermissions()
        {
            var permissions = Permissions.GetAllPermissions().Where(p => !string.IsNullOrEmpty(p));
            return Ok(permissions);
        }
    }
}
