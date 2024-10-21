using System.Net;
using App.Domain.Identity;
using App.DTO.Public.v1;
using App.Public;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Exceptions;

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
        /// <param name="autoMapper"></param>
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
        [XRoadService("INSTANCE/CLASS/MEMBER/SUBSYSTEM/ClientService/GetClients")]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients()
        {
            var users = await _userManager.Users.OrderBy(u => u.FirstName).ToListAsync();
            var clients = users.Select(u => _mapper.Map(u)).ToList();
            return Ok(clients);
        }

        /// <summary>
        /// Retrieves a client by their ID.
        /// </summary>
        /// <param name="id">The ID of the client to retrieve.</param>
        /// <returns>The client with the specified ID.</returns>
        [HttpGet("{id:guid}")]
        [XRoadService("INSTANCE/CLASS/MEMBER/SUBSYSTEM/ClientService/GetClient")]
        public async Task<ActionResult<Client>> GetClient(Guid id)
        {
            var user =
                await _userManager.FindByIdAsync(id.ToString()) ?? throw new NotFoundException("Client not found");
            var client = _mapper.Map(user);
            return Ok(client);
        }

        /// <summary>
        /// Updates a client's information.
        /// </summary>
        /// <param name="id">The ID of the client to update.</param>
        /// <param name="updatedUser">The updated client information.</param>
        /// <returns>No content if the update is successful.</returns>
        [HttpPut("{id:guid}")]
        [XRoadService("INSTANCE/CLASS/MEMBER/SUBSYSTEM/ClientService/UpdateClient")]
        public async Task<IActionResult> UpdateClient(Guid id, Client updatedUser)
        {
            if (id != updatedUser.Id)
            {
                throw new BadRequestException("Client ID does not match the ID in the request.");
            }

            var user =
                await _userManager.FindByIdAsync(id.ToString()) ?? throw new NotFoundException("Client not found");

            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.PersonalCode = updatedUser.PersonalCode;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new BadRequestException("Failed to update client information.");
            }

            return NoContent();
        }
    }
}
