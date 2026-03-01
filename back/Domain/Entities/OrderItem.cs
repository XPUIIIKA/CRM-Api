using Domain.BaseEntities;
using Domain.Abstractions;

namespace Domain.Entities;

public class OrderItem : BaseEntity, IHaveCompany
{
    public Guid CompanyId { get; set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal Price { get; private set; }

    protected OrderItem() { }

    internal OrderItem(Guid orderId, Guid companyId, Guid productId, int quantity, decimal price)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        CompanyId = companyId;
        ProductId = productId;
        Quantity = quantity > 0 ? quantity : 1;
        Price = price < 0 ? 0 : price;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    internal bool TryUpdateQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            return false;
        }

        Quantity = quantity;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    internal bool TryUpdatePrice(decimal price)
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
