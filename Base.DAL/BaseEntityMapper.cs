using AutoMapper;
using Base.Contracts;

namespace Base.DAL;

public class BaseEntityMapper<TSource, TDest> : IEntityMapper<TSource, TDest>
    where TSource : class
    where TDest : class
{
    // Should use dependency injection here
    protected readonly IMapper Mapper;

    public BaseEntityMapper(IMapper mapper)
    {
        Mapper = mapper;
    }

    public virtual TSource? Map(TDest? entity)
    {
        return Mapper.Map<TSource>(entity);
    }

    public virtual TDest? Map(TSource? entity)
    {
        return Mapper.Map<TDest>(entity);
    }
}