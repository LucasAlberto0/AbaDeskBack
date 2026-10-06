using AbaDeskBack.DTOs.Users;
using AbaDeskBack.Enums;
using System;

namespace AbaDeskBack.DTOs.Tests;

public class TestCaseResponse
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ExpectedResult { get; set; } = string.Empty;
    public string? ActualResult { get; set; }
    public TestCaseStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public UserResponse CreatedBy { get; set; } = null!;
}
