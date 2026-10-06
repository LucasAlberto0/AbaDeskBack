using AbaDeskBack.Enums;
using AbaDeskBack.DTOs.Users;
using System;

namespace AbaDeskBack.DTOs.Tickets;

public class TicketDetailsResponse
{
    public Guid Id { get; set; }
    public string ProtocolNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CompanyUnit { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string SystemName { get; set; } = string.Empty;
    
    public Category Category { get; set; }
    public Priority Priority { get; set; }
    public TicketStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }

    public UserResponse? CreatedBy { get; set; }
    public UserResponse? AssignedTo { get; set; }
    
    public List<AbaDeskBack.DTOs.Comments.CommentResponse> Comments { get; set; } = new();
    public List<AbaDeskBack.DTOs.History.HistoryResponse> Histories { get; set; } = new();
}
