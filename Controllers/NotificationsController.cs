using AbaDeskBack.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace AbaDeskBack.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Listar notificações", Description = "Retorna a lista de notificações recentes do usuário autenticado.")]
    public async Task<IActionResult> GetNotifications()
    {
        var response = await _notificationService.GetNotificationsAsync(GetCurrentUserId());
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Excluir notificação", Description = "Remove uma notificação específica.")]
    public async Task<IActionResult> DeleteNotification(Guid id)
    {
        await _notificationService.DeleteNotificationAsync(id, GetCurrentUserId());
        return NoContent();
    }

    [HttpDelete]
    [SwaggerOperation(Summary = "Limpar todas", Description = "Limpa o histórico de notificações do usuário.")]
    public async Task<IActionResult> ClearAll()
    {
        await _notificationService.ClearAllNotificationsAsync(GetCurrentUserId());
        return NoContent();
    }
}
