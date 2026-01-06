using Domain.BaseEntities;

namespace Domain.Entities;

public class EntityTag : BaseEntity
{
    public Guid TagId { get; private set; }
    public Guid EntityId { get; private set; }
    public string EntityType { get; private set; }

    protected EntityTag() { }

    public EntityTag(Guid tagId, Guid entityId, string entityType, Guid createdBy)
    {
        Id = Guid.NewGuid();
        TagId = tagId;
        EntityId = entityId;
        EntityType = entityType;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }
}