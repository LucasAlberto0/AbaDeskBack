using System.ComponentModel.DataAnnotations;

namespace AbaDeskBack.DTOs.Tickets;

public class ResolveTicketRequest
{
    [Required(ErrorMessage = "A descrição da solução é obrigatória.")]
    [MinLength(5, ErrorMessage = "A solução deve conter pelo menos 5 caracteres.")]
    public string Solution { get; set; } = string.Empty;
}
