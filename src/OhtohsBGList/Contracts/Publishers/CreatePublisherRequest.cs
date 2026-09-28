using System.ComponentModel.DataAnnotations;

namespace OhtohsBGList.Contracts.Publishers;

public class CreatePublisherRequest
{
    [Required]
    public required string Name { get; set; }
}
