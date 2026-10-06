using AbaDeskBack.Data;
using AbaDeskBack.DTOs.Tests;
using AbaDeskBack.DTOs.Users;
using AbaDeskBack.Entities;
using AbaDeskBack.Enums;
using AbaDeskBack.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AbaDeskBack.Services.Implementations;

public class TestCaseService : ITestCaseService
{
    private readonly AppDbContext _context;

    public TestCaseService(AppDbContext context)
    {
        _context = context;
    }

    private void EnsureManagePermissions(Role userRole, TicketStatus status)
    {
        if (userRole == Role.User)
            throw new UnauthorizedAccessException("Usuários não podem gerenciar casos de teste.");
            
        if (status != TicketStatus.IN_TEST)
            throw new InvalidOperationException("Os testes só podem ser gerenciados quando o chamado estiver Em Teste (IN_TEST).");
    }

    private void EnsureUserCanAccessTicket(Ticket ticket, Guid userId, Role userRole)
    {
        if (userRole == Role.User && ticket.CreatedByUserId != userId && ticket.HomologationResponsibleUserId != userId)
        {
            throw new UnauthorizedAccessException("Você não tem permissão para acessar este chamado.");
        }
    }

    public async Task<List<TestCaseResponse>> GetTestCasesAsync(Guid ticketId, Guid userId, Role userRole)
    {
        var ticket = await _context.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null) throw new KeyNotFoundException("Chamado não encontrado.");

        EnsureUserCanAccessTicket(ticket, userId, userRole);

        return await _context.TestCases
            .Include(tc => tc.CreatedByUser)
            .Where(tc => tc.TicketId == ticketId)
            .OrderBy(tc => tc.CreatedAt)
            .Select(tc => new TestCaseResponse
            {
                Id = tc.Id,
                TicketId = tc.TicketId,
                Title = tc.Title,
                Description = tc.Description,
                ExpectedResult = tc.ExpectedResult,
                ActualResult = tc.ActualResult,
                Status = tc.Status,
                CreatedAt = tc.CreatedAt,
                UpdatedAt = tc.UpdatedAt,
                CreatedBy = new UserResponse
                {
                    Id = tc.CreatedByUser!.Id,
                    Name = tc.CreatedByUser.Name,
                    Email = tc.CreatedByUser.Email,
                    Role = tc.CreatedByUser.Role,
                    IsActive = tc.CreatedByUser.IsActive
                }
            })
            .ToListAsync();
    }

    public async Task<TestCaseResponse> CreateTestCaseAsync(Guid ticketId, CreateTestCaseRequest request, Guid userId, Role userRole)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null) throw new KeyNotFoundException("Chamado não encontrado.");

        EnsureManagePermissions(userRole, ticket.Status);

        var testCase = new TestCase
        {
            TicketId = ticketId,
            Title = request.Title,
            Description = request.Description,
            ExpectedResult = request.ExpectedResult,
            Status = TestCaseStatus.Pending,
            CreatedByUserId = userId
        };

        _context.TestCases.Add(testCase);
        
        var history = new TicketHistory
        {
            TicketId = ticketId,
            UserId = userId,
            Action = "Caso de teste criado",
            Description = $"Caso de teste adicionado: {request.Title}"
        };
        _context.TicketHistories.Add(history);
        
        await _context.SaveChangesAsync();

        var createdUser = await _context.Users.FindAsync(userId);

        return new TestCaseResponse
        {
            Id = testCase.Id,
            TicketId = testCase.TicketId,
            Title = testCase.Title,
            Description = testCase.Description,
            ExpectedResult = testCase.ExpectedResult,
            ActualResult = testCase.ActualResult,
            Status = testCase.Status,
            CreatedAt = testCase.CreatedAt,
            UpdatedAt = testCase.UpdatedAt,
            CreatedBy = new UserResponse
            {
                Id = createdUser!.Id,
                Name = createdUser.Name,
                Email = createdUser.Email,
                Role = createdUser.Role,
                IsActive = createdUser.IsActive
            }
        };
    }

    public async Task<TestCaseResponse> UpdateTestCaseAsync(Guid testId, UpdateTestCaseRequest request, Guid userId, Role userRole)
    {
        var testCase = await _context.TestCases.Include(tc => tc.Ticket).Include(tc => tc.CreatedByUser).FirstOrDefaultAsync(tc => tc.Id == testId);
        if (testCase == null) throw new KeyNotFoundException("Caso de teste não encontrado.");

        EnsureManagePermissions(userRole, testCase.Ticket!.Status);

        testCase.Title = request.Title;
        testCase.Description = request.Description;
        testCase.ExpectedResult = request.ExpectedResult;
        testCase.UpdatedAt = DateTimeOffset.UtcNow;
        
        await _context.SaveChangesAsync();

        return new TestCaseResponse
        {
            Id = testCase.Id,
            TicketId = testCase.TicketId,
            Title = testCase.Title,
            Description = testCase.Description,
            ExpectedResult = testCase.ExpectedResult,
            ActualResult = testCase.ActualResult,
            Status = testCase.Status,
            CreatedAt = testCase.CreatedAt,
            UpdatedAt = testCase.UpdatedAt,
            CreatedBy = new UserResponse
            {
                Id = testCase.CreatedByUser!.Id,
                Name = testCase.CreatedByUser.Name,
                Email = testCase.CreatedByUser.Email,
                Role = testCase.CreatedByUser.Role,
                IsActive = testCase.CreatedByUser.IsActive
            }
        };
    }

    public async Task DeleteTestCaseAsync(Guid testId, Guid userId, Role userRole)
    {
        var testCase = await _context.TestCases.Include(tc => tc.Ticket).FirstOrDefaultAsync(tc => tc.Id == testId);
        if (testCase == null) throw new KeyNotFoundException("Caso de teste não encontrado.");

        EnsureManagePermissions(userRole, testCase.Ticket!.Status);

        _context.TestCases.Remove(testCase);
        
        var history = new TicketHistory
        {
            TicketId = testCase.TicketId,
            UserId = userId,
            Action = "Caso de teste excluído",
            Description = $"Caso de teste removido: {testCase.Title}"
        };
        _context.TicketHistories.Add(history);
        
        await _context.SaveChangesAsync();
    }

    public async Task ExecuteTestCaseAsync(Guid testId, ExecuteTestCaseRequest request, Guid userId, Role userRole)
    {
        var testCase = await _context.TestCases.Include(tc => tc.Ticket).FirstOrDefaultAsync(tc => tc.Id == testId);
        if (testCase == null) throw new KeyNotFoundException("Caso de teste não encontrado.");

        EnsureManagePermissions(userRole, testCase.Ticket!.Status);

        testCase.ActualResult = request.ActualResult;
        testCase.Status = request.Status;
        testCase.UpdatedAt = DateTimeOffset.UtcNow;
        
        var history = new TicketHistory
        {
            TicketId = testCase.TicketId,
            UserId = userId,
            Action = "Caso de teste executado",
            Description = $"Teste '{testCase.Title}' marcado como {request.Status}."
        };
        _context.TicketHistories.Add(history);
        
        await _context.SaveChangesAsync();
    }
}
