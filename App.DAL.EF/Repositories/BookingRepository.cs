using App.Contracts.DAL.Repositories;
using AutoMapper;
using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;
using Booking = App.Domain.Booking;

namespace App.DAL.EF.Repositories;

/// <summary>
/// Repository for managing bookings in the application.
/// </summary>
public class BookingRepository : BaseEntityRepository<Booking, App.DTO.DAL.Booking, AppDbContext>, IBookingRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BookingRepository"/> class.
    /// </summary>
    /// <param name="dataContext">The application database context.</param>
    /// <param name="mapper">The mapper for converting between domain and DTO objects.</param>
    public BookingRepository(AppDbContext dataContext, IMapper mapper)
        : base(dataContext, new DalDomainMapper<Booking, DTO.DAL.Booking>(mapper)) { }

    /// <summary>
    /// Gets all bookings sorted by start date, first name, and last name.
    /// </summary>
    /// <param name="userId">The user ID to filter bookings by user.</param>
    /// <param name="noTracking">If set to <c>true</c>, the query will be executed without tracking.</param>
    /// <returns>A list of sorted bookings.</returns>
    public async Task<IEnumerable<DTO.DAL.Booking>> GetAllSortedAsync(Guid userId = default, bool noTracking = true)
    {
        var query = RepositoryDbSet
            .Include(e => e.Room)
            .Include(e => e.User)
            .Where(e => userId == default || e.User!.Id == userId)
            .Select(s => new DTO.DAL.Booking
            {
                Id = s.Id,
                RoomId = s.RoomId,
                RoomNumber = s.Room!.RoomNumber,
                QuestFirstName = s.User!.FirstName,
                QuestLastName = s.User!.LastName,
                QuestId = s.User.Id,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                IsCancelled = s.IsCancelled,
            });

        if (noTracking)
            query = query.AsNoTracking();

        return await query
            .OrderBy(p => p.StartDate)
            .ThenBy(p => p.QuestFirstName)
            .ThenBy(p => p.QuestLastName)
            .ToListAsync();
    }

    /// <summary>
    /// Finds a booking with details by its ID.
    /// </summary>
    /// <param name="id">The booking ID.</param>
    /// <param name="userId">The user ID to filter bookings by user.</param>
    /// <param name="noTracking">If set to <c>true</c>, the query will be executed without tracking.</param>
    /// <returns>The booking with details, or <c>null</c> if not found.</returns>
    public async Task<DTO.DAL.Booking?> FindWithDetailsAsync(Guid id, Guid? userId = default, bool noTracking = true)
    {
        var query = RepositoryDbSet
            .Include(e => e.Room)
            .Include(e => e.User)
            .Where(e => e.Id == id && (userId == default || e.User!.Id == userId))
            .Select(s => new DTO.DAL.Booking
            {
                Id = s.Id,
                RoomId = s.RoomId,
                RoomNumber = s.Room!.RoomNumber,
                QuestFirstName = s.User!.FirstName,
                QuestLastName = s.User!.LastName,
                QuestId = s.User.Id,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                IsCancelled = s.IsCancelled,
            });

        if (noTracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync();
    }

    /// <summary>
    /// Checks if a room is booked within a specified date range.
    /// </summary>
    /// <param name="roomId">The room ID.</param>
    /// <param name="startDate">The start date of the booking.</param>
    /// <param name="endDate">The end date of the booking.</param>
    /// <param name="currentBookingId">The current booking ID to exclude from the check.</param>
    /// <returns><c>true</c> if the room is booked; otherwise, <c>false</c>.</returns>
    public async Task<bool> IsRoomBookedAsync(
        Guid roomId,
        DateTime startDate,
        DateTime endDate,
        Guid? currentBookingId = null
    )
    {
        return await RepositoryDbSet.AnyAsync(b =>
            b.RoomId == roomId
            && !b.IsCancelled
            && b.Id != currentBookingId // Exclude the current booking, if client/admin wants to edit booking dates
            && b.StartDate <= endDate
            && startDate <= b.EndDate
        );
    }
}
