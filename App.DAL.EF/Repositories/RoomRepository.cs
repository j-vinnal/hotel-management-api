using App.Contracts.DAL.Repositories;
using App.Domain;
using AutoMapper;
using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF.Repositories;

public class RoomRepository : BaseEntityRepository<Room, DTO.DAL.Room, AppDbContext>, IRoomRepository
{
    public RoomRepository(AppDbContext dataContext, IMapper mapper)
        : base(dataContext, new DalDomainMapper<Room, DTO.DAL.Room>(mapper)) { }

    public async Task<IEnumerable<DTO.DAL.Room>> GetAllSortedAsync(bool noTracking = true)
    {
        var query = RepositoryDbSet
            .Include(e => e.Hotel)
            .Select(s => new DTO.DAL.Room
            {
                Id = s.Id,
                RoomName = s.RoomName,
                RoomNumber = s.RoomNumber,
                BedCount = s.BedCount,
                Price = s.Price,
                ImageUrl = s.ImageUrl,
                HotelId = s.Hotel!.Id,
            });

        if (noTracking)
            query = query.AsNoTracking();

        return await query.OrderBy(p => p.RoomNumber).ToListAsync();
    }

    public async Task<DTO.DAL.Room?> FindWithDetailsAsync(Guid id, bool noTracking = true)
    {
        var query = RepositoryDbSet
            .Include(e => e.Hotel)
            .Where(e => e.Id == id)
            .Select(s => new DTO.DAL.Room
            {
                Id = s.Id,
                RoomName = s.RoomName,
                RoomNumber = s.RoomNumber,
                BedCount = s.BedCount,
                Price = s.Price,
                ImageUrl = s.ImageUrl,
                HotelId = s.Hotel!.Id,
            });

        if (noTracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync();
    }

    /// <summary>
    /// Retrieves available rooms based on booking dates and guest count.
    /// </summary>
    /// <param name="startDate">
    /// The start date of the booking period. If null, no start date filtering is applied.
    /// Must be provided together with <paramref name="endDate"/> or both must be null.
    /// </param>
    /// <param name="endDate">
    /// The end date of the booking period. If null, no end date filtering is applied.
    /// Must be provided together with <paramref name="startDate"/> or both must be null.
    /// </param>
    /// <param name="guestCount">
    /// The number of guests. Defaults to 0 if null, meaning no restriction on bed count.
    /// </param>
    /// <param name="currentBookingId">
    /// The ID of the current booking to exclude from the search, if any.
    /// </param>
    /// <param name="noTracking">
    /// Indicates whether to use no-tracking queries for better performance.
    /// </param>
    /// <returns>
    /// A list of available rooms that meet the specified criteria.
    /// If both <paramref name="startDate"/> and <paramref name="endDate"/> are null, all rooms are returned.
    /// If both are provided, rooms overlapping with the specified date range are excluded.
    /// </returns>
    public async Task<IEnumerable<DTO.DAL.Room>> GetAvailableRoomsAsync(
        DateTime? startDate,
        DateTime? endDate,
        int? guestCount,
        Guid? currentBookingId,
        bool noTracking = true
    )
    {
        // Check if both startDate and endDate are null
        if (!startDate.HasValue && !endDate.HasValue)
        {
            // Directly return all rooms that meet the guest count requirement
            var allRoomsQuery = RepositoryDbSet.Where(r => r.BedCount >= (guestCount ?? 0));

            if (noTracking)
                allRoomsQuery = allRoomsQuery.AsNoTracking();

            return await allRoomsQuery
                .OrderBy(r => r.RoomNumber)
                .Select(r => new DTO.DAL.Room
                {
                    Id = r.Id,
                    RoomName = r.RoomName,
                    RoomNumber = r.RoomNumber,
                    BedCount = r.BedCount,
                    Price = r.Price,
                    ImageUrl = r.ImageUrl,
                    HotelId = r.Hotel!.Id,
                })
                .ToListAsync();
        }

        /// <summary>
        /// Initializes a query to filter bookings, excluding the current booking if specified.
        /// </summary>
        /// <param name="currentBookingId">The ID of the current booking to exclude from the query.</param>
        var bookedRoomIdsQuery = RepositoryDbContext.Bookings.Where(b => !b.IsCancelled && b.Id != currentBookingId);

        /// <summary>
        /// Applies date filters based on provided startDate and endDate.
        /// </summary>
        /// <param name="startDate">The start date of the booking period.</param>
        /// <param name="endDate">The end date of the booking period.</param>
        if (startDate.HasValue && endDate.HasValue)
        {
            // Filter bookings that overlap with the specified date range
            bookedRoomIdsQuery = bookedRoomIdsQuery.Where(b => b.StartDate <= endDate && startDate <= b.EndDate);
        }

        /// <summary>
        /// Retrieves the list of booked room IDs.
        /// </summary>
        var bookedRoomIds = await bookedRoomIdsQuery.Select(b => b.RoomId).ToListAsync();

        /// <summary>
        /// Queries to find available rooms that are not booked and meet the guest count requirement.
        /// </summary>
        /// <param name="guestCount">The number of guests to accommodate.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of available rooms.</returns>
        var query = RepositoryDbSet.Where(r => !bookedRoomIds.Contains(r.Id) && r.BedCount >= (guestCount ?? 0));

        if (noTracking)
            query = query.AsNoTracking();

        return await query
            .OrderBy(r => r.RoomNumber)
            .Select(r => new DTO.DAL.Room
            {
                Id = r.Id,
                RoomName = r.RoomName,
                RoomNumber = r.RoomNumber,
                BedCount = r.BedCount,
                Price = r.Price,
                ImageUrl = r.ImageUrl,
                HotelId = r.Hotel!.Id,
            })
            .ToListAsync();
    }
}
