using Domain.Abstractions;
using Domain.BaseEntities;

namespace Domain.Entities;

public class Product : BaseEntity, IHaveCompany
{
    public Guid CompanyId { get; set; }
    public Guid? CategoryId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public Guid CreatedBy { get; protected set; }
    public virtual Category? Category { get; private set; }

    protected Product() { }

    public Product(Guid companyId, Guid? categoryId, string name, decimal price, Guid createdBy)
    {
        Id = Guid.NewGuid();
        CompanyId = companyId;
        CategoryId = categoryId;
        Name = string.IsNullOrWhiteSpace(name) ? string.Empty : name.Trim();
        Price = price < 0 ? 0 : price;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public bool TryUpdateDetails(string name, decimal price, Guid? categoryId)
    {
        if (string.IsNullOrWhiteSpace(name) || price < 0)
        {
            return false;
        }

        Name = name.Trim();
        Price = price;
        CategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public bool TryChangePrice(decimal price)
    {
        if (price < 0)
        {
            return false;
        }

        Price = price;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }
}
