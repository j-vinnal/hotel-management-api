using System.Net;
using App.Contracts.BLL;
using App.DTO.Public.v1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.Public;
using Asp.Versioning;
using AutoMapper;
using Base.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;


namespace WebApp.ApiControllers
{
    /// <summary>
    ///     API Controller for managing rooms.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly IAppBLL _bll;
        private readonly BllPublicMapper<App.DTO.BLL.Room, App.DTO.Public.v1.Room> _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoomsController"/> class.
        /// </summary>
        /// <param name="bll">The business logic layer interface.</param>
        /// <param name="autoMapper">The AutoMapper instance for mapping between models.</param>
        public RoomsController(IAppBLL bll, IMapper autoMapper)
        {
            _bll = bll;
            _mapper = new BllPublicMapper<App.DTO.BLL.Room, App.DTO.Public.v1.Room>(autoMapper);
        }

        /// <summary>
        /// Retrieves a list of available rooms based on the specified availability request.
        /// </summary>
        /// <param name="request">The room availability request containing optional start date, end date, and guest count.</param>
        /// <returns>A list of available rooms that match the criteria specified in the request.</returns>
        /// <response code="200">Returns the list of available rooms.</response>
        /// <response code="400">If the end date is earlier than the start date.</response>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<App.DTO.Public.v1.Room>>> GetRooms([FromQuery] App.DTO.Public.v1.RoomAvailabilityRequest request)
        {
            if (request.StartDate.HasValue && request.EndDate.HasValue && request.EndDate < request.StartDate)
            {
                return BadRequest(
                    new RestApiErrorResponse()
                    {
                        Status = HttpStatusCode.BadRequest,
                        Error = "End date cannot be earlier than start date."
                    }
                );
            }

            var rooms = await _bll.RoomService.GetAvailableRoomsAsync(
                request.StartDate,
                request.EndDate,
                request.GuestCount,
                request.CurrentBookingId
            );

            var roomDtos = rooms.Select(r => _mapper.Map(r)).ToList();
            return Ok(roomDtos);
        }

        /// <summary>
        /// Gets a specific room by ID.
        /// </summary>
        /// <param name="id">The ID of the room.</param>
        /// <returns>The room with the specified ID.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<App.DTO.Public.v1.Room>> GetRoom(Guid id)
        {
            var room = await _bll.RoomService.FindAsync(id);

            if (room == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map(room));
        }

        /// <summary>
        /// Updates a specific room.
        /// </summary>
        /// <param name="id">The ID of the room to update.</param>
        /// <param name="roomDto">The updated room data.</param>
        /// <returns>No content if successful.</returns>
        [Authorize(Roles = RoleConstants.Admin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoom(Guid id, App.DTO.Public.v1.Room roomDto)
        {
            if (id != roomDto.Id)
            {
                return BadRequest();
            }

            var room = _mapper.Map(roomDto)!;
            _bll.RoomService.Update(room);

            try
            {
                await _bll.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await RoomExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Creates a new room.
        /// </summary>
        /// <param name="roomDto">The room data to create.</param>
        /// <returns>The created room.</returns>
        [Authorize(Roles = RoleConstants.Admin)]
        [HttpPost]
        public async Task<ActionResult<App.DTO.Public.v1.Room>> PostRoom(App.DTO.Public.v1.Room roomDto)
        {
            var room = _mapper.Map(roomDto)!;
            _bll.RoomService.Add(room);
            await _bll.SaveChangesAsync();

            return CreatedAtAction("GetRoom", new { id = room.Id }, _mapper.Map(room));
        }

        /// <summary>
        /// Deletes a specific room.
        /// </summary>
        /// <param name="id">The ID of the room to delete.</param>
        /// <returns>No content if successful.</returns>
        [Authorize(Roles = RoleConstants.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(Guid id)
        {
            var room = await _bll.RoomService.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            _bll.RoomService.Remove(room);
            await _bll.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Checks if a room exists.
        /// </summary>
        /// <param name="id">The ID of the room.</param>
        /// <returns>True if the room exists, otherwise false.</returns>
        private async Task<bool> RoomExists(Guid id)
        {
            return await _bll.RoomService.ExistsAsync(id);
        }
    }
}
