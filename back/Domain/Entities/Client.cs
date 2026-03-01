using Domain.Abstractions;
using Domain.BaseEntities;

namespace Domain.Entities;

public class Client : BaseEntity, IHaveCompany
{
    public Guid CompanyId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Patronymic { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid CreatedBy { get; protected set; }

    protected Client() { }

    public Client(Guid companyId, Guid createdBy)
    {
        Id = Guid.NewGuid();
        CompanyId = companyId;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public void UpdatePersonalData(string firstName, string surname, string patronymic, string phone, string email)
    {
        FirstName = firstName;
        Surname = surname;
        Patronymic = patronymic;
        Phone = phone;
        Email = email;
        UpdatedAt = DateTime.UtcNow;
    }
}
