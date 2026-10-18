using AbaDeskBack.Data;
using AbaDeskBack.DTOs.Notifications;
using AbaDeskBack.Entities;
using AbaDeskBack.Enums;
using AbaDeskBack.Hubs;
using AbaDeskBack.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AbaDeskBack.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<TicketHub> _hubContext;

    public NotificationService(AppDbContext context, IHubContext<TicketHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<List<NotificationResponse>> GetNotificationsAsync(Guid userId)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Include(n => n.Ticket)
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(40)
            .Select(n => new NotificationResponse
            {
                Id = n.Id,
                TicketId = n.TicketId,
                TicketProtocol = n.Ticket != null ? n.Ticket.ProtocolNumber : null,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();
    }

    public async Task CreateNotificationAsync(Guid userId, Guid? ticketId, string title, string message, NotificationType type)
    {
        var notification = new Notification
        {
            UserId = userId,
            TicketId = ticketId,
            Title = title,
            Message = message,
            Type = type,
            IsRead = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        var ticket = ticketId.HasValue ? await _context.Tickets.FindAsync(ticketId.Value) : null;

        var response = new NotificationResponse
        {
            Id = notification.Id,
            TicketId = notification.TicketId,
            TicketProtocol = ticket?.ProtocolNumber,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            CreatedAt = notification.CreatedAt
        };

        await _hubContext.Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", response);
    }

    public async Task DeleteNotificationAsync(Guid notificationId, Guid userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null) return;

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();
    }

    public async Task ClearAllNotificationsAsync(Guid userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .ToListAsync();

        _context.Notifications.RemoveRange(notifications);
        await _context.SaveChangesAsync();
    }
}
