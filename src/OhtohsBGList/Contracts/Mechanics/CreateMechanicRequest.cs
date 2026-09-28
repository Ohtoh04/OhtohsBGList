using System.ComponentModel.DataAnnotations;

namespace OhtohsBGList.Contracts.Mechanics;

public class CreateMechanicRequest
{
    [Required]
    public required string Name { get; set; }

    [MaxLength(200)]
    public string? Notes { get; set; }

    [Required]
    public required int Flags { get; set; }
}
