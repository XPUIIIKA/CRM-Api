using Domain.BaseEntities;

namespace Domain.Entities;

public class OrderStatusHistory : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid StatusId { get; private set; }
    public Guid CreatedBy { get; protected set; }
    protected OrderStatusHistory() { }

    public OrderStatusHistory(Guid orderId, Guid statusId, Guid createdBy)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        StatusId = statusId;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }
}