using AbaDeskBack.Enums;
using System;

namespace AbaDeskBack.Entities;

public class Homologation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid TicketId { get; set; }
    public Ticket? Ticket { get; set; }
    
    public Guid RequestedByUserId { get; set; }
    public User? RequestedByUser { get; set; }
    
    public Guid ResponsibleUserId { get; set; }
    public User? ResponsibleUser { get; set; }
    
    public HomologationStatus Status { get; set; } = HomologationStatus.Pending;
    public string? Comment { get; set; }
    
    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DecidedAt { get; set; }
    
    public Guid? DecidedByUserId { get; set; }
    public User? DecidedByUser { get; set; }
}
