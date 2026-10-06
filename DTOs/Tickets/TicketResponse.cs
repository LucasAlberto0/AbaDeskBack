using AbaDeskBack.Enums;
using System;

namespace AbaDeskBack.DTOs.Tickets;

public class TicketResponse
{
    public Guid Id { get; set; }
    public string ProtocolNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SystemName { get; set; } = string.Empty;
    public Category Category { get; set; }
    public Priority Priority { get; set; }
    public TicketStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
