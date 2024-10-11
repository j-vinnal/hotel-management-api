using App.Contracts.BLL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.Domain.Identity;
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
    ///     API Controller for managing hotels.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class HotelsController : ControllerBase
    {
        private readonly IAppBLL _bll;
        private readonly UserManager<AppUser> _userManager;
        private readonly BllPublicMapper<App.DTO.BLL.Hotel, App.DTO.Public.v1.Hotel> _mapper;

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
            _mapper = new BllPublicMapper<App.DTO.BLL.Hotel, App.DTO.Public.v1.Hotel>(autoMapper);
        }

        /// <summary>
        /// Gets all hotels.
        /// </summary>
        /// <returns>A list of hotels.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<App.DTO.Public.v1.Hotel>>> GetHotels()
        {
            var hotels = await _bll.HotelService.GetAllAsync();
            var hotelDtos = hotels.Select(h => _mapper.Map(h)).ToList();

            return Ok(hotelDtos);
        }

        /// <summary>
        /// Gets a specific hotel by ID.
        /// </summary>
        /// <param name="id">The ID of the hotel.</param>
        /// <returns>The hotel with the specified ID.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<App.DTO.Public.v1.Hotel>> GetHotel(Guid id)
        {
            var hotel = await _bll.HotelService.FindAsync(id);

            if (hotel == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map(hotel));
        }

        /// <summary>
        /// Updates a specific hotel.
        /// </summary>
        /// <param name="id">The ID of the hotel to update.</param>
        /// <param name="hotelDto">The updated hotel data.</param>
        /// <returns>No content if successful.</returns>
        [Authorize(Roles = RoleConstants.Admin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHotel(Guid id, App.DTO.Public.v1.Hotel hotelDto)
        {
            if (id != hotelDto.Id)
            {
                return BadRequest();
            }

            var hotel = _mapper.Map(hotelDto)!;
            hotel.AppUserId = Guid.Parse(_userManager.GetUserId(User));
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

        /// <summary>
        /// Creates a new hotel.
        /// </summary>
        /// <param name="hotelDto">The hotel data to create.</param>
        /// <returns>The created hotel.</returns>
        [Authorize(Roles = RoleConstants.Admin)]
        [HttpPost]
        public async Task<ActionResult<App.DTO.Public.v1.Hotel>> PostHotel(App.DTO.Public.v1.Hotel hotelDto)
        {
            var hotel = _mapper.Map(hotelDto)!;
            hotel.AppUserId = Guid.Parse(_userManager.GetUserId(User));
            _bll.HotelService.Add(hotel);
            await _bll.SaveChangesAsync();

            return CreatedAtAction("GetHotel", new { id = hotel.Id }, _mapper.Map(hotel));
        }

        /// <summary>
        /// Deletes a specific hotel.
        /// </summary>
        /// <param name="id">The ID of the hotel to delete.</param>
        /// <returns>No content if successful.</returns>
        [Authorize(Roles = RoleConstants.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHotel(Guid id)
        {
            var hotel = await _bll.HotelService.FindAsync(id);
            if (hotel == null)
            {
                return NotFound();
            }

            _bll.HotelService.Remove(hotel);
            await _bll.SaveChangesAsync();

            return NoContent();
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
    }
}
