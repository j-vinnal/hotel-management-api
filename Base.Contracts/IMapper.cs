namespace Base.Contracts;

public interface IMapper<TSource, TDest>
    where TSource : class
    where TDest : class
{
    TSource? Map(TDest? entity);
    TDest? Map(TSource? entity);
}