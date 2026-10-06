using AbaDeskBack.Services.Interfaces;
using AbaDeskBack.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> GetSummary()
    {
        var response = await _dashboardService.GetSummaryAsync(GetCurrentUserId(), GetCurrentUserRole());
        return Ok(response);
    }
}
