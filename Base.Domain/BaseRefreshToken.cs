using System.ComponentModel.DataAnnotations;

namespace Base.Domain;

public class BaseRefreshToken : BaseRefreshToken<Guid>
{
}

public class BaseRefreshToken<TKey> : EntityId<TKey>
    where TKey : struct, IEquatable<TKey>
{
    [MaxLength(64)] public string RefreshToken { get; set; } = Guid.NewGuid().ToString();

    public DateTime ExpirationDt { get; set; } = DateTime.UtcNow.AddDays(7);


    [MaxLength(64)] public string? PreviousRefreshToken { get; set; }

    public DateTime? PreviousExpirationDt { get; set; } = DateTime.UtcNow.AddDays(7);
}