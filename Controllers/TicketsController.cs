using AbaDeskBack.DTOs.Comments;
using AbaDeskBack.DTOs.Tickets;
using AbaDeskBack.Enums;
using AbaDeskBack.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _ticketService.CreateTicketAsync(request, GetCurrentUserId());
        return CreatedAtAction(nameof(GetTicketDetails), new { id = response.Id }, response);
    }

    [HttpGet]
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

    [HttpPost("{id}/start-development")]
    public async Task<IActionResult> StartDevelopment(Guid id)
    {
        try
        {
            await _ticketService.StartDevelopmentAsync(id, GetCurrentUserId(), GetCurrentUserRole());
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

    [HttpPost("{id}/send-to-test")]
    public async Task<IActionResult> SendToTest(Guid id)
    {
        try
        {
            await _ticketService.SendToTestAsync(id, GetCurrentUserId(), GetCurrentUserRole());
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

    [HttpPost("{id}/send-to-homologation")]
    public async Task<IActionResult> SendToHomologation(Guid id)
    {
        try
        {
            await _ticketService.SendToHomologationAsync(id, GetCurrentUserId(), GetCurrentUserRole());
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
