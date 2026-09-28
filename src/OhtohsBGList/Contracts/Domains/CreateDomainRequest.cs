using System.ComponentModel.DataAnnotations;

namespace OhtohsBGList.Contracts.Domains;

public class CreateDomainRequest
{
    [Required]
    public required string Name { get; set; }

    [MaxLength(200)]
    public string? Notes { get; set; }

    [Required]
    public required int Flags { get; set; }
}
