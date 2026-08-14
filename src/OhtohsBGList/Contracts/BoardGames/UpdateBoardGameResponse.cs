namespace OhtohsBGList.Contracts.BoardGames;

public class UpdateBoardGameResponse
{
    public required int Id { get; set; }

    public required string Name { get; set; }

    public required int Year { get; set; }
}
