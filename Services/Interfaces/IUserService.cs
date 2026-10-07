using AbaDeskBack.DTOs.Common;
using AbaDeskBack.DTOs.Users;
using System;
using System.Threading.Tasks;

namespace AbaDeskBack.Services.Interfaces;

public interface IUserService
{
    Task<PagedResult<UserResponse>> GetUsersAsync(string? search = null, int page = 1, int pageSize = 20);
    Task<UserResponse> GetUserByIdAsync(Guid id);
    Task<UserResponse> CreateUserAsync(CreateUserRequest request);
    Task<UserResponse> UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task<string> DeleteUserAsync(Guid id);
}
