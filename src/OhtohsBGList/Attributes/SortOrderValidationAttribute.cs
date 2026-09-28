using System.ComponentModel.DataAnnotations;

namespace OhtohsBGList.Attributes;

public class SortOrderValidationAttribute : ValidationAttribute
{
    public string[] AllowedValues { get; } = ["ASC", "DESC"];

    public SortOrderValidationAttribute()
        : base("Value must be one of the following: {0}.") { }

    protected override ValidationResult? IsValid(
        object? value,
        ValidationContext validationContext)
    {
        var strValue = value as string;
        if (!string.IsNullOrEmpty(strValue) &&
            AllowedValues.Contains(strValue))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(
                FormatErrorMessage(string.Join(",", AllowedValues))
            );
    }
}
