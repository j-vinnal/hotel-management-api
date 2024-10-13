using App.Contracts.DAL.Repositories;
using App.Domain;
using AutoMapper;
using Base.Contracts;
using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF.Repositories;

public class RoomRepository : BaseEntityRepository<Room, DTO.DAL.Room, AppDbContext>, IRoomRepository
{
    public RoomRepository(AppDbContext dataContext, IMapper mapper) : base(dataContext,
        new DalDomainMapper<Room, DTO.DAL.Room>(mapper))
    {
    }

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
                HotelId = s.Hotel!.Id

            });

        if (noTracking) query = query.AsNoTracking();

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
                HotelId = s.Hotel!.Id

            });

        if (noTracking) query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync();
    }


    public async Task<IEnumerable<DTO.DAL.Room>> GetAvailableRoomsAsync(DateTime? startDate, DateTime? endDate, int? guestCount, Guid? currentBookingId, bool noTracking = true)
    {
        var startDateOnly = startDate ?? DateTime.MinValue;
        var endDateOnly = endDate ?? DateTime.MaxValue;

        var bookedRoomIds = await RepositoryDbContext.Bookings
            .Where(b => !b.IsCancelled &&
                        b.Id != currentBookingId && // Exclude the current booking, if client/admin wants to edit booking dates
                       ((startDateOnly >= b.StartDate.Date && startDateOnly <= b.EndDate.Date) ||
                         (endDateOnly <= b.EndDate.Date && endDateOnly >= b.StartDate.Date)))
            .Select(b => b.RoomId)
            .ToListAsync();

        var query = RepositoryDbSet
            .Where(r => !bookedRoomIds.Contains(r.Id) && r.BedCount >= (guestCount ?? 0));

        if (noTracking) query = query.AsNoTracking();

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
                HotelId = r.Hotel!.Id
            }).ToListAsync();
    }
}
