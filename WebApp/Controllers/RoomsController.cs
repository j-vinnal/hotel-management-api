using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using App.Contracts.DAL;
using App.Public;

namespace WebApp.Controllers
{
    public class RoomsController : Controller
    {
        private readonly IAppUnitOfWork _uow;
        private readonly BllPublicMapper<App.DTO.DAL.Room, App.DTO.Public.v1.Room> _mapper;

        public RoomsController(IAppUnitOfWork uow, IMapper autoMapper)
        {
            _uow = uow;
            _mapper = new BllPublicMapper<App.DTO.DAL.Room, App.DTO.Public.v1.Room>(autoMapper);
        }
        
        // GET: Rooms
        public async Task<IActionResult> Index()
        {
            var rooms = await _uow.RoomRepository.GetAllSortedAsync();
            var roomDtos = rooms.Select(r => _mapper.Map(r)).ToList();
            return View(roomDtos);
        }

        // GET: Rooms/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var room = await _uow.RoomRepository.FindWithDetailsAsync(id.Value);
            if (room == null) return NotFound();

            var roomDto = _mapper.Map(room);
            return View(roomDto);
        }

        // GET: Rooms/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["HotelId"] = new SelectList(_uow.HotelRepository.GetAll(), "Id", "Name");
            return View();
        }

        // POST: Rooms/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(App.DTO.Public.v1.Room room)
        {
            if (ModelState.IsValid)
            {
                var entityDal = _mapper.Map(room)!;
                _uow.RoomRepository.Add(entityDal);
                await _uow.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["HotelId"] = new SelectList(_uow.HotelRepository.GetAll(), "Id", "Name", room.HotelId);
            return View(room);
        }

        // GET: Rooms/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var room = await _uow.RoomRepository.FindAsync(id.Value);
            if (room == null) return NotFound();

            var roomDto = _mapper.Map(room)!;
            ViewData["HotelId"] = new SelectList(_uow.HotelRepository.GetAll(), "Id", "Name", roomDto.HotelId);
            return View(roomDto);
        }

        // POST: Rooms/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid id, App.DTO.Public.v1.Room room)
        {
            if (id != room.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var entityDal = _mapper.Map(room)!;
                    _uow.RoomRepository.Update(entityDal);
                    await _uow.SaveChangesAsync();
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
            ViewData["HotelId"] = new SelectList(_uow.HotelRepository.GetAll(), "Id", "Name", room.HotelId);
            return View(room);
        }

        // GET: Rooms/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var room = await _uow.RoomRepository.FindWithDetailsAsync(id.Value);
            if (room == null) return NotFound();

            var roomDto = _mapper.Map(room);
            return View(roomDto);
        }

        // POST: Rooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var room = await _uow.RoomRepository.FindWithDetailsAsync(id);
            if (room != null)
            {
                _uow.RoomRepository.Remove(room);
                await _uow.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool RoomExists(Guid id)
        {
            return _uow.RoomRepository.Exists(id);
        }
    }
}