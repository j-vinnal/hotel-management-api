using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Base.Contracts;
using Base.Contracts;
using Base.Contracts.Domain;

namespace Base.Domain;

public abstract class AuditableEntity : AuditableEntity<Guid>, IEntityId
{
}

public abstract class AuditableEntity<TKey> : EntityId<TKey>, IDomainAuditableEntity
    where TKey : struct, IEquatable<TKey>

{
    [ScaffoldColumn(false)] public string? UpdatedBy { get; set; }

    [ScaffoldColumn(false)] public DateTime UpdatedAtDt { get; set; } = DateTime.UtcNow;

    [ScaffoldColumn(false)] public string? CreatedBy { get; set; }

    [ScaffoldColumn(false)]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedAtDt { get; set; } = DateTime.UtcNow;
}