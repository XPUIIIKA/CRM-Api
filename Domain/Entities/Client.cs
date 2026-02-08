using Domain.Abstractions;
using Domain.BaseEntities;

namespace Domain.Entities;

public class Client : BaseEntity, IHaveCompany
{
    public Guid CompanyId { get; set; }
    public string FirstName { get; set; }
    public string Surname { get; set; }
    public string Patronymic { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
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