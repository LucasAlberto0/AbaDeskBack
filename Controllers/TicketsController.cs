using AbaDeskBack.DTOs.Comments;
using AbaDeskBack.DTOs.Tickets;
using AbaDeskBack.Enums;
using AbaDeskBack.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AbaDeskBack.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    private Guid GetCurrentUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(id!);
    }

    private Role GetCurrentUserRole()
    {
        var roleString = User.FindFirstValue(ClaimTypes.Role);
        return Enum.Parse<Role>(roleString!);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Criar Chamado", Description = "Cria um novo chamado técnico ou requisição. Usuários podem criar para si, admins para qualquer um.")]
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _ticketService.CreateTicketAsync(request, GetCurrentUserId(), GetCurrentUserRole());
        return CreatedAtAction(nameof(GetTicketDetails), new { id = response.Id }, response);
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Excluir Chamado", Description = "Exclui permanentemente um chamado e seus anexos. Permissão: Admin, ou Criador do Chamado.")]
    public async Task<IActionResult> DeleteTicket(Guid id)
    {
        try
        {
            await _ticketService.DeleteTicketAsync(id, GetCurrentUserId(), GetCurrentUserRole());
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Listar Chamados", Description = "Lista todos os chamados com filtros avançados. Admins/Attendants veem tudo, Users veem os próprios.")]
    public async Task<IActionResult> GetTickets(
        [FromQuery] TicketStatus? status,
        [FromQuery] Priority? priority,
        [FromQuery] Category? category,
        [FromQuery] string? systemName,
        [FromQuery] string? department,
        [FromQuery] Guid? assignedToUserId,
        [FromQuery] Guid? createdByUserId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var response = await _ticketService.GetTicketsAsync(
            GetCurrentUserId(),
            GetCurrentUserRole(),
            status, priority, category, systemName, department, assignedToUserId, createdByUserId, search, page, pageSize);

        return Ok(response);
    }

    [HttpGet("my")]
    [SwaggerOperation(Summary = "Meus Chamados", Description = "Lista chamados do usuário logado (criados por ele ou atribuídos a ele).")]
    public async Task<IActionResult> GetMyTickets(
        [FromQuery] TicketStatus? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var response = await _ticketService.GetMyTicketsAsync(
            GetCurrentUserId(), status, search, page, pageSize);

        return Ok(response);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Detalhes do Chamado", Description = "Busca detalhes completos de um chamado.")]
    public async Task<IActionResult> GetTicketDetails(Guid id)
    {
        try
        {
            var response = await _ticketService.GetTicketDetailsAsync(id, GetCurrentUserId(), GetCurrentUserRole());
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpPost("{id}/comments")]
    [SwaggerOperation(Summary = "Adicionar Comentário", Description = "Adiciona um novo comentário ao histórico do chamado.")]
    public async Task<IActionResult> AddComment(Guid id, [FromBody] CreateCommentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var response = await _ticketService.AddCommentAsync(id, request, GetCurrentUserId());
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpGet("{id}/comments")]
    [SwaggerOperation(Summary = "Listar Comentários", Description = "Lista todos os comentários de um chamado específico.")]
    public async Task<IActionResult> GetComments(Guid id)
    {
        try
        {
            var response = await _ticketService.GetCommentsAsync(id, GetCurrentUserId(), GetCurrentUserRole());
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpGet("{id}/history")]
    [SwaggerOperation(Summary = "Listar Histórico", Description = "Lista o histórico de alterações (auditoria) do chamado.")]
    public async Task<IActionResult> GetHistory(Guid id)
    {
        try
        {
            var response = await _ticketService.GetHistoryAsync(id, GetCurrentUserId(), GetCurrentUserRole());
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpPost("{id}/analyze")]
    [SwaggerOperation(Summary = "Iniciar Análise", Description = "Move o status do chamado para 'Em Análise'. Permissão: Admin, Suporte Técnico.")]
    public async Task<IActionResult> StartAnalysis(Guid id)
    {
        try
        {
            await _ticketService.StartAnalysisAsync(id, GetCurrentUserId(), GetCurrentUserRole());
            return Ok();
        }
        catch (Exception ex) when (ex is KeyNotFoundException || ex is InvalidOperationException)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpPost("{id}/assign")]
    [SwaggerOperation(Summary = "Atribuir Chamado", Description = "Atribui um chamado a um usuário específico (ex: Suporte Técnico).")]
    public async Task<IActionResult> AssignTicket(Guid id, [FromBody] AssignTicketRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            await _ticketService.AssignTicketAsync(id, request.UserId, GetCurrentUserId(), GetCurrentUserRole());
            return Ok();
        }
        catch (Exception ex) when (ex is KeyNotFoundException || ex is InvalidOperationException)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpPost("{id}/start-progress")]
    [SwaggerOperation(Summary = "Iniciar Atendimento", Description = "Move o status para 'Em Progresso'. Permissão: Admin, Suporte Técnico.")]
    public async Task<IActionResult> StartProgress(Guid id)
    {
        try
        {
            await _ticketService.StartProgressAsync(id, GetCurrentUserId(), GetCurrentUserRole());
            return Ok();
        }
        catch (Exception ex) when (ex is KeyNotFoundException || ex is InvalidOperationException)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpPost("{id}/wait-for-user")]
    [SwaggerOperation(Summary = "Aguardar Usuário", Description = "Move o status para 'Aguardando Usuário' quando necessita de informações externas.")]
    public async Task<IActionResult> WaitForUser(Guid id)
    {
        try
        {
            await _ticketService.WaitForUserAsync(id, GetCurrentUserId(), GetCurrentUserRole());
            return Ok();
        }
        catch (Exception ex) when (ex is KeyNotFoundException || ex is InvalidOperationException)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpPost("{id}/resolve")]
    [SwaggerOperation(Summary = "Resolver Chamado", Description = "Move o status para 'Resolvido'. Permissão: Admin, Suporte Técnico.")]
    public async Task<IActionResult> ResolveTicket(Guid id)
    {
        try
        {
            await _ticketService.ResolveTicketAsync(id, GetCurrentUserId(), GetCurrentUserRole());
            return Ok();
        }
        catch (Exception ex) when (ex is KeyNotFoundException || ex is InvalidOperationException)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }
}
