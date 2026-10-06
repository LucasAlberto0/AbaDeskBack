using AbaDeskBack.DTOs.Tests;
using AbaDeskBack.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AbaDeskBack.Services.Interfaces;

public interface ITestCaseService
{
    Task<List<TestCaseResponse>> GetTestCasesAsync(Guid ticketId, Guid userId, Role userRole);
    Task<TestCaseResponse> CreateTestCaseAsync(Guid ticketId, CreateTestCaseRequest request, Guid userId, Role userRole);
    Task<TestCaseResponse> UpdateTestCaseAsync(Guid testId, UpdateTestCaseRequest request, Guid userId, Role userRole);
    Task DeleteTestCaseAsync(Guid testId, Guid userId, Role userRole);
    Task ExecuteTestCaseAsync(Guid testId, ExecuteTestCaseRequest request, Guid userId, Role userRole);
}
