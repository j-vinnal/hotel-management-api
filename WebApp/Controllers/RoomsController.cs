using App.Contracts.BLL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using App.Public;
using Base.Helpers;

namespace WebApp.Controllers
{
    public class RoomsController : Controller
    {
        private readonly IAppBLL _bll;
        private readonly BllPublicMapper<App.DTO.BLL.Room, App.DTO.Public.v1.Room> _mapper;

        public RoomsController(IAppBLL bll, IMapper autoMapper)
        {
            _bll = bll;
            _mapper = new BllPublicMapper<App.DTO.BLL.Room, App.DTO.Public.v1.Room>(autoMapper);
        }

        // GET: Rooms
        public async Task<IActionResult> Index()
        {
            var rooms = await _bll.RoomService.GetAllSortedAsync();
            var roomDtos = rooms.Select(r => _mapper.Map(r)).ToList();
            return View(roomDtos);
        }

        // GET: Rooms/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var room = await _bll.RoomService.FindWithDetailsAsync(id.Value);
            if (room == null) return NotFound();

            var roomDto = _mapper.Map(room);
            return View(roomDto);
        }

        // GET: Rooms/Create
        [Authorize(Roles = RoleConstants.Admin)]
        public IActionResult Create()
        {
            ViewData["HotelId"] = new SelectList(_bll.HotelService.GetAll(), "Id", "Name");
            return View();
        }

        // POST: Rooms/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> Create(App.DTO.Public.v1.Room room)
        {
            if (ModelState.IsValid)
            {
                var entityDal = _mapper.Map(room)!;
                _bll.RoomService.Add(entityDal);
                await _bll.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["HotelId"] = new SelectList(_bll.HotelService.GetAll(), "Id", "Name", room.HotelId);
            return View(room);
        }

        // GET: Rooms/Edit/5
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var room = await _bll.RoomService.FindAsync(id.Value);
            if (room == null) return NotFound();

            var roomDto = _mapper.Map(room)!;
            ViewData["HotelId"] = new SelectList(_bll.HotelService.GetAll(), "Id", "Name", roomDto.HotelId);
            return View(roomDto);
        }

        // POST: Rooms/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> Edit(Guid id, App.DTO.Public.v1.Room room)
        {
            if (id != room.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var entityDal = _mapper.Map(room)!;
                    _bll.RoomService.Update(entityDal);
                    await _bll.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.Id))
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
            ViewData["HotelId"] = new SelectList(_bll.HotelService.GetAll(), "Id", "Name", room.HotelId);
            return View(room);
        }

        // GET: Rooms/Delete/5
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var room = await _bll.RoomService.FindWithDetailsAsync(id.Value);
            if (room == null) return NotFound();

            var roomDto = _mapper.Map(room);
            return View(roomDto);
        }

        // POST: Rooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var room = await _bll.RoomService.FindWithDetailsAsync(id);
            if (room != null)
            {
                _bll.RoomService.Remove(room);
                await _bll.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool RoomExists(Guid id)
        {
            return _bll.RoomService.Exists(id);
        }
    }
}