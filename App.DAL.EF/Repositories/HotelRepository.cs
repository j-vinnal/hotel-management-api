using App.Contracts.DAL.Repositories;
using App.DTO.DAL;
using AutoMapper;
using Base.Contracts;
using Base.DAL.EF;
using Hotel = App.Domain.Hotel;

namespace App.DAL.EF.Repositories
{
    /// <summary>
    /// Repository for managing hotel entities in the application.
    /// </summary>
    public class HotelRepository : BaseEntityRepository<Hotel, DTO.DAL.Hotel, AppDbContext>, IHotelRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HotelRepository"/> class.
        /// </summary>
        /// <param name="dataContext">The application database context.</param>
        /// <param name="mapper">The mapper for converting between domain and DTO objects.</param>
        public HotelRepository(AppDbContext dataContext, IMapper mapper)
            : base(dataContext, new DalDomainMapper<Hotel, DTO.DAL.Hotel>(mapper)) { }
    }
}
