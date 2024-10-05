using App.Contracts.BLL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        private readonly IAppBLL _bll;
        private readonly UserManager<AppUser> _userManager;
        private readonly BllPublicMapper<App.DTO.BLL.Booking, App.DTO.Public.v1.Booking> _mapper;

        public BookingsController(IAppBLL bll, UserManager<AppUser> userManager, IMapper autoMapper)
        {
            _bll = bll;
            _userManager = userManager;
            _mapper = new BllPublicMapper<App.DTO.BLL.Booking, App.DTO.Public.v1.Booking>(autoMapper);
        }

        // GET: Bookings
        public async Task<IActionResult> Index()
        {
            IEnumerable<App.DTO.BLL.Booking> bookings;

            if (User.IsInRole(RoleConstants.Admin))
            {
                // Fetch all bookings for admin users
                bookings = await _bll.BookingService.GetAllSortedAsync();
            }
            else
            {
                // Fetch bookings for the current user
                var userId = Guid.Parse(_userManager.GetUserId(User));
                bookings = await _bll.BookingService.GetAllSortedAsync(userId);
            }

            var bookingDtos = bookings.Select(b => _mapper.Map(b)).ToList();
            return View(bookingDtos);
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            App.DTO.BLL.Booking? booking;

            if (User.IsInRole("Admin"))
            {
                // Admin can view any booking
                booking = await _bll.BookingService.FindWithDetailsAsync(id.Value);
            }
            else
            {
                // Non-admin users can only view their own bookings
                var userId = Guid.Parse(_userManager.GetUserId(User));
                booking = await _bll.BookingService.FindWithDetailsAsync(id.Value, userId);
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
                RoomSelectList = new SelectList(_bll.RoomService.GetAll(), "Id", "RoomNumber")
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

                _bll.BookingService.Add(bookingDto);
                await _bll.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            viewModel.RoomSelectList = new SelectList(_bll.RoomService.GetAll(), "Id", "RoomNumber", viewModel.Booking.RoomId);
            return View(viewModel);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();
            App.DTO.BLL.Booking? booking;

            if (User.IsInRole("Admin"))
            {
                // Admin can view any booking
                booking = await _bll.BookingService.FindWithDetailsAsync(id.Value);
            }
            else
            {
                // Non-admin users can only view their own bookings
                var userId = Guid.Parse(_userManager.GetUserId(User));
                booking = await _bll.BookingService.FindWithDetailsAsync(id.Value, userId);
            }

            if (booking == null) return NotFound();

            var bookingDto = _mapper.Map(booking)!;

            ViewData["RoomId"] = new SelectList(_bll.RoomService.GetAll(), "Id", "RoomNumber", booking.RoomId);
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
                    _bll.BookingService.Update(entityDal);
                    await _bll.SaveChangesAsync();
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

            ViewData["RoomId"] = new SelectList(_bll.RoomService.GetAll(), "Id", "RoomNumber", booking.RoomId);
            return View(booking);
        }


        // GET: Admin/Contests/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            App.DTO.BLL.Booking? booking;

            if (User.IsInRole("Admin"))
            {
                // Admin can view any booking
                booking = await _bll.BookingService.FindWithDetailsAsync(id.Value);
            }
            else
            {
                // Non-admin users can only view their own bookings
                var userId = Guid.Parse(_userManager.GetUserId(User));
                booking = await _bll.BookingService.FindWithDetailsAsync(id.Value, userId);
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

            App.DTO.BLL.Booking? booking;

            if (User.IsInRole("Admin"))
            {
                // Admin can delete any booking
                booking = await _bll.BookingService.FindWithDetailsAsync(id);
            }
            else
            {
                // Non-admin users can only delete their own bookings
                var userId = Guid.Parse(_userManager.GetUserId(User));
                booking = await _bll.BookingService.FindWithDetailsAsync(id, userId);
            }

            if (booking != null)
            {
                _bll.BookingService.Remove(booking);

            }

            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(Guid id)
        {
            return _bll.BookingService.Exists(id);
        }
    }
}