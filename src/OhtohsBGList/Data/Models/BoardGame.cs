using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OhtohsBGList.Data.Models;

[Table("BoardGames")]
public class BoardGame : IAuditable
{
    [Key]
    [Required]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(200)]
    public string? AlternateNames { get; set; }

    [MaxLength(200)]
    public string? Designer { get; set; }

    [Required]
    public int Flags { get; set; }

    [Required]
    public int Year { get; set; }

    [Required]
    public int MinPlayers { get; set; }

    [Required]
    public int MaxPlayers { get; set; }

    [Required]
    public int PlayTime { get; set; }

    [Required]
    public int MinAge { get; set; }

    [Required]
    public int UsersRated { get; set; }

    [Required]
    [Precision(4, 2)]
    public decimal RatingAverage { get; set; }

    [Required]
    public int BGGRank { get; set; }

    [Required]
    [Precision(4, 2)]
    public decimal ComplexityAverage { get; set; }

    [Required]
    public int OwnedUsers { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; }

    [Required]
    public DateTime LastModifiedDate { get; set; }

    public int? PublisherId { get; set; }
    public Publisher? Publisher { get; set; }

    public ICollection<BoardGames_Domains>? BoardGames_Domains { get; set; }

    public ICollection<BoardGames_Mechanics>? BoardGames_Mechanics { get; set; }

    public ICollection<BoardGames_Categories>? BoardGames_Categories { get; set; }
}
