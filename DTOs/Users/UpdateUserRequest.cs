using AbaDeskBack.Enums;
using System.ComponentModel.DataAnnotations;

namespace AbaDeskBack.DTOs.Users;

public class UpdateUserRequest
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required]
    public Role Role { get; set; }

    public bool IsActive { get; set; }
}
