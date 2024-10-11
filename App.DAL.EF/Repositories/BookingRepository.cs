using App.Contracts.DAL.Repositories;
using AutoMapper;
using Base.Contracts;
using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;
using Booking = App.Domain.Booking;

namespace App.DAL.EF.Repositories;

public class BookingRepository : BaseEntityRepository<Booking, App.DTO.DAL.Booking, AppDbContext>, IBookingRepository
{
    public BookingRepository(AppDbContext dataContext, IMapper mapper) : base(dataContext,
        new DalDomainMapper<Booking, DTO.DAL.Booking>(mapper))
    {

    }

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
                IsCancelled = s.IsCancelled
            });

        if (noTracking) query = query.AsNoTracking();

        return await query.OrderBy(p => p.StartDate).ThenBy(p => p.QuestFirstName).ThenBy(p => p.QuestLastName).ToListAsync();
    }

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
                IsCancelled = s.IsCancelled
            });

        if (noTracking) query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync();
    }

    public async Task<bool> IsRoomBookedAsync(Guid roomId, DateTime startDate, DateTime endDate)
    {
        var startDateOnly = startDate.Date;
        var endDateOnly = endDate.Date;

        return await RepositoryDbSet
            .AnyAsync(b => b.RoomId == roomId &&
                           !b.IsCancelled &&
                           ((startDateOnly >= b.StartDate.Date && startDateOnly <= b.EndDate.Date) ||
                            (endDateOnly <= b.EndDate.Date && endDateOnly >= b.StartDate.Date))
                           );
    }
}