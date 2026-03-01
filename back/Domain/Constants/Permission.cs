namespace Domain.Constants;

public enum Permission
{
    // --- Управление системой (только для владельца/админа) ---
    ConfigureCompany = 1,      // Настройка компании
    ManageRoles = 2,           // Создание и редактирование ролей
    ManageUsers = 3,           // Приглашение сотрудников, увольнение

    // --- Клиенты (Clients) ---
    ViewClients = 10,          // Видеть список клиентов
    CreateClient = 11,         // Добавлять нового клиента
    EditClient = 12,           // Редактировать данные клиента
    DeleteClient = 13,         // Удалять клиента
    ExportClients = 14,        // Выгрузка базы в Excel/CSV

    // --- Заказы / Сделки (Orders) ---
    ViewAllOrders = 20,        // Видеть заказы всех сотрудников
    ViewOwnOrders = 21,        // Видеть только те заказы, где ты ответственный
    ManageOrders = 22,            // Редактировать заказ
    
    ViewProducts = 26,
    ManageProducts = 27,
    
    // --- Аналитика и финансы ---
    ViewAnalytics = 30,        // Доступ к графикам и отчетам по прибыли
    ViewFinancials = 31,       // Доступ к кассе, расходам и доходам
    
    // --- Коммуникации ---
    ManageTasks = 40,          // Ставить задачи себе и другим
    ViewLogs = 50              // Просмотр истории изменений (аудит)
}