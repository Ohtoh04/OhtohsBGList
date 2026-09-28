using System.ComponentModel.DataAnnotations;

namespace OhtohsBGList.Contracts.Categories;

public class CreateCategoryRequest
{
    [Required]
    public required string Name { get; set; }
}
