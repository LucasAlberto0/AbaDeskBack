using AbaDeskBack.Data;
using AbaDeskBack.DTOs.Homologations;
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

public class HomologationService : IHomologationService
{
    private readonly AppDbContext _context;

    public HomologationService(AppDbContext context)
    {
        _context = context;
    }

    private void EnsureUserCanAccessTicket(Ticket ticket, Guid userId, Role userRole)
    {
        if (userRole == Role.User && ticket.CreatedByUserId != userId && ticket.HomologationResponsibleUserId != userId)
        {
            throw new UnauthorizedAccessException("Você não tem permissão para acessar este chamado.");
        }
    }

    public async Task<List<HomologationResponse>> GetHomologationsAsync(Guid ticketId, Guid userId, Role userRole)
    {
        var ticket = await _context.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null) throw new KeyNotFoundException("Chamado não encontrado.");

        EnsureUserCanAccessTicket(ticket, userId, userRole);

        return await _context.Homologations
            .Include(h => h.DecidedByUser)
            .Where(h => h.TicketId == ticketId && h.DecidedAt.HasValue)
            .OrderByDescending(h => h.DecidedAt)
            .Select(h => new HomologationResponse
            {
                Id = h.Id,
                TicketId = h.TicketId,
                Status = h.Status,
                Observations = h.Comment ?? string.Empty,
                CreatedAt = h.DecidedAt ?? h.RequestedAt,
                User = new UserResponse
                {
                    Id = h.DecidedByUser!.Id,
                    Name = h.DecidedByUser.Name,
                    Email = h.DecidedByUser.Email,
                    Role = h.DecidedByUser.Role,
                    IsActive = h.DecidedByUser.IsActive
                }
            })
            .ToListAsync();
    }

    public async Task<HomologationResponse> CreateHomologationAsync(Guid ticketId, CreateHomologationRequest request, Guid userId, Role userRole)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null) throw new KeyNotFoundException("Chamado não encontrado.");

        if (ticket.Status != TicketStatus.WAITING_HOMOLOGATION)
            throw new InvalidOperationException("O chamado não está aguardando homologação.");

        if (userRole == Role.User && ticket.HomologationResponsibleUserId != userId)
            throw new UnauthorizedAccessException("Apenas o responsável pela homologação pode registrar o parecer.");

        var homologation = new Homologation
        {
            TicketId = ticketId,
            RequestedByUserId = ticket.CreatedByUserId,
            ResponsibleUserId = ticket.HomologationResponsibleUserId ?? userId,
            DecidedByUserId = userId,
            Status = request.Status,
            Comment = request.Observations,
            RequestedAt = DateTimeOffset.UtcNow,
            DecidedAt = DateTimeOffset.UtcNow
        };

        _context.Homologations.Add(homologation);

        var historyAction = request.Status == HomologationStatus.Approved ? "Homologação Aprovada" : "Homologação Reprovada";
        var toStatus = request.Status == HomologationStatus.Approved ? TicketStatus.RESOLVED : TicketStatus.IN_DEVELOPMENT;

        var history = new TicketHistory
        {
            TicketId = ticketId,
            UserId = userId,
            Action = historyAction,
            FromStatus = ticket.Status,
            ToStatus = toStatus,
            Description = request.Observations
        };
        _context.TicketHistories.Add(history);

        ticket.Status = toStatus;
        if (toStatus == TicketStatus.RESOLVED)
            ticket.ResolvedAt = DateTimeOffset.UtcNow;
            
        ticket.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId);

        return new HomologationResponse
        {
            Id = homologation.Id,
            TicketId = homologation.TicketId,
            Status = homologation.Status,
            Observations = homologation.Comment ?? string.Empty,
            CreatedAt = homologation.DecidedAt ?? homologation.RequestedAt,
            User = new UserResponse
            {
                Id = user!.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive
            }
        };
    }
}
