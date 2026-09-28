using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OhtohsBGList.Data.Models;

[Table("Domains")]
public class Domain : IAuditable
{
    [Key]
    [Required]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(200)]
    public string? Notes { get; set; }

    [Required]
    public int Flags { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; }

    [Required]
    public DateTime LastModifiedDate { get; set; }

    public ICollection<BoardGames_Domains>? BoardGames_Domains { get; set; }
}
