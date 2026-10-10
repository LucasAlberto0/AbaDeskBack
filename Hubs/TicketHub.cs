using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace AbaDeskBack.Hubs;

[Authorize]
public class TicketHub : Hub
{
    public async Task JoinTicketGroup(string ticketId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Ticket_{ticketId}");
    }

    public async Task LeaveTicketGroup(string ticketId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Ticket_{ticketId}");
    }

    public async Task Typing(string ticketId, string userName, bool isTyping)
    {
        
        await Clients.OthersInGroup($"Ticket_{ticketId}").SendAsync("UserTyping", userName, isTyping);
    }
}
