using AbaDeskBack.Enums;

namespace AbaDeskBack.DTOs.Notifications;

public class NotificationResponse
{
    public Guid Id { get; set; }
    public Guid? TicketId { get; set; }
    public string? TicketProtocol { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
