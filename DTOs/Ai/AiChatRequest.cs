using System.ComponentModel.DataAnnotations;

namespace AbaDeskBack.DTOs.Ai;

public class AiChatRequest
{
    [Required(ErrorMessage = "A mensagem é obrigatória.")]
    [StringLength(4000, MinimumLength = 1, ErrorMessage = "A mensagem deve ter entre 1 e 4000 caracteres.")]
    public string Message { get; set; } = string.Empty;

    public string? SessionId { get; set; }
}
