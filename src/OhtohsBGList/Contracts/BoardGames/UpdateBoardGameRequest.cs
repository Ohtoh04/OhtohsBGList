using System.ComponentModel.DataAnnotations;

namespace OhtohsBGList.Contracts.BoardGames;

public class UpdateBoardGameRequest
{
    [Required]
    public required string Name { get; set; }

    [Required]
    public required int Year { get; set; }
}
