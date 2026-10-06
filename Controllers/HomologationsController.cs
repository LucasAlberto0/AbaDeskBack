using AbaDeskBack.DTOs.Homologations;
using AbaDeskBack.Enums;
using AbaDeskBack.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AbaDeskBack.Controllers;

[ApiController]
[Route("api/tickets/{ticketId}/[controller]")]
[Authorize]
public class HomologationsController : ControllerBase
{
    private readonly IHomologationService _homologationService;

    public HomologationsController(IHomologationService homologationService)
    {
        _homologationService = homologationService;
    }

    private Guid GetCurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private Role GetCurrentUserRole() => Enum.Parse<Role>(User.FindFirstValue(ClaimTypes.Role)!);

    [HttpGet]
    public async Task<IActionResult> GetHomologations(Guid ticketId)
    {
        try
        {
            var response = await _homologationService.GetHomologationsAsync(ticketId, GetCurrentUserId(), GetCurrentUserRole());
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

    [HttpPost]
    public async Task<IActionResult> CreateHomologation(Guid ticketId, [FromBody] CreateHomologationRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var response = await _homologationService.CreateHomologationAsync(ticketId, request, GetCurrentUserId(), GetCurrentUserRole());
            return Ok(response);
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
