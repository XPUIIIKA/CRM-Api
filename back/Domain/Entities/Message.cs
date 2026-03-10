using Domain.Abstractions;
using Domain.BaseEntities;

namespace Domain.Entities;

public class Message : BaseEntity, IHaveCompany
{
    public Guid CompanyId { get; set; }
    public Guid DialogId { get; private set; }
    public Guid SenderUserId { get; private set; }
    public Guid ReceiverUserId { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }

    protected Message() { }

    public Message(
        Guid companyId,
        Guid dialogId,
        Guid senderUserId,
        Guid receiverUserId,
        string text)
    {
        Id = Guid.NewGuid();
        CompanyId = companyId;
        DialogId = dialogId;
        SenderUserId = senderUserId;
        ReceiverUserId = receiverUserId;
        Text = text;
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public void MarkAsRead()
    {
        if (IsRead)
        {
            return;
        }

        IsRead = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
