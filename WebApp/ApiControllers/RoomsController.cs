using System.Net;
using App.Contracts.BLL;
using App.DTO.Public.v1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.Public;
using Asp.Versioning;
using AutoMapper;
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
    [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
            try
            {
                // Validate that both startDate and endDate are either both provided or both omitted
                if ((request.StartDate.HasValue && !request.EndDate.HasValue) || (!request.StartDate.HasValue && request.EndDate.HasValue))
                {
                    return BadRequest(
                        new RestApiErrorResponse()
                        {
                            Status = HttpStatusCode.BadRequest,
                            Error = "Both startDate and endDate must be provided or not provided."
                        }
                    );
                }

                if (request.StartDate.HasValue && request.EndDate.HasValue)
                {
                    var validationResult = ValidateBookingDates(request.StartDate.Value, request.EndDate.Value);
                    if (validationResult != null)
                    {
                        return validationResult;
                    }
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
            catch (Exception ex)
            {
                return HandleXRoadError(ex, "Server.ServerProxy.InternalError");
            }
        }

        /// <summary>
        /// Gets a specific room by ID.
        /// </summary>
        /// <param name="id">The ID of the room.</param>
        /// <returns>The room with the specified ID.</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<App.DTO.Public.v1.Room>> GetRoom(Guid id)
        {
            try
            {
                var room = await _bll.RoomService.FindAsync(id);

                if (room == null)
                {
                    return NotFound();
                }

                return Ok(_mapper.Map(room));
            }
            catch (Exception ex)
            {
                return HandleXRoadError(ex, "Server.ServerProxy.InternalError");
            }
        }

        /// <summary>
        /// Updates a specific room.
        /// </summary>
        /// <param name="id">The ID of the room to update.</param>
        /// <param name="roomDto">The updated room data.</param>
        /// <returns>No content if successful.</returns>
        [HttpPut("{id}")]

        public async Task<IActionResult> PutRoom(Guid id, App.DTO.Public.v1.Room roomDto)
        {

            try
            {
                if (id != roomDto.Id)
                {
                    return BadRequest(
                        new RestApiErrorResponse()
                        {
                            Status = HttpStatusCode.BadRequest,
                            Error = "Room not found"
                        }
                    );
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
            catch (Exception ex)
            {
                return HandleXRoadError(ex, "Server.ServerProxy.InternalError");
            }
        }

        /// <summary>
        /// Creates a new room.
        /// </summary>
        /// <param name="roomDto">The room data to create.</param>
        /// <returns>The created room.</returns>
        [HttpPost]
        public async Task<ActionResult<App.DTO.Public.v1.Room>> PostRoom(App.DTO.Public.v1.Room roomDto)
        {
            try
            {
                var room = _mapper.Map(roomDto)!;
                _bll.RoomService.Add(room);
                await _bll.SaveChangesAsync();

                return CreatedAtAction("GetRoom", new { id = room.Id }, _mapper.Map(room));
            }
            catch (Exception ex)
            {
                return HandleXRoadError(ex, "Server.ServerProxy.InternalError");
            }
        }

        /// <summary>
        /// Deletes a specific room.
        /// </summary>
        /// <param name="id">The ID of the room to delete.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(Guid id)
        {
            try
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
            catch (Exception ex)
            {
                return HandleXRoadError(ex, "Server.ServerProxy.InternalError");
            }
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
        /// <returns>
        /// A BadRequest result if the end date is earlier than the start date; otherwise, null.
        /// </returns>
        private ActionResult? ValidateBookingDates(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
            {
                return BadRequest(
                    new RestApiErrorResponse()
                    {
                        Status = HttpStatusCode.BadRequest,
                        Error = "End date cannot be earlier than start date."
                    }
                );
            }
            return null;
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
