using App.Contracts.BLL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using App.Domain.Identity;
using App.Public;
using Microsoft.AspNetCore.Identity;
using Base.Helpers;

namespace WebApp.Controllers
{
    public class HotelsController : Controller
    {
        private readonly IAppBLL _bll;
        private readonly UserManager<AppUser> _userManager;
        private readonly BllPublicMapper<App.DTO.BLL.Hotel, App.DTO.Public.v1.Hotel> _mapper;

        public HotelsController(IAppBLL bll, UserManager<AppUser> userManager, IMapper autoMapper)
        {
            _bll = bll;
            _userManager = userManager;
            _mapper = new BllPublicMapper<App.DTO.BLL.Hotel, App.DTO.Public.v1.Hotel>(autoMapper);
        }

        // GET: Hotels
        public async Task<IActionResult> Index()
        {
            var hotels = await _bll.HotelService.GetAllAsync();
            var hotelDtos = hotels.Select(h => _mapper.Map(h)).ToList();
            return View(hotelDtos);
        }

        // GET: Hotels/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var hotel = await _bll.HotelService.FindAsync(id.Value);
            if (hotel == null) return NotFound();

            var hotelDto = _mapper.Map(hotel);
            return View(hotelDto);
        }

        // GET: Hotels/Create
        [Authorize(Roles = RoleConstants.Admin)]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Hotels/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> Create(App.DTO.Public.v1.Hotel hotel)
        {
            if (ModelState.IsValid)
            {
                var entityDal = _mapper.Map(hotel)!;
                entityDal.AppUserId = Guid.Parse(_userManager.GetUserId(User));
                _bll.HotelService.Add(entityDal);
                await _bll.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(hotel);
        }

        // GET: Hotels/Edit/5
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var hotel = await _bll.HotelService.FindAsync(id.Value);
            if (hotel == null) return NotFound();

            var hotelDto = _mapper.Map(hotel);
            return View(hotelDto);
        }

        // POST: Hotels/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> Edit(Guid id, App.DTO.Public.v1.Hotel hotel)
        {
            if (id != hotel.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var entityDal = _mapper.Map(hotel)!;
                    entityDal.AppUserId = Guid.Parse(_userManager.GetUserId(User));
                    _bll.HotelService.Update(entityDal);
                    await _bll.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HotelExists(hotel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(hotel);
        }

        // GET: Hotels/Delete/5
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var hotel = await _bll.HotelService.FindAsync(id.Value);
            if (hotel == null) return NotFound();

            var hotelDto = _mapper.Map(hotel);
            return View(hotelDto);
        }

        // POST: Hotels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var hotel = await _bll.HotelService.FindAsync(id);
            if (hotel != null)
            {
                _bll.HotelService.Remove(hotel);
                await _bll.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool HotelExists(Guid id)
        {
            return _bll.HotelService.Exists(id);
        }
    }
}