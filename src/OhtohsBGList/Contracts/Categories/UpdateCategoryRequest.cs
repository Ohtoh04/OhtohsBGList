using System.ComponentModel.DataAnnotations;

namespace OhtohsBGList.Contracts.Categories;

public class UpdateCategoryRequest
{
    [Required]
    public required string Name { get; set; }
}
