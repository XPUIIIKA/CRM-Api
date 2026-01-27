using Domain.BaseEntities;
using Domain.Constants;

namespace Domain.Entities;

public class Order : BaseEntity
{
    public Guid CompanyId { get; private set; }
    public Guid? ClientId { get; private set; }
    public Guid? CurrentStatusId { get; private set; }
    public Guid CreatedBy { get; protected set; } 
    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
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
    
    public void AddItem(Guid productId, int quantity, decimal price)
    {
        var existingItem = _items.FirstOrDefault(x => x.ProductId == productId);

        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
            existingItem.UpdatePrice(price);
        }
        else
        {
            var newItem = new OrderItem(Id, productId, quantity, price);
            _items.Add(newItem);
        }
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SetCustomer(Guid customerId)
    {
        if (ClientId == customerId)
            return;

        ClientId = customerId;
    
        UpdatedAt = DateTime.UtcNow;
    }
    
    public bool CanBeConfirmed => 
        Items.Any() && 
        ClientId.HasValue && 
        CurrentStatusId == OrderStatusIds.Draft;

    public void Confirm()
    {
        if (!CanBeConfirmed) return;

        CurrentStatusId = OrderStatusIds.Active;
        UpdatedAt = DateTime.UtcNow;
    }
}
