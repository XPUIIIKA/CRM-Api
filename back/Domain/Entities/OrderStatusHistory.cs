using Domain.BaseEntities;
using Domain.Abstractions;

namespace Domain.Entities;

public class OrderStatusHistory : BaseEntity, IHaveCompany
{
    public Guid CompanyId { get; set; }
    public Guid OrderId { get; private set; }
    public Guid StatusId { get; private set; }
    public Guid CreatedBy { get; protected set; }
    protected OrderStatusHistory() { }

    public OrderStatusHistory(Guid orderId, Guid statusId, Guid createdBy, Guid companyId)
    {
        Id = Guid.NewGuid();
        CompanyId = companyId;
        OrderId = orderId;
        StatusId = statusId;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }
}
