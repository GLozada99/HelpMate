using Domain.Enums;

namespace Application.DTOs.Board.Board;

public record UpdateBoardDTO(
    string Name,
    string Description,
    UpdateBoardStatus Status
);
