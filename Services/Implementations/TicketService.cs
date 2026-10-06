using AbaDeskBack.Data;
using AbaDeskBack.DTOs.Common;
using AbaDeskBack.DTOs.Tickets;
using AbaDeskBack.DTOs.Comments;
using AbaDeskBack.DTOs.History;
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

public class TicketService : ITicketService
{
    private readonly AppDbContext _context;

    public TicketService(AppDbContext context)
    {
        _context = context;
    }

    private async Task<string> GenerateProtocolNumberAsync()
    {
        // Simple logic for MVP: get max protocol number and increment
        var maxProtocol = await _context.Tickets.MaxAsync(t => (string?)t.ProtocolNumber);
        if (string.IsNullOrEmpty(maxProtocol) || !int.TryParse(maxProtocol, out int currentMax))
        {
            return "1000";
        }
        return (currentMax + 1).ToString();
    }

    private void EnsureUserCanAccessTicket(Ticket ticket, Guid userId, Role userRole)
    {
        if (userRole == Role.User && ticket.CreatedByUserId != userId && ticket.HomologationResponsibleUserId != userId)
        {
            throw new UnauthorizedAccessException("Você não tem permissão para acessar este chamado.");
        }
    }

    public async Task<TicketResponse> CreateTicketAsync(CreateTicketRequest request, Guid userId)
    {
        var protocol = await GenerateProtocolNumberAsync();

        var ticket = new Ticket
        {
            ProtocolNumber = protocol,
            Title = request.Title,
            Description = request.Description,
            CompanyUnit = request.CompanyUnit,
            Department = request.Department,
            SystemName = request.SystemName,
            Category = request.Category,
            Priority = request.Priority,
            Status = TicketStatus.NEW,
            CreatedByUserId = userId,
            HomologationResponsibleUserId = userId // By default, the creator is responsible for homologation
        };

        _context.Tickets.Add(ticket);

        var history = new TicketHistory
        {
            TicketId = ticket.Id,
            UserId = userId,
            Action = "Chamado criado",
            ToStatus = TicketStatus.NEW,
            Description = $"Chamado aberto no sistema: {request.Title}"
        };

        _context.TicketHistories.Add(history);
        await _context.SaveChangesAsync();

        return new TicketResponse
        {
            Id = ticket.Id,
            ProtocolNumber = ticket.ProtocolNumber,
            Title = ticket.Title,
            Description = ticket.Description,
            SystemName = ticket.SystemName,
            Category = ticket.Category,
            Priority = ticket.Priority,
            Status = ticket.Status,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt
        };
    }

    public async Task<PagedResult<TicketListItemResponse>> GetTicketsAsync(
        Guid userId,
        Role userRole,
        TicketStatus? status = null,
        Priority? priority = null,
        Category? category = null,
        string? systemName = null,
        string? department = null,
        Guid? assignedToUserId = null,
        Guid? createdByUserId = null,
        string? search = null,
        int page = 1,
        int pageSize = 20)
    {
        var query = _context.Tickets
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .AsNoTracking();

        if (userRole == Role.User)
        {
            query = query.Where(t => t.CreatedByUserId == userId || t.HomologationResponsibleUserId == userId);
        }

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(t => t.Priority == priority.Value);

        if (category.HasValue)
            query = query.Where(t => t.Category == category.Value);

        if (!string.IsNullOrEmpty(systemName))
            query = query.Where(t => t.SystemName.Contains(systemName));

        if (!string.IsNullOrEmpty(department))
            query = query.Where(t => t.Department.Contains(department));

        if (assignedToUserId.HasValue)
            query = query.Where(t => t.AssignedToUserId == assignedToUserId.Value);

        if (createdByUserId.HasValue)
            query = query.Where(t => t.CreatedByUserId == createdByUserId.Value);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(t => 
                t.Title.Contains(search) || 
                t.ProtocolNumber.Contains(search) ||
                t.Description.Contains(search));
        }

        var totalItems = await query.CountAsync();
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TicketListItemResponse
            {
                Id = t.Id,
                ProtocolNumber = t.ProtocolNumber,
                Title = t.Title,
                SystemName = t.SystemName,
                Category = t.Category,
                Priority = t.Priority,
                Status = t.Status,
                CreatedAt = t.CreatedAt,
                CreatedByName = t.CreatedByUser!.Name,
                AssignedToName = t.AssignedToUser != null ? t.AssignedToUser.Name : null
            })
            .ToListAsync();

        return new PagedResult<TicketListItemResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<PagedResult<TicketListItemResponse>> GetMyTicketsAsync(
        Guid userId,
        TicketStatus? status = null,
        string? search = null,
        int page = 1,
        int pageSize = 20)
    {
        return await GetTicketsAsync(
            userId: userId, 
            userRole: Role.User, // Force Role.User logic to show only their tickets
            status: status, 
            search: search, 
            page: page, 
            pageSize: pageSize);
    }

    public async Task<TicketDetailsResponse> GetTicketDetailsAsync(Guid ticketId, Guid userId, Role userRole)
    {
        var ticket = await _context.Tickets
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .Include(t => t.HomologationResponsibleUser)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
            throw new KeyNotFoundException("Chamado não encontrado.");

        EnsureUserCanAccessTicket(ticket, userId, userRole);

        return new TicketDetailsResponse
        {
            Id = ticket.Id,
            ProtocolNumber = ticket.ProtocolNumber,
            Title = ticket.Title,
            Description = ticket.Description,
            CompanyUnit = ticket.CompanyUnit,
            Department = ticket.Department,
            SystemName = ticket.SystemName,
            Category = ticket.Category,
            Priority = ticket.Priority,
            Status = ticket.Status,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            ResolvedAt = ticket.ResolvedAt,
            CreatedBy = new UserResponse
            {
                Id = ticket.CreatedByUser!.Id,
                Name = ticket.CreatedByUser.Name,
                Email = ticket.CreatedByUser.Email,
                Role = ticket.CreatedByUser.Role,
                IsActive = ticket.CreatedByUser.IsActive
            },
            AssignedTo = ticket.AssignedToUser != null ? new UserResponse
            {
                Id = ticket.AssignedToUser.Id,
                Name = ticket.AssignedToUser.Name,
                Email = ticket.AssignedToUser.Email,
                Role = ticket.AssignedToUser.Role,
                IsActive = ticket.AssignedToUser.IsActive
            } : null,
            HomologationResponsible = ticket.HomologationResponsibleUser != null ? new UserResponse
            {
                Id = ticket.HomologationResponsibleUser.Id,
                Name = ticket.HomologationResponsibleUser.Name,
                Email = ticket.HomologationResponsibleUser.Email,
                Role = ticket.HomologationResponsibleUser.Role,
                IsActive = ticket.HomologationResponsibleUser.IsActive
            } : null
        };
    }

    public async Task<CommentResponse> AddCommentAsync(Guid ticketId, CreateCommentRequest request, Guid userId)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        
        if (ticket == null)
            throw new KeyNotFoundException("Chamado não encontrado.");

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new UnauthorizedAccessException("Usuário inválido.");

        EnsureUserCanAccessTicket(ticket, userId, user.Role);

        var comment = new TicketComment
        {
            TicketId = ticketId,
            UserId = userId,
            Content = request.Content
        };

        _context.TicketComments.Add(comment);
        
        ticket.UpdatedAt = DateTimeOffset.UtcNow;
        _context.Tickets.Update(ticket);
        
        await _context.SaveChangesAsync();

        return new CommentResponse
        {
            Id = comment.Id,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            User = new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive
            }
        };
    }

    public async Task<List<CommentResponse>> GetCommentsAsync(Guid ticketId, Guid userId, Role userRole)
    {
        var ticket = await _context.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null)
            throw new KeyNotFoundException("Chamado não encontrado.");

        EnsureUserCanAccessTicket(ticket, userId, userRole);

        return await _context.TicketComments
            .Include(c => c.User)
            .Where(c => c.TicketId == ticketId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentResponse
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                User = new UserResponse
                {
                    Id = c.User!.Id,
                    Name = c.User.Name,
                    Email = c.User.Email,
                    Role = c.User.Role,
                    IsActive = c.User.IsActive
                }
            })
            .ToListAsync();
    }

    public async Task<List<HistoryResponse>> GetHistoryAsync(Guid ticketId, Guid userId, Role userRole)
    {
        var ticket = await _context.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null)
            throw new KeyNotFoundException("Chamado não encontrado.");

        EnsureUserCanAccessTicket(ticket, userId, userRole);

        return await _context.TicketHistories
            .Include(h => h.User)
            .Where(h => h.TicketId == ticketId)
            .OrderBy(h => h.CreatedAt)
            .Select(h => new HistoryResponse
            {
                Id = h.Id,
                Action = h.Action,
                FromStatus = h.FromStatus,
                ToStatus = h.ToStatus,
                Description = h.Description,
                CreatedAt = h.CreatedAt,
                User = new UserResponse
                {
                    Id = h.User!.Id,
                    Name = h.User.Name,
                    Email = h.User.Email,
                    Role = h.User.Role,
                    IsActive = h.User.IsActive
                }
            })
            .ToListAsync();
    }
}
