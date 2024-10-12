using App.Contracts.BLL.Services;
using App.Contracts.DAL;
using App.Contracts.DAL.Repositories;
using App.DTO.BLL;
using AutoMapper;
using Base.BLL;

namespace App.BLL.Services;

public class RoomService : BaseEntityService<Room, DTO.DAL.Room, IRoomRepository>,
    IRoomService
{
    
    public RoomService(IAppUnitOfWork uow, IMapper mapper) : base(uow.RoomRepository, new BllDalMapper<App.DTO.DAL.Room, App.DTO.BLL.Room>(mapper))
    {
       
    }

    public async Task<IEnumerable<Room>> GetAllSortedAsync(bool noTracking = true)
    {
        return (await Repository.GetAllSortedAsync()).Select(e => EntityMapper.Map(e))!;
    }

    public async Task<Room?> FindWithDetailsAsync(Guid id, bool noTracking = true)
    {
        return EntityMapper.Map(await Repository.FindWithDetailsAsync(id));
    }
    
    public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime startDate, DateTime endDate, int guestCount, bool noTracking = true)
    {
        return (await Repository.GetAvailableRoomsAsync(startDate, endDate, guestCount, noTracking)).Select(e => EntityMapper.Map(e))!;
    }
}
