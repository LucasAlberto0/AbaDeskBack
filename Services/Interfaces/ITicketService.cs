using AbaDeskBack.DTOs.Common;
using AbaDeskBack.DTOs.Tickets;
using AbaDeskBack.DTOs.Comments;
using AbaDeskBack.DTOs.History;
using AbaDeskBack.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AbaDeskBack.Services.Interfaces;

public interface ITicketService
{
    Task<TicketResponse> CreateTicketAsync(CreateTicketRequest request, Guid userId, Role userRole);
    
    Task<PagedResult<TicketListItemResponse>> GetTicketsAsync(
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
        int pageSize = 20);

    Task<PagedResult<TicketListItemResponse>> GetMyTicketsAsync(
        Guid userId,
        TicketStatus? status = null,
        string? search = null,
        int page = 1,
        int pageSize = 20);

    Task<TicketDetailsResponse> GetTicketDetailsAsync(Guid ticketId, Guid userId, Role userRole);

    Task<CommentResponse> AddCommentAsync(Guid ticketId, CreateCommentRequest request, Guid userId);
    
    Task<List<CommentResponse>> GetCommentsAsync(Guid ticketId, Guid userId, Role userRole);
    
    Task<List<HistoryResponse>> GetHistoryAsync(Guid ticketId, Guid userId, Role userRole);

    Task StartAnalysisAsync(Guid ticketId, Guid userId, Role userRole);
    Task AssignTicketAsync(Guid ticketId, Guid assignedToUserId, Guid userId, Role userRole);
    Task StartProgressAsync(Guid ticketId, Guid userId, Role userRole);
    Task WaitForUserAsync(Guid ticketId, Guid userId, Role userRole);
    Task ResolveTicketAsync(Guid ticketId, Guid userId, Role userRole);
    Task DeleteTicketAsync(Guid ticketId, Guid userId, Role userRole);
}
