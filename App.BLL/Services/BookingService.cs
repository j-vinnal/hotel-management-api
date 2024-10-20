using App.Constants;
using App.Contracts.BLL.Services;
using App.Contracts.DAL;
using App.Contracts.DAL.Repositories;
using App.DTO.BLL;
using AutoMapper;
using Base.BLL;

namespace App.BLL.Services;

public class BookingService : BaseEntityService<Booking, DTO.DAL.Booking, IBookingRepository>, IBookingService
{
    public BookingService(IAppUnitOfWork uow, IMapper mapper)
        : base(uow.BookingRepository, new BllDalMapper<App.DTO.DAL.Booking, Booking>(mapper)) { }

    public async Task<IEnumerable<Booking>> GetAllSortedAsync(Guid userId = default, bool noTracking = true)
    {
        return (await Repository.GetAllSortedAsync(userId)).Select(e => EntityMapper.Map(e))!;
    }

    public async Task<Booking?> FindWithDetailsAsync(Guid id, Guid? userId = default, bool noTracking = true)
    {
        return EntityMapper.Map(await Repository.FindWithDetailsAsync(id, userId));
    }

    public async Task<bool> IsRoomBookedAsync(
        Guid roomId,
        DateTime startDate,
        DateTime endDate,
        Guid? currentBookingId = null
    )
    {
        return await Repository.IsRoomBookedAsync(roomId, startDate, endDate, currentBookingId);
    }

    /// <summary>
    /// Determines if a booking can be canceled based on the start datetime and the cancellation limit.
    /// </summary>
    /// <param name="booking">The booking to check.</param>
    /// <returns><c>true</c> if the booking can be canceled; otherwise, <c>false</c>.</returns>
    public bool CanCancelBooking(Booking booking)
    {
        var now = DateTime.UtcNow;
        return booking.StartDate >= now.AddDays(BusinessConstants.BookingCancellationDaysLimit);
    }


}
