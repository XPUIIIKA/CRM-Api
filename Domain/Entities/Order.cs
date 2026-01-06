using Domain.BaseEntities;

namespace Domain.Entities;

public class Order : BaseEntity
{
    public Guid CompanyId { get; private set; }
    public Guid? ClientId { get; private set; }
    public Guid? CurrentStatusId { get; private set; }

    protected Order() { }

    public Order(Guid companyId, Guid createdBy)
    {
        Id = Guid.NewGuid();
        CompanyId = companyId;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public void AssignClient(Guid clientId)
    {
        ClientId = clientId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeStatus(Guid statusId)
    {
        CurrentStatusId = statusId;
        UpdatedAt = DateTime.UtcNow;
    }
}
