using AbaDeskBack.Enums;
using System;
using System.Collections.Generic;

namespace AbaDeskBack.Entities;

public class Ticket
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ProtocolNumber { get; set; } = string.Empty; // e.g. 1042
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CompanyUnit { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string SystemName { get; set; } = string.Empty;
    
    public Category Category { get; set; }
    public Priority Priority { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.NEW;

    public Guid CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public Guid? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }

    public Guid? HomologationResponsibleUserId { get; set; }
    public User? HomologationResponsibleUser { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }

    public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    public ICollection<TicketHistory> History { get; set; } = new List<TicketHistory>();
    public ICollection<TestCase> TestCases { get; set; } = new List<TestCase>();
    public ICollection<Homologation> Homologations { get; set; } = new List<Homologation>();
}
