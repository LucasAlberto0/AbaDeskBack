using AbaDeskBack.Services.Interfaces;
using AbaDeskBack.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AbaDeskBack.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    private Guid GetCurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private Role GetCurrentUserRole() => Enum.Parse<Role>(User.FindFirstValue(ClaimTypes.Role)!);

    [HttpGet("summary")]
    [SwaggerOperation(Summary = "Resumo do Dashboard", Description = "Retorna totais e dados resumidos dos chamados para exibição no dashboard. Baseado no nível de acesso do usuário.")]
    public async Task<IActionResult> GetSummary()
    {
        var response = await _dashboardService.GetSummaryAsync(GetCurrentUserId(), GetCurrentUserRole());
        return Ok(response);
    }
}
