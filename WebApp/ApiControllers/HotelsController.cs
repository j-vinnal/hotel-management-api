using App.Contracts.BLL;
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

        /// <summary>
        /// Gets all hotels.
        /// </summary>
        /// <returns>A list of hotels.</returns>
        /// <response code="200">Returns the list of hotels.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="500">If an internal server error occurs.</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType<IEnumerable<Hotel>>(StatusCodes.Status200OK)]
        [ProducesResponseType<RestApiErrorResponse>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<RestApiErrorResponse>(StatusCodes.Status500InternalServerError)]
        [XRoadService("INSTANCE/CLASS/MEMBER/SUBSYSTEM/HotelService/GetHotels")]
        public async Task<ActionResult<IEnumerable<Hotel>>> GetHotels()
        {
            var userIdStr = _userManager.GetUserId(User) ?? throw new BadRequestException("User ID is not available.");
            var userId = Guid.Parse(userIdStr);

            var hotels = await _bll.HotelService.GetAllAsync(userId);
            var hotelDtos = hotels.Select(h => _mapper.Map(h)).ToList();

            return Ok(hotelDtos);
        }

        /// <summary>
        /// Gets a specific hotel by ID.
        /// </summary>
        /// <param name="id">The ID of the hotel.</param>
        /// <returns>The hotel with the specified ID.</returns>
        /// <response code="200">Returns the hotel with the specified ID.</response>
        /// <response code="404">If the hotel is not found.</response>
        /// <response code="500">If an internal server error occurs.</response>
        [HttpGet("{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType<Hotel>(StatusCodes.Status200OK)]
        [ProducesResponseType<RestApiErrorResponse>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<RestApiErrorResponse>(StatusCodes.Status500InternalServerError)]
        [XRoadService("INSTANCE/CLASS/MEMBER/SUBSYSTEM/HotelService/GetHotel")]
        public async Task<ActionResult<Hotel>> GetHotel(Guid id)
        {
            var userIdStr = _userManager.GetUserId(User) ?? throw new BadRequestException("User ID is not available.");
            var userId = Guid.Parse(userIdStr);

            var hotel = await _bll.HotelService.FindAsync(id, userId) ?? throw new NotFoundException("Hotel not found");

            return Ok(_mapper.Map(hotel));
        }

        /// <summary>
        /// Updates a specific hotel.
        /// </summary>
        /// <param name="id">The ID of the hotel to update.</param>
        /// <param name="hotelDto">The updated hotel data, including name, location, and other relevant details.</param>
        /// <returns>The updated hotel data if successful.</returns>
        /// <response code="200">If the hotel is successfully updated.</response>
        /// <response code="400">If the hotel data is invalid.</response>
        /// <response code="404">If the hotel is not found.</response>
        /// <response code="500">If an internal server error occurs.</response>
        [HttpPut("{id:guid}")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType<Hotel>(StatusCodes.Status200OK)]
        [ProducesResponseType<RestApiErrorResponse>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<RestApiErrorResponse>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<RestApiErrorResponse>(StatusCodes.Status500InternalServerError)]
        [XRoadService("INSTANCE/CLASS/MEMBER/SUBSYSTEM/HotelService/PutHotel")]
        public async Task<ActionResult<Hotel>> PutHotel(Guid id, Hotel hotelDto)
        {
            if (id != hotelDto.Id)
            {
                throw new BadRequestException("Hotel ID does not match the ID in the request.");
            }

            var userIdStr = _userManager.GetUserId(User) ?? throw new BadRequestException("User ID is not available.");
            var userId = Guid.Parse(userIdStr);

            var existingEntity =
                await _bll.HotelService.FindAsync(hotelDto.Id, userId)
                ?? throw new NotFoundException("Hotel not found");

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
                    throw new NotFoundException("Hotel not found");
                }
                else
                {
                    throw;
                }
            }

            return Ok(hotelDto);
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
        /// Deletes a specific hotel.
        /// </summary>
        /// <param name="id">The ID of the hotel to delete.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the hotel is successfully deleted.</response>
        /// <response code="404">If the hotel is not found.</response>
        /// <response code="500">If an internal server error occurs.</response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<RestApiErrorResponse>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<RestApiErrorResponse>(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteHotel(Guid id)
        {
            var userIdStr = _userManager.GetUserId(User) ?? throw new BadRequestException("User ID is not available.");
            var userId = Guid.Parse(userIdStr);

            var hotel = await _bll.HotelService.FindAsync(id, userId) ?? throw new NotFoundException("Hotel not found");

            _bll.HotelService.Remove(hotel);
            await _bll.SaveChangesAsync();

            return NoContent();
        }
    }
}
