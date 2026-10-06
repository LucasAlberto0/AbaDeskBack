using AbaDeskBack.Enums;
using System.ComponentModel.DataAnnotations;

namespace AbaDeskBack.DTOs.Tests;

public class ExecuteTestCaseRequest
{
    public string? ActualResult { get; set; }

    [Required]
    public TestCaseStatus Status { get; set; }
}
