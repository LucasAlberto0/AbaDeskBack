using AbaDeskBack.DTOs.Tests;
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
public class TestCasesController : ControllerBase
{
    private readonly ITestCaseService _testCaseService;

    public TestCasesController(ITestCaseService testCaseService)
    {
        _testCaseService = testCaseService;
    }

    private Guid GetCurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private Role GetCurrentUserRole() => Enum.Parse<Role>(User.FindFirstValue(ClaimTypes.Role)!);

    [HttpGet("/api/tickets/{ticketId}/tests")]
    public async Task<IActionResult> GetTestCases(Guid ticketId)
    {
        try
        {
            var response = await _testCaseService.GetTestCasesAsync(ticketId, GetCurrentUserId(), GetCurrentUserRole());
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

    [HttpPost("/api/tickets/{ticketId}/tests")]
    public async Task<IActionResult> CreateTestCase(Guid ticketId, [FromBody] CreateTestCaseRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var response = await _testCaseService.CreateTestCaseAsync(ticketId, request, GetCurrentUserId(), GetCurrentUserRole());
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

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTestCase(Guid id, [FromBody] UpdateTestCaseRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var response = await _testCaseService.UpdateTestCaseAsync(id, request, GetCurrentUserId(), GetCurrentUserRole());
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTestCase(Guid id)
    {
        try
        {
            await _testCaseService.DeleteTestCaseAsync(id, GetCurrentUserId(), GetCurrentUserRole());
            return NoContent();
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

    [HttpPost("{id}/execute")]
    public async Task<IActionResult> ExecuteTestCase(Guid id, [FromBody] ExecuteTestCaseRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _testCaseService.ExecuteTestCaseAsync(id, request, GetCurrentUserId(), GetCurrentUserRole());
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
