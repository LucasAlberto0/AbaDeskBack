using System.ComponentModel.DataAnnotations;

namespace AbaDeskBack.DTOs.Comments;

public class CreateCommentRequest
{
    [Required(ErrorMessage = "O conteúdo do comentário é obrigatório.")]
    [MinLength(3, ErrorMessage = "O comentário deve ter no mínimo 3 caracteres.")]
    public string Content { get; set; } = string.Empty;
}
