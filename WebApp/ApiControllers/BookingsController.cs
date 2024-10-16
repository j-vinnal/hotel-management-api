using System.Net;
using App.Constants;
using App.Contracts.BLL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.Domain.Identity;
using App.DTO.Public.v1;
using App.Public;
using Asp.Versioning;
using AutoMapper;
using Base.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;


namespace WebApp.ApiControllers
{
    /// <summary>
    ///     API Controller for managing bookings.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/v{version:apiVersion}/[controller]")]

    public class BookingsController : ControllerBase
    {
        private readonly IAppBLL _bll;
        private readonly UserManager<AppUser> _userManager;
        private readonly BllPublicMapper<App.DTO.BLL.Booking, App.DTO.Public.v1.Booking> _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="BookingsController"/> class.
        /// </summary>
        /// <param name="bll">The business logic layer interface.</param>
        /// <param name="userManager">The user manager for handling user-related operations.</param>
        /// <param name="autoMapper">The AutoMapper instance for mapping between models.</param>
        public BookingsController(IAppBLL bll, UserManager<AppUser> userManager, IMapper autoMapper)
        {
            _bll = bll;
            _userManager = userManager;
            _mapper = new BllPublicMapper<App.DTO.BLL.Booking, App.DTO.Public.v1.Booking>(autoMapper);
        }

        /// <summary>
        /// Gets all bookings.
        /// </summary>
        /// <returns>A list of bookings.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<App.DTO.Public.v1.Booking>>> GetBookings(bool viewAll = true)
        {

            try
            {
                IEnumerable<App.DTO.BLL.Booking> bookings;

                var userId = Guid.Parse(_userManager.GetUserId(User));

                if (User.IsInRole(RoleConstants.Admin))
                {
                    if (viewAll)
                    {
                        bookings = await _bll.BookingService.GetAllSortedAsync();
                    }
                    else
                    {
                        bookings = await _bll.BookingService.GetAllSortedAsync(userId);
                    }
                }
                else
                {
                    bookings = await _bll.BookingService.GetAllSortedAsync(userId);
                }

                var bookingDtos = bookings.Select(b => _mapper.Map(b)).ToList();
                return Ok(bookingDtos);
            }
            catch (Exception ex)
            {
                return HandleXRoadError(ex, "Server.ServerProxy.InternalError");
            }
        }

        /// <summary>
        /// Gets a specific booking by ID.
        /// </summary>
        /// <param name="id">The ID of the booking.</param>
        /// <returns>The booking with the specified ID.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<App.DTO.Public.v1.Booking>> GetBooking(Guid id)
        {
            try
            {
                App.DTO.BLL.Booking? booking;

                if (User.IsInRole(RoleConstants.Admin))
                {
                    booking = await _bll.BookingService.FindWithDetailsAsync(id);
                }
                else
                {
                    var userId = Guid.Parse(_userManager.GetUserId(User));
                    booking = await _bll.BookingService.FindWithDetailsAsync(id, userId);
                }

                if (booking == null)
                {
                    return NotFound();
                }

                return Ok(_mapper.Map(booking));
            }
            catch (Exception ex)
            {
                return HandleXRoadError(ex, "Server.ServerProxy.InternalError");
            }
        }

        /// <summary>
        /// Updates a specific booking.
        /// </summary>
        /// <param name="id">The ID of the booking to update.</param>
        /// <param name="bookingDto">The updated booking data.</param>
        /// <returns>No content if successful, or a bad request if the room is already booked for the selected dates.</returns>
        [Authorize(Roles = RoleConstants.Admin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBooking(Guid id, App.DTO.Public.v1.Booking bookingDto)
        {
            try
            {
                if (id != bookingDto.Id)
                {
                    return BadRequest();
                }

                var validationResult = ValidateBookingDates(bookingDto.StartDate, bookingDto.EndDate);
                if (validationResult != null)
                {
                    return validationResult;
                }

                bool canBook = !await _bll.BookingService.IsRoomBookedAsync(
                    bookingDto.RoomId,
                    bookingDto.StartDate,
                    bookingDto.EndDate,
                    bookingDto.Id
                );

                if (!canBook)
                {
                    return BadRequest(
                        new RestApiErrorResponse()
                        {
                            Status = HttpStatusCode.BadRequest,
                            Error = $"Room {bookingDto.RoomNumber} is already booked for the selected dates {bookingDto.StartDate:dd.MM.yyyy} - {bookingDto.EndDate:dd.MM.yyyy}."

                        }
                    );
                }

                var booking = _mapper.Map(bookingDto)!;
                _bll.BookingService.Update(booking);

                try
                {
                    await _bll.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await BookingExists(id))
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
        /// Creates a new booking.
        /// </summary>
        /// <param name="bookingDto">The booking data to create.</param>
        /// <returns>
        /// The created booking if successful.
        /// Returns a bad request if the room is already booked for the selected dates.
        /// </returns>
        [HttpPost]
        public async Task<ActionResult<App.DTO.Public.v1.Booking>> PostBooking(App.DTO.Public.v1.Booking bookingDto)
        {
            try
            {
                if (!User.IsInRole(RoleConstants.Admin))
                {
                    bookingDto.QuestId = Guid.Parse(_userManager.GetUserId(User));
                }

             
                var validationResult = ValidateBookingDates(bookingDto.StartDate, bookingDto.EndDate);
                if (validationResult != null)
                {
                    return validationResult;
                }

                bool canBook = !await _bll.BookingService.IsRoomBookedAsync(bookingDto.RoomId, bookingDto.StartDate, bookingDto.EndDate);

                if (!canBook)
                {
                    return BadRequest(
                        new RestApiErrorResponse()
                        {
                            Status = HttpStatusCode.BadRequest,
                            Error = $"Room {bookingDto.RoomNumber} is already booked for the selected dates {bookingDto.StartDate:dd.MM.yyyy} - {bookingDto.EndDate:dd.MM.yyyy}."
                        }
                    );
                }

                var booking = _mapper.Map(bookingDto)!;
                _bll.BookingService.Add(booking);
                await _bll.SaveChangesAsync();

                return CreatedAtAction("GetBooking", new { id = booking.Id }, _mapper.Map(booking));
            }
            catch (Exception ex)
            {
                return HandleXRoadError(ex, "Server.ServerProxy.InternalError");
            }
        }
        /// <summary>
        /// Cancels a specific booking.
        /// </summary>
        /// <param name="id">The ID of the booking to cancel.</param>
        /// <returns>No content if successful, or a forbidden status if not allowed.</returns>
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelBooking(Guid id)
        {
            try
            {
                var userId = Guid.Parse(_userManager.GetUserId(User));
                var booking = await _bll.BookingService.FindAsync(id, userId);

                if (booking == null)
                {
                    return NotFound();
                }

                if (_bll.BookingService.CanCancelBooking(booking))
                {
                    booking.IsCancelled = true;
                    _bll.BookingService.Update(booking);
                    await _bll.SaveChangesAsync();
                    return NoContent();
                }

                return BadRequest(
                    new RestApiErrorResponse()
                    {
                        Status = HttpStatusCode.Forbidden,
                        Error = $"Booking can only be cancelled within {BusinessConstants.BookingCancellationDaysLimit} days of the start date."
                    }
                );
            }
            catch (Exception ex)
            {
                return HandleXRoadError(ex, "Server.ServerProxy.InternalError");
            }
        }

        /// <summary>
        /// Deletes a specific booking.
        /// </summary>
        /// <param name="id">The ID of the booking to delete.</param>
        /// <returns>No content if successful.</returns>
        [Authorize(Roles = RoleConstants.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(Guid id)
        {
            try
            {
                var booking = await _bll.BookingService.FindAsync(id);
                if (booking == null)
                {
                    return NotFound();
                }

                _bll.BookingService.Remove(booking);
                await _bll.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return HandleXRoadError(ex, "Server.ServerProxy.InternalError");
            }
        }

        /// <summary>
        /// Checks if a booking exists.
        /// </summary>
        /// <param name="id">The ID of the booking.</param>
        /// <returns>True if the booking exists, otherwise false.</returns>
        private async Task<bool> BookingExists(Guid id)
        {
            return await _bll.BookingService.ExistsAsync(id);
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
