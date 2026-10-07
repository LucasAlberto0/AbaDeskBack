using AbaDeskBack.DTOs.Users;
using AbaDeskBack.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AbaDeskBack.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Listar Usuários", Description = "Lista todos os usuários cadastrados com paginação e busca por nome/email.")]
    public async Task<IActionResult> GetUsers([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var response = await _userService.GetUsersAsync(search, page, pageSize);
        return Ok(response);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Buscar Usuário", Description = "Busca os detalhes de um usuário específico pelo seu ID.")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        try
        {
            var response = await _userService.GetUserByIdAsync(id);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Criar Usuário", Description = "Cria manualmente um novo usuário. Apenas administradores podem usar esta rota.")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var response = await _userService.CreateUserAsync(request);
            return CreatedAtAction(nameof(GetUserById), new { id = response.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Atualizar Usuário", Description = "Atualiza os dados de um usuário existente (nome, e-mail, cargo, empresa e papel).")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var response = await _userService.UpdateUserAsync(id, request);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Inativar Usuário", Description = "Altera o status do usuário para inativo, impedindo que ele faça login.")]
    public async Task<IActionResult> ToggleUserStatus(Guid id)
    {
        try
        {
            var msg = await _userService.DeleteUserAsync(id);
            return Ok(new { message = msg });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
