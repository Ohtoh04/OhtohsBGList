using System.ComponentModel.DataAnnotations;
using OhtohsBGList.Data.Models;

namespace OhtohsBGList.Contracts.BoardGames;

public class CreateBoardGameRequest
{
    [Required]
    public required string Name { get; set; }

    [Required]
    public required int Year { get; set; }
}
