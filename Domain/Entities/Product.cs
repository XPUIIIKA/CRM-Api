using Domain.Abstractions;
using Domain.BaseEntities;

namespace Domain.Entities;

public class Product : BaseEntity, IHaveCompany
{
    public Guid CompanyId { get; set; }
    public Guid? CategoryId { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public Guid CreatedBy { get; protected set; }

    protected Product() { }

    public Product(Guid companyId, Guid categoryId, string name, decimal price, Guid createdBy)
    {
        Id = Guid.NewGuid();
        CompanyId = companyId;
        CategoryId = categoryId;
        Name = name;
        Price = price;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public void ChangePrice(decimal price)
    {
        Price = price;
        UpdatedAt = DateTime.UtcNow;
    }
}