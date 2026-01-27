using Domain.BaseEntities;

namespace Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal Price { get; private set; }

    protected OrderItem() { }

    internal OrderItem(Guid orderId, Guid productId, int quantity, decimal price)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        Price = price;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    internal void UpdateQuantity(int quantity)
    {
        Quantity = quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void UpdatePrice(decimal price)
    {
        Price = price;
        UpdatedAt = DateTime.UtcNow;
    }
}