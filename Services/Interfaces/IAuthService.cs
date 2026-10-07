using AbaDeskBack.DTOs.Auth;
using System.Threading.Tasks;

namespace AbaDeskBack.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> UpdateProfileAsync(System.Guid userId, UpdateProfileRequest request);
}
