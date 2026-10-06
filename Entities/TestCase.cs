using AbaDeskBack.Enums;
using System;

namespace AbaDeskBack.Entities;

public class TestCase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid TicketId { get; set; }
    public Ticket? Ticket { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ExpectedResult { get; set; } = string.Empty;
    public string? ActualResult { get; set; }
    
    public TestCaseStatus Status { get; set; } = TestCaseStatus.Pending;
    
    public Guid CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}
