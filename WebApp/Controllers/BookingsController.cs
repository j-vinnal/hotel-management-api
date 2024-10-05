using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using App.Contracts.DAL;
using App.Domain.Identity;
using App.Public;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApp.ViewModels;
using Base.Helpers;

namespace WebApp.Controllers
{
    public class BookingsController : Controller
    {
        private readonly IAppUnitOfWork _uow;
        private readonly UserManager<AppUser> _userManager;
        private readonly BllPublicMapper<App.DTO.DAL.Booking, App.DTO.Public.v1.Booking> _mapper;

        public BookingsController(IAppUnitOfWork uow, UserManager<AppUser> userManager, IMapper autoMapper)
        {
            _uow = uow;
            _userManager = userManager;
            _mapper = new BllPublicMapper<App.DTO.DAL.Booking, App.DTO.Public.v1.Booking>(autoMapper);
        }

        // GET: Bookings
        public async Task<IActionResult> Index()
        {
            IEnumerable<App.DTO.DAL.Booking> bookings;

            if (User.IsInRole(RoleConstants.Admin))
            {
                // Fetch all bookings for admin users
                bookings = await _uow.BookingRepository.GetAllSortedAsync();
            }
            else
            {
                // Fetch bookings for the current user
                var userId = Guid.Parse(_userManager.GetUserId(User));
                bookings = await _uow.BookingRepository.GetAllSortedAsync(userId);
            }

            var bookingDtos = bookings.Select(b => _mapper.Map(b)).ToList();
            return View(bookingDtos);
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            App.DTO.DAL.Booking? booking;

            if (User.IsInRole("Admin"))
            {
                // Admin can view any booking
                booking = await _uow.BookingRepository.FindWithDetailsAsync(id.Value);
            }
            else
            {
                // Non-admin users can only view their own bookings
                var userId = Guid.Parse(_userManager.GetUserId(User));
                booking = await _uow.BookingRepository.FindWithDetailsAsync(id.Value, userId);
            }

            if (booking == null) return NotFound();

            var bookingDto = _mapper.Map(booking);
            return View(bookingDto);
        }

        // GET: Bookings/Create
        public async Task<IActionResult> Create()
        {
            if (User.IsInRole("Admin"))
            {
                var users = await _userManager.Users.ToListAsync();
                ViewBag.AppUserId = new SelectList(users, "Id", "Email");
            }
            var viewModel = new BookingViewModel
            {
                RoomSelectList = new SelectList(_uow.RoomRepository.GetAll(), "Id", "RoomNumber")
            };
            return View(viewModel);
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var bookingDto = _mapper.Map(viewModel.Booking)!;

                // Only set QuestId automatically for non-admin users
                if (!User.IsInRole("Admin"))
                {
                    bookingDto.QuestId = Guid.Parse(_userManager.GetUserId(User));
                }

                _uow.BookingRepository.Add(bookingDto);
                await _uow.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            viewModel.RoomSelectList = new SelectList(_uow.RoomRepository.GetAll(), "Id", "RoomNumber", viewModel.Booking.RoomId);
            return View(viewModel);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();
            App.DTO.DAL.Booking? booking;

            if (User.IsInRole("Admin"))
            {
                // Admin can view any booking
                booking = await _uow.BookingRepository.FindWithDetailsAsync(id.Value);
            }
            else
            {
                // Non-admin users can only view their own bookings
                var userId = Guid.Parse(_userManager.GetUserId(User));
                booking = await _uow.BookingRepository.FindWithDetailsAsync(id.Value, userId);
            }

            if (booking == null) return NotFound();

            var bookingDto = _mapper.Map(booking)!;

            ViewData["RoomId"] = new SelectList(_uow.RoomRepository.GetAll(), "Id", "RoomNumber", booking.RoomId);
            ViewData["AppUserId"] = new SelectList(await _userManager.Users.ToListAsync(), "Id", "Email", booking.QuestId);
            return View(bookingDto);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, App.DTO.Public.v1.Booking booking)
        {
            if (id != booking.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var entityDal = _mapper.Map(booking)!;
                    _uow.BookingRepository.Update(entityDal);
                    await _uow.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.Id))
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

            ViewData["RoomId"] = new SelectList(_uow.RoomRepository.GetAll(), "Id", "RoomNumber", booking.RoomId);
            return View(booking);
        }


        // GET: Admin/Contests/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            App.DTO.DAL.Booking? booking;

            if (User.IsInRole("Admin"))
            {
                // Admin can view any booking
                booking = await _uow.BookingRepository.FindWithDetailsAsync(id.Value);
            }
            else
            {
                // Non-admin users can only view their own bookings
                var userId = Guid.Parse(_userManager.GetUserId(User));
                booking = await _uow.BookingRepository.FindWithDetailsAsync(id.Value, userId);
            }

            if (booking == null)
            {
                return NotFound();
            }

            var bookingDto = _mapper.Map(booking)!;

            return View(bookingDto);
        }
        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {

            App.DTO.DAL.Booking? booking;

            if (User.IsInRole("Admin"))
            {
                // Admin can delete any booking
                booking = await _uow.BookingRepository.FindWithDetailsAsync(id);
            }
            else
            {
                // Non-admin users can only delete their own bookings
                var userId = Guid.Parse(_userManager.GetUserId(User));
                booking = await _uow.BookingRepository.FindWithDetailsAsync(id, userId);
            }

            if (booking != null)
            {
                _uow.BookingRepository.Remove(booking);

            }

            await _uow.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(Guid id)
        {
            return _uow.BookingRepository.Exists(id);
        }
    }
}
