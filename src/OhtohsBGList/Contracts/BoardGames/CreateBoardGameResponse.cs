namespace OhtohsBGList.Contracts.BoardGames;

public class CreateBoardGameResponse
{
    public required int Id { get; set; }

    public required string Name { get; set; }

    public required int Year { get; set; }
}
