using OhtohsBGList.Contracts.Categories;
using OhtohsBGList.Data.Models;

namespace OhtohsBGList.Mappings;

public static class CategoryMappings
{
    public static Category ToCategory(this CreateCategoryRequest request)
    {
        return new Category
        {
            Name = request.Name
        };
    }

    public static CreateCategoryResponse ToCreateCategoryResponse(this Category category)
    {
        return new CreateCategoryResponse
        {
            Id = category.Id,
            Name = category.Name
        };
    }

    public static void ApplyUpdate(this Category category, UpdateCategoryRequest request)
    {
        category.Name = request.Name;
    }

    public static UpdateCategoryResponse ToUpdateCategoryResponse(this Category category)
    {
        return new UpdateCategoryResponse
        {
            Id = category.Id,
            Name = category.Name
        };
    }
}
