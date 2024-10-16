using System.Net;
using App.Contracts.BLL;
using App.Domain.Identity;
using App.DTO.Public.v1;
using App.Public;
using Asp.Versioning;
using AutoMapper;
using Base.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApp.ApiControllers
{
    /// <summary>
    ///     API Controller for managing hotels.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class HotelsController : ControllerBase
    {
        private readonly IAppBLL _bll;
        private readonly UserManager<AppUser> _userManager;
        private readonly BllPublicMapper<App.DTO.BLL.Hotel, Hotel> _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="HotelsController"/> class.
        /// </summary>
        /// <param name="bll">The business logic layer interface.</param>
        /// <param name="userManager">The user manager for handling user-related operations.</param>
        /// <param name="autoMapper">The AutoMapper instance for mapping between models.</param>
        public HotelsController(IAppBLL bll, UserManager<AppUser> userManager, IMapper autoMapper)
        {
            _bll = bll;
            _userManager = userManager;
            _mapper = new BllPublicMapper<App.DTO.BLL.Hotel, Hotel>(autoMapper);
        }

        //TODO: Implement get hotel name by id

        /// <summary>
        /// Gets all hotels.
        /// </summary>
        /// <returns>A list of hotels.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Hotel>>> GetHotels()
        {
            try
            {
                var userIdStr = _userManager.GetUserId(User);
                if (userIdStr == null)
                {
                    return BadRequest("User ID is not available.");
                }

                var userId = Guid.Parse(userIdStr);

                var hotels = await _bll.HotelService.GetAllAsync(userId);
                var hotelDtos = hotels.Select(h => _mapper.Map(h)).ToList();

                return Ok(hotelDtos);
            }
            catch (Exception ex)
            {
                return HandleXRoadError(ex, "Server.ServerProxy.InternalError");
            }
        }

        /// <summary>
        /// Gets a specific hotel by ID.
        /// </summary>
        /// <param name="id">The ID of the hotel.</param>
        /// <returns>The hotel with the specified ID.</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Hotel>> GetHotel(Guid id)
        {
            try
            {
                var userIdStr = _userManager.GetUserId(User);
                if (userIdStr == null)
                {
                    return BadRequest("User ID is not available.");
                }

                var userId = Guid.Parse(userIdStr);

                var hotel = await _bll.HotelService.FindAsync(id, userId);

                if (hotel == null)
                {
                    return NotFound();
                }

                return Ok(_mapper.Map(hotel));
            }
            catch (Exception ex)
            {
                return HandleXRoadError(ex, "Server.ServerProxy.InternalError");
            }
        }

        /// <summary>
        /// Updates a specific hotel.
        /// </summary>
        /// <param name="id">The ID of the hotel to update.</param>
        /// <param name="hotelDto">The updated hotel data, including name, location, and other relevant details.</param>
        /// <returns>No content if the update is successful; otherwise, a BadRequest or NotFound result.</returns>
        [Authorize(Roles = RoleConstants.Admin)]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> PutHotel(Guid id, Hotel hotelDto)
        {
            try
            {
                if (id != hotelDto.Id)
                {
                    return BadRequest();
                }

                var userIdStr = _userManager.GetUserId(User);
                if (userIdStr == null)
                {
                    return BadRequest("User ID is not available.");
                }

                var userId = Guid.Parse(userIdStr);

                var existingEntity = await _bll.HotelService.FindAsync(hotelDto.Id, userId);
                if (existingEntity == null)
                {
                    return BadRequest(
                        new RestApiErrorResponse() { Status = HttpStatusCode.BadRequest, Error = "Hotel not found." }
                    );
                }

                var hotel = _mapper.Map(hotelDto)!;
                hotel.AppUserId = userId;
                _bll.HotelService.Update(hotel);

                try
                {
                    await _bll.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await HotelExists(id))
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
        /// Checks if a hotel exists.
        /// </summary>
        /// <param name="id">The ID of the hotel.</param>
        /// <returns>True if the hotel exists, otherwise false.</returns>
        private async Task<bool> HotelExists(Guid id)
        {
            return await _bll.HotelService.ExistsAsync(id);
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
                detail = Guid.NewGuid().ToString(),
            };

            Response.ContentType = "application/json";
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            Response.Headers.Append("X-Road-Error", errorType);

            return new JsonResult(errorResponse);
        }
    }
}
