using App.DTO.DAL;
using Base.Contracts.DAL;

namespace App.Contracts.DAL.Repositories;

public interface IHotelRepository : IBaseEntityRepository<Hotel>, IHotelRepositoryCustom<Hotel>
{
// add here custom methods for repo only
}

public interface IHotelRepositoryCustom<TEntity>
{
    //add here shared methods between repo and service
}