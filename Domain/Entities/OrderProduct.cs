using Domain.BaseEntities;

namespace Domain.Entities;

public class OrderProduct : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }

    public int Quantity { get; private set; }
    public decimal Price { get; private set; }

    protected OrderProduct() { }

    public OrderProduct(Guid orderId, Guid productId, int quantity, decimal price, Guid createdBy)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        Price = price;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }
}