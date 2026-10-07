using AbaDeskBack.Enums;
using System;

namespace AbaDeskBack.DTOs.Users;

public class UserResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Role Role { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string CompanyUnit { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
