namespace Base.Contracts.DAL;

public interface IBaseUOW
{
    Task<int> SaveChangesAsync();
}