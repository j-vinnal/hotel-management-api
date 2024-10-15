
using App.Contracts.BLL.Services;
using App.Contracts.DAL;
using App.Contracts.DAL.Repositories;
using App.DTO.BLL;
using AutoMapper;
using Base.BLL;


namespace App.BLL.Services;

public class HotelService : BaseEntityService<Hotel, DTO.DAL.Hotel, IHotelRepository>,
    IHotelService
{
    public HotelService(IAppUnitOfWork uow, IMapper mapper) : base(uow.HotelRepository,
        new BllDalMapper<App.DTO.DAL.Hotel, App.DTO.BLL.Hotel>(mapper))
    {
        
    }
}