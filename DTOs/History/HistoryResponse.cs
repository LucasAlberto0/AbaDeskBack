using AbaDeskBack.Enums;
using AbaDeskBack.DTOs.Users;
using System;

namespace AbaDeskBack.DTOs.History;

public class HistoryResponse
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public TicketStatus? FromStatus { get; set; }
    public TicketStatus? ToStatus { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public UserResponse User { get; set; } = null!;
}
