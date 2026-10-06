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
        if (userRole == Role.User && ticket.CreatedByUserId != userId)
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
            Status = TicketStatus.Open,
            CreatedByUserId = userId
        };

        _context.Tickets.Add(ticket);

        var history = new TicketHistory
        {
            TicketId = ticket.Id,
            UserId = userId,
            Action = "Chamado criado",
            ToStatus = TicketStatus.Open,
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
            query = query.Where(t => t.CreatedByUserId == userId);
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
            .Include(t => t.Comments).ThenInclude(c => c.User)
            .Include(t => t.History).ThenInclude(h => h.User)
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

            Comments = ticket.Comments.OrderBy(c => c.CreatedAt).Select(c => new CommentResponse
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
            }).ToList(),
            Histories = ticket.History.OrderByDescending(h => h.CreatedAt).Select(h => new HistoryResponse
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
            }).ToList()
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

    private void AddHistory(Ticket ticket, Guid userId, string action, string description, TicketStatus? fromStatus = null, TicketStatus? toStatus = null)
    {
        _context.TicketHistories.Add(new TicketHistory
        {
            TicketId = ticket.Id,
            UserId = userId,
            Action = action,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            Description = description
        });
    }

    public async Task StartAnalysisAsync(Guid ticketId, Guid userId, Role userRole)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null) throw new KeyNotFoundException("Chamado não encontrado.");
        if (userRole == Role.User) throw new UnauthorizedAccessException("Usuários não podem iniciar análise.");
        if (ticket.Status != TicketStatus.Open) throw new InvalidOperationException("O chamado não está no status Aberto.");

        ticket.Status = TicketStatus.InAnalysis;
        ticket.UpdatedAt = DateTimeOffset.UtcNow;
        AddHistory(ticket, userId, "Análise iniciada", "O chamado entrou em análise.", TicketStatus.Open, TicketStatus.InAnalysis);
        await _context.SaveChangesAsync();
    }

    public async Task AssignTicketAsync(Guid ticketId, Guid assignedToUserId, Guid userId, Role userRole)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null) throw new KeyNotFoundException("Chamado não encontrado.");
        if (userRole == Role.User) throw new UnauthorizedAccessException("Usuários não podem atribuir chamados.");

        var assignedUser = await _context.Users.FindAsync(assignedToUserId);
        if (assignedUser == null || assignedUser.Role == Role.User) throw new InvalidOperationException("Usuário atribuído inválido.");

        ticket.AssignedToUserId = assignedToUserId;
        ticket.UpdatedAt = DateTimeOffset.UtcNow;
        AddHistory(ticket, userId, "Responsável atribuído", $"O chamado foi atribuído para {assignedUser.Name}.");
        await _context.SaveChangesAsync();
    }

    public async Task StartProgressAsync(Guid ticketId, Guid userId, Role userRole)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null) throw new KeyNotFoundException("Chamado não encontrado.");
        if (userRole == Role.User) throw new UnauthorizedAccessException("Usuários não podem iniciar o atendimento.");
        if (ticket.Status != TicketStatus.InAnalysis && ticket.Status != TicketStatus.WaitingUser) 
            throw new InvalidOperationException("Apenas chamados em Análise ou Aguardando Usuário podem ir para Em Atendimento.");

        var fromStatus = ticket.Status;
        ticket.Status = TicketStatus.InProgress;
        ticket.UpdatedAt = DateTimeOffset.UtcNow;
        AddHistory(ticket, userId, "Atendimento iniciado", "O chamado está em atendimento.", fromStatus, TicketStatus.InProgress);
        await _context.SaveChangesAsync();
    }

    public async Task WaitForUserAsync(Guid ticketId, Guid userId, Role userRole)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null) throw new KeyNotFoundException("Chamado não encontrado.");
        if (userRole == Role.User) throw new UnauthorizedAccessException("Usuários não podem alterar para aguardando usuário.");
        if (ticket.Status != TicketStatus.InProgress) throw new InvalidOperationException("Apenas chamados Em Atendimento podem ir para Aguardando Usuário.");

        var fromStatus = ticket.Status;
        ticket.Status = TicketStatus.WaitingUser;
        ticket.UpdatedAt = DateTimeOffset.UtcNow;
        AddHistory(ticket, userId, "Aguardando Usuário", "O suporte solicitou informações ao usuário.", fromStatus, TicketStatus.WaitingUser);
        await _context.SaveChangesAsync();
    }

    public async Task ResolveTicketAsync(Guid ticketId, Guid userId, Role userRole)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null) throw new KeyNotFoundException("Chamado não encontrado.");
        
        if (ticket.Status != TicketStatus.InProgress) 
            throw new InvalidOperationException("Apenas chamados Em Atendimento podem ser resolvidos.");
            
        var fromStatus = ticket.Status;
        ticket.Status = TicketStatus.Resolved;
        ticket.ResolvedAt = DateTimeOffset.UtcNow;
        ticket.UpdatedAt = DateTimeOffset.UtcNow;
        AddHistory(ticket, userId, "Chamado resolvido", "O chamado foi finalizado.", fromStatus, TicketStatus.Resolved);
        await _context.SaveChangesAsync();
    }
}
