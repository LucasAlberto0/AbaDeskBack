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
            query = query.Where(t => t.CreatedByUserId == userId);
        }

        var allTickets = await query.ToListAsync();

        var summary = new DashboardSummaryResponse
        {
            TotalTickets = allTickets.Count,
            OpenTickets = allTickets.Count(t => t.Status == TicketStatus.Open),
            InProgressTickets = allTickets.Count(t => t.Status == TicketStatus.InProgress),
            InAnalysisTickets = allTickets.Count(t => t.Status == TicketStatus.InAnalysis),
            WaitingUserTickets = allTickets.Count(t => t.Status == TicketStatus.WaitingUser),
            ResolvedTickets = allTickets.Count(t => t.Status == TicketStatus.Resolved),
            ResolutionRate = allTickets.Count > 0 ? Math.Round((double)allTickets.Count(t => t.Status == TicketStatus.Resolved) / allTickets.Count * 100, 1) : 0
        };

        if (userRole == Role.User)
        {
            summary.PendingMyAction = allTickets.Count(t => t.Status == TicketStatus.WaitingUser);
        }
        else
        {
            summary.PendingMyAction = allTickets.Count(t => 
                (t.Status == TicketStatus.Open) ||
                (t.Status == TicketStatus.InAnalysis && t.AssignedToUserId == userId) ||
                (t.Status == TicketStatus.InProgress && t.AssignedToUserId == userId)
            );
        }

        return summary;
    }
}
