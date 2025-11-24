using Domain.Enums;

namespace Application.DTOs.Board.Board;

public record UpdateBoardDTO
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public UpdateBoardStatus? Status { get; init; }
}
