using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using App.Domain.Identity;
using App.DTO.Public.v1;
using App.Public;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

namespace WebApp.ApiControllers
{
    /// <summary>
    /// Controller for managing client-related operations.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class ClientsController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly BllPublicMapper<AppUser, Client> _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientsController"/> class.
        /// </summary>
        /// <param name="userManager">The user manager to handle user operations.</param>
        public ClientsController(UserManager<AppUser> userManager, IMapper autoMapper)
        {
            _userManager = userManager;
            _mapper = new BllPublicMapper<AppUser, Client>(autoMapper);
        }

        /// <summary>
        /// Retrieves all clients.
        /// </summary>
        /// <returns>A list of clients.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients()
        {

            try
            {
                var users = await _userManager.Users
                    .OrderBy(u => u.FirstName)
                    .ToListAsync();
                var clients = users.Select(u => _mapper.Map(u)).ToList();
                return Ok(clients);
            }
            catch (Exception ex)
            {
                return HandleXRoadError(ex, "Server.ServerProxy.InternalError");
            }
        }

        /// <summary>
        /// Retrieves a client by their ID.
        /// </summary>
        /// <param name="id">The ID of the client to retrieve.</param>
        /// <returns>The client with the specified ID.</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Client>> GetClient(Guid id)
        {

            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    return NotFound();
                }
                var client = _mapper.Map(user);
                return Ok(client);
            }
            catch (Exception ex)
            {
                return HandleXRoadError(ex, "Server.ServerProxy.InternalError");
            }
        }

        /// <summary>
        /// Updates a client's information.
        /// </summary>
        /// <param name="id">The ID of the client to update.</param>
        /// <param name="updatedUser">The updated client information.</param>
        /// <returns>No content if the update is successful.</returns>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateClient(Guid id, Client updatedUser)
        {
            try
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
            catch (Exception ex)
            {
                return HandleXRoadError(ex, "Server.ServerProxy.InternalError");
            }
        }
        /// <summary>
        /// Handles X-Road specific errors.
        /// </summary>
        /// <param name="exception">The exception that occurred.</param>
        /// <param name="errorType">The type of X-Road error.</param>
        /// <returns>An ActionResult with the error details.</returns>
        private ActionResult HandleXRoadError(Exception exception, string errorType)
        {
            var errorResponse = new
            {
                type = errorType,
                message = exception.Message,
                detail = Guid.NewGuid().ToString()
            };

            Response.ContentType = "application/json";
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            Response.Headers.Append("X-Road-Error", errorType);

            return new JsonResult(errorResponse);
        }
    }
}