namespace Application.DTOs.Board.Board;

public enum UpdateBoardStatus
{
    Active
}

public record UpdateBoardDTO(
    string Name,
    string Description,
    UpdateBoardStatus Status
);
