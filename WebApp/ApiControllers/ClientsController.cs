using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using App.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace WebApp.ApiControllers
{
    /// <summary>
    /// Controller for managing client-related operations.
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ClientsController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientsController"/> class.
        /// </summary>
        /// <param name="userManager">The user manager to handle user operations.</param>
        public ClientsController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Retrieves a user by their ID.
        /// </summary>
        /// <param name="id">The ID of the user to retrieve.</param>
        /// <returns>The user with the specified ID.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<AppUser>> GetUser(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        /// <summary>
        /// Updates a user's information.
        /// </summary>
        /// <param name="id">The ID of the user to update.</param>
        /// <param name="updatedUser">The updated user information.</param>
        /// <returns>No content if the update is successful.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, AppUser updatedUser)
        {
            if (id != updatedUser.Id)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.PersonalCode = updatedUser.PersonalCode;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }
    }
}
