namespace Domain.Constants;

public static class RoleIds
{
    // Системный админ (полный доступ внутри срм)
    public static readonly Guid SystemAdmin = Guid.Parse("a0b0c0d0-e0f0-0a0b-b060-0e0f0a0b0c0d");
    // Владелец компании (полный доступ внутри своей компании)
    public static readonly Guid CompanyOwner = Guid.Parse("a1b2c3d4-e5f6-4a5b-bc6d-7e8f9a0b1c2d");

    // Менеджер (работа с клиентами и заказами, без настроек компании)
    public static readonly Guid Manager = Guid.Parse("b2c3d4e5-f6a7-5b6c-cd7e-8f9a0b1c2d3e");

    // Сотрудник/Исполнитель (ограниченный доступ только к своим задачам/заказам)
    public static readonly Guid Employee = Guid.Parse("c3d4e5f6-a7b8-6c7d-de8f-9a0b1c2d3e4f");
}

public static class RoleNames
{
    public const string SystemAdmin = "SystemAdmin";
    public const string CompanyOwner = "CompanyOwner";
    public const string Manager = "Manager";
    public const string Employee = "Employee";
}