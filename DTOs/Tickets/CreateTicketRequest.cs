using AbaDeskBack.Enums;
using System.ComponentModel.DataAnnotations;

namespace AbaDeskBack.DTOs.Tickets;

public class CreateTicketRequest
{
    [Required(ErrorMessage = "O título é obrigatório.")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    public string Description { get; set; } = string.Empty;

    public string CompanyUnit { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;

    [Required(ErrorMessage = "O nome do sistema é obrigatório.")]
    public string SystemName { get; set; } = string.Empty;

    [Required(ErrorMessage = "A categoria é obrigatória.")]
    public Category Category { get; set; }

    [Required(ErrorMessage = "A prioridade é obrigatória.")]
    public Priority Priority { get; set; }

    [Required(ErrorMessage = "O anexo da imagem é obrigatório.")]
    public string AttachmentBase64 { get; set; } = string.Empty;
}
