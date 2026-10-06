using AbaDeskBack.DTOs.Users;
using AbaDeskBack.Enums;
using System;

namespace AbaDeskBack.DTOs.Homologations;

public class HomologationResponse
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public HomologationStatus Status { get; set; }
    public string Observations { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public UserResponse User { get; set; } = null!;
}
