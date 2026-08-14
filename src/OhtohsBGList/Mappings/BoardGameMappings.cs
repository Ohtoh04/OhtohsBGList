using OhtohsBGList.Contracts.BoardGames;
using OhtohsBGList.Data.Models;

namespace OhtohsBGList.Mappings;

public static class BoardGameMappings
{
    public static BoardGame ToBoardGame(this CreateBoardGameRequest request)
    {
        return new BoardGame
        {
            Name = request.Name,
            Year = request.Year
        };
    }

    public static CreateBoardGameResponse ToCreateBoardGameResponse(this BoardGame boardGame)
    {
        return new CreateBoardGameResponse
        {
            Id = boardGame.Id,
            Name = boardGame.Name,
            Year = boardGame.Year
        };
    }

    public static void ApplyUpdate(this BoardGame boardGame, UpdateBoardGameRequest request)
    {
        boardGame.Name = request.Name;
        boardGame.Year = request.Year;
    }

    public static UpdateBoardGameResponse ToUpdateBoardGameResponse(this BoardGame boardGame)
    {
        return new UpdateBoardGameResponse
        {
            Id = boardGame.Id,
            Name = boardGame.Name,
            Year = boardGame.Year
        };
    }
}
