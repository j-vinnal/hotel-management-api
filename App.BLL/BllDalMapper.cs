using AutoMapper;
using Base.Contracts;
using Base.Contracts.BLL;
using Base.Contracts.DAL;

namespace App.BLL;

public class BllDalMapper<TSource, TDest> : IEntityMapper<TSource, TDest> 
    where TSource : class 
    where TDest : class
{
    private readonly IMapper _mapper;

    public BllDalMapper(IMapper mapper)
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