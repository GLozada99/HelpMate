namespace Application.DTOs.Board;

public enum UpdateBoardStatus
{
    Active
}

public record UpdateBoardDTO(
    string Name,
    string Description,
    UpdateBoardStatus Status
);
