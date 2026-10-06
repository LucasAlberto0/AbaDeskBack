using System;

namespace AbaDeskBack.DTOs.Auth;

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public Users.UserResponse User { get; set; } = null!;
}
