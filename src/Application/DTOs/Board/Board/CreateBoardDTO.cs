using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Board.Board;

public record CreateBoardDTO
{
    [Required]
    [StringLength(4, MinimumLength = 4)]
    public required string Code { get; init; }

    [Required] public required string Name { get; init; }

    [Required] public required string Description { get; init; }
}
