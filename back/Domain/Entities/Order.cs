using Domain.Abstractions;
using Domain.BaseEntities;
using Domain.Constants;

namespace Domain.Entities;

public class Order : BaseEntity, IHaveCompany
{
    public Guid CompanyId { get; set; } 
    public Guid? ClientId { get; private set; }
    public Guid? CurrentStatusId { get; private set; }
    public Guid? AssignedManagerId { get; private set; }
    public string DeliveryAddress { get; private set; } = string.Empty;
    public string Notes { get; private set; } = string.Empty;
    public string SalesChannel { get; private set; } = "Unknown";
    public Guid CreatedBy { get; protected set; } 

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items;

    protected Order() { }

    public Order(Guid createdBy, Guid companyId)
    {
        Id = Guid.NewGuid();
        CompanyId = companyId;
        CreatedBy = createdBy;
        CurrentStatusId = OrderStatusIds.Draft;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public void AssignClient(Guid clientId)
    {
        ClientId = clientId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveClient()
    {
        ClientId = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateMainInfo(string? deliveryAddress, string? notes, string? salesChannel = null)
    {
        DeliveryAddress = deliveryAddress?.Trim() ?? string.Empty;
        Notes = notes?.Trim() ?? string.Empty;
        SalesChannel = string.IsNullOrWhiteSpace(salesChannel) ? "Unknown" : salesChannel.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeStatus(Guid statusId)
    {
        CurrentStatusId = statusId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignManager(Guid managerId)
    {
        AssignedManagerId = managerId;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool TryAddItem(Guid productId, int quantity, decimal price)
    {
        return TryAddItem(productId, quantity, price, out _);
    }

    public bool TryAddItem(Guid productId, int quantity, decimal price, out OrderItem? addedItem)
    {
        addedItem = null;

        if (quantity <= 0 || price < 0)
        {
            return false;
        }

        var existingItem = _items.FirstOrDefault(x => x.ProductId == productId);

        if (existingItem != null)
        {
            if (!existingItem.TryUpdateQuantity(existingItem.Quantity + quantity) ||
                !existingItem.TryUpdatePrice(price))
            {
                return false;
            }
        }
        else
        {
            var newItem = new OrderItem(Id, CompanyId, productId, quantity, price);
            _items.Add(newItem);
            addedItem = newItem;
        }

        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public void ClearItems()
    {
        _items.Clear();
        UpdatedAt = DateTime.UtcNow;
    }
}
