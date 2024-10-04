namespace Base.Contracts.Domain;

public interface IDomainAuditableEntity
{
    public string? UpdatedBy { get; set; }
    public DateTime UpdatedAtDt { get; set; }

    public string? CreatedBy { get; set; }
    public DateTime CreatedAtDt { get; set; }
}