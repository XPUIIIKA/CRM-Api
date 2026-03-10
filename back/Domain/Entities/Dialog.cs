using Domain.Abstractions;
using Domain.BaseEntities;

namespace Domain.Entities;

public class Dialog : BaseEntity, IHaveCompany
{
    public Guid CompanyId { get; set; }
    public Guid CreatedBy { get; private set; }

    protected Dialog() { }

    public Dialog(Guid companyId, Guid createdBy)
    {
        Id = Guid.NewGuid();
        CompanyId = companyId;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
