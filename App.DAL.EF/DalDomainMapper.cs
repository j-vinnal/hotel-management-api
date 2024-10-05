using AutoMapper;
using Base.Contracts;

namespace App.DAL.EF;

public class DalDomainMapper<TSource, TDest> : IEntityMapper<TSource, TDest>
    where TSource : class where TDest : class
{
    private readonly IMapper _mapper;
    
    public DalDomainMapper(IMapper mapper)
    {
        _mapper = mapper;
    }
    public TSource? Map(TDest? inObject)
    {
        return _mapper.Map<TSource>(inObject);
    }

    public TDest? Map(TSource? inObject)
    {
        return _mapper.Map<TDest>(inObject);
    }
}