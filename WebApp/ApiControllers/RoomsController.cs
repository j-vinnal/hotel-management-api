using App.Contracts.BLL;
using App.DTO.Public.v1;
using App.Public;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Exceptions;

namespace WebApp.ApiControllers
{
    /// <summary>
    ///     API Controller for managing rooms.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly IAppBLL _bll;
        private readonly BllPublicMapper<App.DTO.BLL.Room, Room> _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoomsController"/> class.
        /// </summary>
        /// <param name="bll">The business logic layer interface.</param>
        /// <param name="autoMapper">The AutoMapper instance for mapping between models.</param>
        public RoomsController(IAppBLL bll, IMapper autoMapper)
        {
            _bll = bll;
            _mapper = new BllPublicMapper<App.DTO.BLL.Room, Room>(autoMapper);
        }

        /// <summary>
        /// Retrieves a list of available rooms based on the specified availability request.
        /// </summary>
        /// <param name="request">The room availability request containing optional start date, end date, and guest count.</param>
        /// <returns>A list of available rooms that match the criteria specified in the request.</returns>
        /// <response code="200">Returns the list of available rooms.</response>
        /// <response code="400">If the end date is earlier than the start date.</response>
        /// <response code="500">If an internal server error occurs.</response>
        [HttpGet]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType<IEnumerable<Room>>(StatusCodes.Status200OK)]
        [ProducesResponseType<RestApiErrorResponse>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<RestApiErrorResponse>(StatusCodes.Status500InternalServerError)]
        [XRoadService("INSTANCE/CLASS/MEMBER/SUBSYSTEM/RoomService/GetRooms")]
        public async Task<ActionResult<IEnumerable<Room>>> GetRooms([FromQuery] RoomAvailabilityRequest request)
        {
            switch (request)
            {
                // Validate that both startDate and endDate are either both provided or both omitted
                case { StartDate: not null, EndDate: null }:
                case { StartDate: null, EndDate: not null }:
                    throw new BadRequestException("Both startDate and endDate must be provided or not provided.");
                case { StartDate: not null, EndDate: not null }:
                    ValidateBookingDates(request.StartDate.Value, request.EndDate.Value);
                    break;
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
        /// <response code="200">Returns the room with the specified ID.</response>
        /// <response code="404">If the room is not found.</response>
        /// <response code="500">If an internal server error occurs.</response>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType<Room>(StatusCodes.Status200OK)]
        [ProducesResponseType<RestApiErrorResponse>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<RestApiErrorResponse>(StatusCodes.Status500InternalServerError)]
        [XRoadService("INSTANCE/CLASS/MEMBER/SUBSYSTEM/RoomService/GetRoom")]
        public async Task<ActionResult<Room>> GetRoom(Guid id)
        {
            var room = await _bll.RoomService.FindAsync(id) ?? throw new NotFoundException("Room not found");

            return Ok(_mapper.Map(room));
        }

        /// <summary>
        /// Updates a specific room.
        /// </summary>
        /// <param name="id">The ID of the room to update.</param>
        /// <param name="roomDto">The updated room data.</param>
        /// <returns>The updated room data if the update is successful.</returns>
        /// <response code="200">If the room is successfully updated.</response>
        /// <response code="400">If the room data is invalid.</response>
        /// <response code="404">If the room is not found.</response>
        /// <response code="500">If an internal server error occurs.</response>
        [HttpPut("{id}")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType<Room>(StatusCodes.Status200OK)]
        [ProducesResponseType<RestApiErrorResponse>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<RestApiErrorResponse>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<RestApiErrorResponse>(StatusCodes.Status500InternalServerError)]
        [XRoadService("INSTANCE/CLASS/MEMBER/SUBSYSTEM/RoomService/PutRoom")]
        public async Task<ActionResult<Room>> PutRoom(Guid id, Room roomDto)
        {
            if (id != roomDto.Id)
            {
                throw new BadRequestException("Room ID does not match the ID in the request.");
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
                    throw new NotFoundException("Room not found");
                }
                else
                {
                    throw;
                }
            }

            return Ok(roomDto);
        }

        /// <summary>
        /// Creates a new room.
        /// </summary>
        /// <param name="roomDto">The room data to create.</param>
        /// <returns>The created room.</returns>
        /// <response code="201">Returns the created room.</response>
        /// <response code="400">If the room data is invalid.</response>
        /// <response code="500">If an internal server error occurs.</response>
        [HttpPost]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType<Room>(StatusCodes.Status201Created)]
        [ProducesResponseType<RestApiErrorResponse>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<RestApiErrorResponse>(StatusCodes.Status500InternalServerError)]
        [XRoadService("INSTANCE/CLASS/MEMBER/SUBSYSTEM/RoomService/PostRoom")]
        public async Task<ActionResult<Room>> PostRoom(Room roomDto)
        {
            var room = _mapper.Map(roomDto)!;
            _bll.RoomService.Add(room);
            await _bll.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, _mapper.Map(room));
        }

        /// <summary>
        /// Deletes a specific room.
        /// </summary>
        /// <param name="id">The ID of the room to delete.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the room is successfully deleted.</response>
        /// <response code="404">If the room is not found.</response>
        /// <response code="500">If an internal server error occurs.</response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<RestApiErrorResponse>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<RestApiErrorResponse>(StatusCodes.Status500InternalServerError)]
        [XRoadService("INSTANCE/CLASS/MEMBER/SUBSYSTEM/RoomService/DeleteRoom")]
        public async Task<IActionResult> DeleteRoom(Guid id)
        {
            var room = await _bll.RoomService.FindAsync(id);
            if (room == null)
            {
                throw new NotFoundException("Room not found");
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

        /// <summary>
        /// Validates the booking dates to ensure the end date is not earlier than the start date.
        /// </summary>
        /// <param name="startDate">The start date of the booking.</param>
        /// <param name="endDate">The end date of the booking.</param>
        private static void ValidateBookingDates(DateTime startDate, DateTime endDate)
        {
            if (endDate.Date < startDate.Date)
            {
                throw new BadRequestException("End date cannot be earlier than start date.");
            }
        }
    }
}
