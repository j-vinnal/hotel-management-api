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
                RoomNumber = s.RoomNumber,
                BedCount = s.BedCount,
                Price = s.Price,
                HotelName = s.Hotel!.Name,
                HotelId = s.Hotel.Id

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
                RoomNumber = s.RoomNumber,
                BedCount = s.BedCount,
                Price = s.Price,
                HotelName = s.Hotel!.Name,
                HotelId = s.Hotel.Id

            });

        if (noTracking) query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<DTO.DAL.Room>> GetAvailableRoomsAsync(DateTime startDate, DateTime endDate, bool noTracking = true)
    {
        var bookedRoomIds = await RepositoryDbContext.Bookings
            .Where(b => !b.IsCancelled && 
                        ((b.StartDate <= startDate && b.EndDate > startDate) || 
                         (b.StartDate < endDate && b.EndDate >= endDate) || 
                         (b.StartDate >= startDate && b.EndDate <= endDate)))
            .Select(b => b.RoomId)
            .ToListAsync();

        var query = RepositoryDbSet
            .Where(r => !bookedRoomIds.Contains(r.Id));

        if (noTracking) query = query.AsNoTracking();

        return await query.Select(r => new DTO.DAL.Room
        {
            Id = r.Id,
            RoomNumber = r.RoomNumber,
            BedCount = r.BedCount,
            Price = r.Price,
            HotelName = r.Hotel!.Name,
            HotelId = r.Hotel.Id
        }).ToListAsync();
    }
    
    
    
}

