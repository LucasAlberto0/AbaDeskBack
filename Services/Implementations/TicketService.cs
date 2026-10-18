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
    private readonly INotificationService _notificationService;

    public TicketService(AppDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    private async Task<string> GenerateProtocolNumberAsync()
    {
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

    public async Task<TicketResponse> CreateTicketAsync(CreateTicketRequest request, Guid userId, Role userRole)
    {
        if (userRole == Role.Attendant)
            throw new UnauthorizedAccessException("Suporte técnico não tem permissão para criar chamados.");
        var protocol = await GenerateProtocolNumberAsync();

        byte[]? attachmentData = null;
        string? attachmentContentType = null;
        string? attachmentFileName = null;

        if (!string.IsNullOrEmpty(request.AttachmentBase64))
        {
            try
            {
                var base64String = request.AttachmentBase64;
                if (base64String.Contains(","))
                {
                    var parts = base64String.Split(',');
                    var header = parts[0];
                    if (header.Contains("image/png")) attachmentContentType = "image/png";
                    else if (header.Contains("image/webp")) attachmentContentType = "image/webp";
                    else if (header.Contains("image/gif")) attachmentContentType = "image/gif";
                    else attachmentContentType = "image/jpeg";

                    base64String = parts[1];
                }
                else
                {
                    attachmentContentType = "image/jpeg";
                }

                attachmentData = Convert.FromBase64String(base64String);
                var extension = attachmentContentType switch
                {
                    "image/png" => ".png",
                    "image/webp" => ".webp",
                    "image/gif" => ".gif",
                    _ => ".jpg"
                };
                attachmentFileName = $"anexo-{Guid.NewGuid()}{extension}";
            }
            catch (Exception)
            {
            }
        }

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
            AttachmentData = attachmentData,
            AttachmentContentType = attachmentContentType,
            AttachmentFileName = attachmentFileName,
            CreatedByUserId = userId
        };
        ticket.AttachmentUrl = attachmentData != null ? $"/api/tickets/{ticket.Id}/attachment" : string.Empty;

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
            var searchLower = search.ToLower();
            query = query.Where(t => 
                t.Title.ToLower().Contains(searchLower) || 
                t.ProtocolNumber.ToLower().Contains(searchLower) ||
                t.Description.ToLower().Contains(searchLower) ||
                (t.CreatedByUser != null && t.CreatedByUser.Name.ToLower().Contains(searchLower)));
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
                AssignedToName = t.AssignedToUser != null ? t.AssignedToUser.Name : null,
                AssignedToUserId = t.AssignedToUserId,
                Solution = t.Solution
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
            userRole: Role.User,
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
            AttachmentUrl = ticket.AttachmentUrl,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            ResolvedAt = ticket.ResolvedAt,
            Solution = ticket.Solution,
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

        if (userId != ticket.CreatedByUserId)
        {
            var preview = request.Content.Length > 60 ? request.Content.Substring(0, 57) + "..." : request.Content;
            await _notificationService.CreateNotificationAsync(
                ticket.CreatedByUserId,
                ticket.Id,
                $"Nova resposta no chamado #{ticket.ProtocolNumber}",
                $"{user.Name}: {preview}",
                NotificationType.NewComment);
        }
        else if (ticket.AssignedToUserId.HasValue && userId != ticket.AssignedToUserId.Value)
        {
            var preview = request.Content.Length > 60 ? request.Content.Substring(0, 57) + "..." : request.Content;
            await _notificationService.CreateNotificationAsync(
                ticket.AssignedToUserId.Value,
                ticket.Id,
                $"Novo comentário no chamado #{ticket.ProtocolNumber}",
                $"{user.Name}: {preview}",
                NotificationType.NewComment);
        }

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
        if (userRole == Role.User) throw new UnauthorizedAccessException("Usuários não possuem permissão para iniciar análise de chamados.");
        if (ticket.Status != TicketStatus.Open) throw new InvalidOperationException($"Transição inválida: apenas chamados com status Aberto podem entrar em análise (status atual: {ticket.Status}).");

        var fromStatus = ticket.Status;
        ticket.Status = TicketStatus.InAnalysis;
        ticket.UpdatedAt = DateTimeOffset.UtcNow;
        AddHistory(ticket, userId, "Análise iniciada", "O chamado entrou em análise técnica.", fromStatus, TicketStatus.InAnalysis);
        await _context.SaveChangesAsync();

        if (userId != ticket.CreatedByUserId)
        {
            await _notificationService.CreateNotificationAsync(
                ticket.CreatedByUserId,
                ticket.Id,
                $"Chamado #{ticket.ProtocolNumber} em análise",
                "Seu chamado entrou em fase de análise técnica.",
                NotificationType.StatusChanged);
        }
    }

    public async Task AssignTicketAsync(Guid ticketId, Guid assignedToUserId, Guid userId, Role userRole)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null) throw new KeyNotFoundException("Chamado não encontrado.");
        if (userRole == Role.User) throw new UnauthorizedAccessException("Usuários não podem atribuir chamados.");

        var assignedUser = await _context.Users.FindAsync(assignedToUserId);
        if (assignedUser == null || assignedUser.Role == Role.User || !assignedUser.IsActive) 
            throw new InvalidOperationException("Usuário atribuído inválido.");

        ticket.AssignedToUserId = assignedToUserId;
        ticket.UpdatedAt = DateTimeOffset.UtcNow;
        AddHistory(ticket, userId, "Responsável atribuído", $"O chamado foi atribuído para {assignedUser.Name}.");
        await _context.SaveChangesAsync();

        if (assignedToUserId != userId)
        {
            await _notificationService.CreateNotificationAsync(
                assignedToUserId,
                ticket.Id,
                $"Novo chamado atribuído: #{ticket.ProtocolNumber}",
                $"Você foi designado como responsável pelo chamado \"{ticket.Title}\".",
                NotificationType.StatusChanged);
        }
    }

    public async Task StartProgressAsync(Guid ticketId, Guid userId, Role userRole, Guid? assignedToUserId = null)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null) throw new KeyNotFoundException("Chamado não encontrado.");
        if (userRole == Role.User) throw new UnauthorizedAccessException("Usuários não podem iniciar o atendimento.");
        if (ticket.Status != TicketStatus.Open && ticket.Status != TicketStatus.InAnalysis && ticket.Status != TicketStatus.WaitingUser) 
            throw new InvalidOperationException($"Transição inválida: chamados com status {ticket.Status} não podem ser movidos para Em Atendimento.");

        if (assignedToUserId.HasValue)
        {
            var targetUser = await _context.Users.FindAsync(assignedToUserId.Value);
            if (targetUser == null || targetUser.Role == Role.User || !targetUser.IsActive)
                throw new InvalidOperationException("O responsável designado deve ser um técnico ou administrador ativo.");
            ticket.AssignedToUserId = assignedToUserId.Value;
        }
        else if (!ticket.AssignedToUserId.HasValue)
        {
            ticket.AssignedToUserId = userId;
        }

        if (!ticket.AssignedToUserId.HasValue)
            throw new InvalidOperationException("Não é permitido mover um chamado para atendimento sem um responsável atribuído.");

        var attendant = await _context.Users.FindAsync(ticket.AssignedToUserId.Value);
        var fromStatus = ticket.Status;
        ticket.Status = TicketStatus.InProgress;
        ticket.UpdatedAt = DateTimeOffset.UtcNow;
        AddHistory(ticket, userId, "Atendimento iniciado", $"{attendant?.Name ?? "Técnico"} assumiu o atendimento do chamado.", fromStatus, TicketStatus.InProgress);
        await _context.SaveChangesAsync();

        if (userId != ticket.CreatedByUserId)
        {
            await _notificationService.CreateNotificationAsync(
                ticket.CreatedByUserId,
                ticket.Id,
                $"Atendimento iniciado no chamado #{ticket.ProtocolNumber}",
                $"{attendant?.Name ?? "Técnico"} iniciou o atendimento do seu chamado.",
                NotificationType.StatusChanged);
        }
    }

    public async Task WaitForUserAsync(Guid ticketId, Guid userId, Role userRole)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null) throw new KeyNotFoundException("Chamado não encontrado.");
        if (userRole == Role.User) throw new UnauthorizedAccessException("Usuários não podem alterar para aguardando usuário.");
        if (ticket.Status != TicketStatus.InProgress) 
            throw new InvalidOperationException($"Transição inválida: apenas chamados Em Atendimento podem ir para Aguardando Usuário (status atual: {ticket.Status}).");

        var fromStatus = ticket.Status;
        ticket.Status = TicketStatus.WaitingUser;
        ticket.UpdatedAt = DateTimeOffset.UtcNow;
        AddHistory(ticket, userId, "Aguardando Usuário", "O suporte solicitou informações ao usuário.", fromStatus, TicketStatus.WaitingUser);
        await _context.SaveChangesAsync();

        if (userId != ticket.CreatedByUserId)
        {
            await _notificationService.CreateNotificationAsync(
                ticket.CreatedByUserId,
                ticket.Id,
                $"Informações solicitadas no chamado #{ticket.ProtocolNumber}",
                "O suporte técnico está aguardando suas informações para dar continuidade.",
                NotificationType.InformationRequested);
        }
    }

    public async Task ResolveTicketAsync(Guid ticketId, ResolveTicketRequest request, Guid userId, Role userRole)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null) throw new KeyNotFoundException("Chamado não encontrado.");
        if (userRole == Role.User) throw new UnauthorizedAccessException("Usuários não podem resolver chamados.");
        
        if (ticket.Status != TicketStatus.InProgress) 
            throw new InvalidOperationException($"Transição inválida: apenas chamados Em Atendimento podem ser resolvidos (status atual: {ticket.Status}).");

        if (!ticket.AssignedToUserId.HasValue)
            throw new InvalidOperationException("O chamado precisa ter um responsável atribuído para ser finalizado.");

        if (string.IsNullOrWhiteSpace(request?.Solution))
            throw new InvalidOperationException("É obrigatório registrar a solução aplicada para resolver o chamado.");

        var fromStatus = ticket.Status;
        ticket.Status = TicketStatus.Resolved;
        ticket.Solution = request.Solution.Trim();
        ticket.ResolvedAt = DateTimeOffset.UtcNow;
        ticket.UpdatedAt = DateTimeOffset.UtcNow;
        AddHistory(ticket, userId, "Chamado resolvido", $"Solução registrada: {ticket.Solution}", fromStatus, TicketStatus.Resolved);
        await _context.SaveChangesAsync();

        if (userId != ticket.CreatedByUserId)
        {
            await _notificationService.CreateNotificationAsync(
                ticket.CreatedByUserId,
                ticket.Id,
                $"Chamado #{ticket.ProtocolNumber} resolvido",
                $"Solução: {ticket.Solution}",
                NotificationType.TicketResolved);
        }
    }

    public async Task ReopenTicketAsync(Guid ticketId, ReopenTicketRequest request, Guid userId, Role userRole)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null) throw new KeyNotFoundException("Chamado não encontrado.");
        
        if (ticket.Status != TicketStatus.Resolved) 
            throw new InvalidOperationException($"Transição inválida: apenas chamados Resolvidos podem ser reabertos (status atual: {ticket.Status}).");

        if (string.IsNullOrWhiteSpace(request?.Justification))
            throw new InvalidOperationException("É obrigatório informar uma justificativa para reabrir um chamado resolvido.");

        var targetStatus = request.TargetStatus == TicketStatus.InProgress ? TicketStatus.InProgress : TicketStatus.Open;

        if (targetStatus == TicketStatus.InProgress && !ticket.AssignedToUserId.HasValue)
        {
            ticket.AssignedToUserId = userId;
        }

        var user = await _context.Users.FindAsync(userId);
        var fromStatus = ticket.Status;
        ticket.Status = targetStatus;
        ticket.ResolvedAt = null;
        ticket.UpdatedAt = DateTimeOffset.UtcNow;
        AddHistory(ticket, userId, "Chamado reaberto", $"Reaberto por {user?.Name ?? "Usuário"}. Justificativa: {request.Justification.Trim()}", fromStatus, targetStatus);
        await _context.SaveChangesAsync();

        if (ticket.AssignedToUserId.HasValue && userId != ticket.AssignedToUserId.Value)
        {
            await _notificationService.CreateNotificationAsync(
                ticket.AssignedToUserId.Value,
                ticket.Id,
                $"Chamado #{ticket.ProtocolNumber} reaberto",
                $"{user?.Name ?? "Usuário"} reabriu o chamado com justificativa.",
                NotificationType.TicketReopened);
        }

        if (userId != ticket.CreatedByUserId)
        {
            await _notificationService.CreateNotificationAsync(
                ticket.CreatedByUserId,
                ticket.Id,
                $"Chamado #{ticket.ProtocolNumber} reaberto",
                $"{user?.Name ?? "O suporte"} reabriu seu chamado.",
                NotificationType.TicketReopened);
        }
    }

    public async Task DeleteTicketAsync(Guid ticketId, Guid userId, Role userRole)
    {
        var ticket = await _context.Tickets
            .Include(t => t.History)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == ticketId);
            
        if (ticket == null) throw new KeyNotFoundException("Chamado não encontrado.");

        if (userRole == Role.Attendant && ticket.Status != TicketStatus.Resolved)
            throw new UnauthorizedAccessException("O suporte técnico só possui permissão para excluir chamados após serem solucionados/resolvidos.");
        
        if (userRole == Role.User && ticket.CreatedByUserId != userId)
            throw new UnauthorizedAccessException("Usuários só podem deletar os próprios chamados.");

        _context.TicketHistories.RemoveRange(ticket.History);
        _context.TicketComments.RemoveRange(ticket.Comments);
        _context.Tickets.Remove(ticket);
        await _context.SaveChangesAsync();
    }

    public async Task<TicketAttachmentDto?> GetAttachmentAsync(Guid ticketId)
    {
        var ticket = await _context.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
            return null;

        if (ticket.AttachmentData != null && ticket.AttachmentData.Length > 0)
        {
            return new TicketAttachmentDto
            {
                Data = ticket.AttachmentData,
                ContentType = ticket.AttachmentContentType ?? "image/jpeg",
                FileName = ticket.AttachmentFileName ?? "anexo.jpg"
            };
        }

        if (!string.IsNullOrEmpty(ticket.AttachmentUrl) && ticket.AttachmentUrl.StartsWith("/uploads/"))
        {
            var fileName = Path.GetFileName(ticket.AttachmentUrl);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);
            if (File.Exists(filePath))
            {
                var bytes = await File.ReadAllBytesAsync(filePath);
                return new TicketAttachmentDto
                {
                    Data = bytes,
                    ContentType = fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? "image/png" : "image/jpeg",
                    FileName = fileName
                };
            }
        }

        return null;
    }
}
