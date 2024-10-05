using App.Contracts.DAL.Repositories;
using App.DTO.BLL;
using Base.Contracts.DAL;

namespace App.Contracts.BLL.Services;

public interface IRoomService : IBaseEntityRepository<Room>, IRoomRepositoryCustom<Room>
{
    // Define additional methods if needed
}