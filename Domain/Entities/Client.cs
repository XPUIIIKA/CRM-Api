using Domain.BaseEntities;

namespace Domain.Entities;

public class Client : BaseEntity
{
    public Guid CompanyId { get; private set; }
    public string FirstName { get; private set; }
    public string Surname { get; private set; }
    public string Patronymic { get; private set; }
    public string Phone { get; private set; }
    public string Email { get; private set; }
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