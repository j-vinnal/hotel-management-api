
using Base.Contracts;

namespace Base.Domain;

public abstract class EntityId : EntityId<Guid>, IEntityId
{
}

public abstract class EntityId<TKey> : IEntityId<TKey>
    where TKey : struct, IEquatable<TKey>

{
    public TKey Id { get; set; } = default!;
}