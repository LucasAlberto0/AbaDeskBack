using AbaDeskBack.Enums;
using System.ComponentModel.DataAnnotations;

namespace AbaDeskBack.DTOs.Tickets;

public class ReopenTicketRequest
{
    [Required(ErrorMessage = "A justificativa para reabertura é obrigatória.")]
    [MinLength(5, ErrorMessage = "A justificativa deve conter pelo menos 5 caracteres.")]
    public string Justification { get; set; } = string.Empty;

    public TicketStatus TargetStatus { get; set; } = TicketStatus.Open;
}
