using System;

namespace AbaDeskBack.Entities;

public class TicketComment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid TicketId { get; set; }
    public Ticket? Ticket { get; set; }
    
    public Guid UserId { get; set; }
    public User? User { get; set; }
    
    public string Content { get; set; } = string.Empty;
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}
