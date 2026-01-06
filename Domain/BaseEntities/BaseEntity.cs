namespace Domain.BaseEntities;

public class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime UpdatedAt { get; protected set; }
    public Guid CreatedBy { get; protected set; }
}