namespace Domain.Constants;

public static class OrderStatusIds
{
    public static readonly Guid Draft = Guid.Parse("d1e2f3a4-b5c6-4d5e-8f9a-0b1c2d3e4f5a");
    public static readonly Guid Active = Guid.Parse("e2f3a4b5-c6d7-4e5f-9a0b-1c2d3e4f5a6b");
    public static readonly Guid Completed = Guid.Parse("f3a4b5c6-d7e8-4f5a-0b1c-2d3e4f5a6b7c");
    public static readonly Guid Cancelled = Guid.Parse("a4b5c6d7-e8f9-4a5b-1c2d-3e4f5a6b7c8d");
}

public static class OrderStatusNames
{
    public const string Draft = "Draft";
    public const string Active = "Active";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
}