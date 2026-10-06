using AbaDeskBack.Enums;
using System;

namespace AbaDeskBack.Entities;

public class TicketHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid TicketId { get; set; }
    public Ticket? Ticket { get; set; }
    
    public Guid UserId { get; set; }
    public User? User { get; set; }
    
    public string Action { get; set; } = string.Empty;
    public TicketStatus? FromStatus { get; set; }
    public TicketStatus? ToStatus { get; set; }
    public string Description { get; set; } = string.Empty;
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
