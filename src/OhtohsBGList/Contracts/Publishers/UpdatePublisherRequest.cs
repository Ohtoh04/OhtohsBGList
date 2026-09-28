using System.ComponentModel.DataAnnotations;

namespace OhtohsBGList.Contracts.Publishers;

public class UpdatePublisherRequest
{
    [Required]
    public required string Name { get; set; }
}
