using System.ComponentModel.DataAnnotations;

namespace AbaDeskBack.DTOs.Tests;

public class UpdateTestCaseRequest
{
    [Required(ErrorMessage = "O título é obrigatório.")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "O resultado esperado é obrigatório.")]
    public string ExpectedResult { get; set; } = string.Empty;
}
