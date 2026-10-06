using AbaDeskBack.Enums;
using System.ComponentModel.DataAnnotations;

namespace AbaDeskBack.DTOs.Homologations;

public class CreateHomologationRequest
{
    [Required]
    public HomologationStatus Status { get; set; }

    [Required(ErrorMessage = "A observação é obrigatória.")]
    public string Observations { get; set; } = string.Empty;
}
