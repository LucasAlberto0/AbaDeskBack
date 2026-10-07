using AbaDeskBack.DTOs.Auth;
using AbaDeskBack.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AbaDeskBack.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Autenticar usuário", Description = "Autentica o usuário utilizando email e senha, retornando o token JWT.")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Cadastrar usuário", Description = "Registra um novo usuário no sistema e retorna um token de acesso.")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _authService.RegisterAsync(request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("me")]
    [Authorize]
    [SwaggerOperation(Summary = "Buscar dados do usuário logado", Description = "Retorna os detalhes do usuário atual com base no token JWT enviado.")]
    public IActionResult GetCurrentUser()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var role = User.FindFirstValue(ClaimTypes.Role);
        var name = User.FindFirstValue("name");

        if (string.IsNullOrEmpty(userIdString))
        {
            return Unauthorized();
        }

        return Ok(new
        {
            Id = Guid.Parse(userIdString),
            Name = name,
            Email = email,
            Role = role,
            IsActive = true
        });
    }

    [HttpPatch("profile")]
    [Authorize]
    [SwaggerOperation(Summary = "Atualizar perfil", Description = "Atualiza nome e empresa do usuário logado, gerando um novo token JWT.")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        try
        {
            var response = await _authService.UpdateProfileAsync(Guid.Parse(userIdString), request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}
