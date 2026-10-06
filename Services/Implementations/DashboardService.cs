using AbaDeskBack.Data;
using AbaDeskBack.DTOs.Dashboard;
using AbaDeskBack.Enums;
using AbaDeskBack.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AbaDeskBack.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryResponse> GetSummaryAsync(Guid userId, Role userRole)
    {
        var query = _context.Tickets.AsNoTracking();

        if (userRole == Role.User)
        {
            query = query.Where(t => t.CreatedByUserId == userId || t.HomologationResponsibleUserId == userId);
        }

        var allTickets = await query.ToListAsync();

        var summary = new DashboardSummaryResponse
        {
            TotalTickets = allTickets.Count,
            OpenTickets = allTickets.Count(t => t.Status == TicketStatus.NEW),
            InProgressTickets = allTickets.Count(t => t.Status == TicketStatus.ANALYZING || t.Status == TicketStatus.IN_DEVELOPMENT || t.Status == TicketStatus.IN_TEST),
            WaitingHomologationTickets = allTickets.Count(t => t.Status == TicketStatus.WAITING_HOMOLOGATION),
            ResolvedTickets = allTickets.Count(t => t.Status == TicketStatus.RESOLVED)
        };

        if (userRole == Role.User)
        {
            summary.PendingMyAction = allTickets.Count(t => t.Status == TicketStatus.WAITING_HOMOLOGATION && t.HomologationResponsibleUserId == userId);
        }
        else
        {
            summary.PendingMyAction = allTickets.Count(t => 
                (t.Status == TicketStatus.NEW) ||
                (t.Status == TicketStatus.ANALYZING && t.AssignedToUserId == userId) ||
                (t.Status == TicketStatus.IN_DEVELOPMENT && t.AssignedToUserId == userId) ||
                (t.Status == TicketStatus.IN_TEST && t.AssignedToUserId == userId)
            );
        }

        return summary;
    }
}
