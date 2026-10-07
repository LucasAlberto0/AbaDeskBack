using AbaDeskBack.Data;
using AbaDeskBack.DTOs.Common;
using AbaDeskBack.DTOs.Users;
using AbaDeskBack.Entities;
using AbaDeskBack.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AbaDeskBack.Services.Implementations;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(AppDbContext context, IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<PagedResult<UserResponse>> GetUsersAsync(string? search = null, int page = 1, int pageSize = 20)
    {
        var query = _context.Users.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.Name.Contains(search) || u.Email.Contains(search));
        }

        var totalItems = await query.CountAsync();
        var items = await query
            .OrderBy(u => u.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive
            })
            .ToListAsync();

        return new PagedResult<UserResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<UserResponse> GetUserByIdAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) throw new System.Collections.Generic.KeyNotFoundException("Usuário não encontrado.");

        return new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive
        };
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Este e-mail já está em uso.");

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            Role = request.Role,
            IsActive = true
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive
        };
    }

    public async Task<UserResponse> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) throw new System.Collections.Generic.KeyNotFoundException("Usuário não encontrado.");

        if (user.Email != request.Email && await _context.Users.AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Este e-mail já está em uso.");

        user.Name = request.Name;
        user.Email = request.Email;
        user.Role = request.Role;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();

        return new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive
        };
    }

    public async Task ToggleUserStatusAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) throw new System.Collections.Generic.KeyNotFoundException("Usuário não encontrado.");

        if (user.Role == AbaDeskBack.Enums.Role.Admin) 
            throw new InvalidOperationException("Não é possível excluir ou inativar uma conta de Administrador.");

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();
    }
}
