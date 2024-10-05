using App.Contracts.DAL.Repositories;
using App.DTO.DAL;
using AutoMapper;
using Base.Contracts;
using Base.DAL.EF;
using Hotel = App.Domain.Hotel;

namespace App.DAL.EF.Repositories;

public class HotelRepository : BaseEntityRepository<Hotel, DTO.DAL.Hotel, AppDbContext>, IHotelRepository
{
    public HotelRepository(AppDbContext dataContext, IMapper mapper) : base(dataContext,
        new DalDomainMapper<Hotel, DTO.DAL.Hotel>(mapper))
    {
    }
    
}