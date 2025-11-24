using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.Board.Board;

public record BoardDTO
{
    [Required] public int Id { get; init; }

    [Required] public string Code { get; init; } = null!;

    [Required] public string Name { get; init; } = null!;

    [Required] public string Description { get; init; } = null!;

    [Required] public int CreatedById { get; init; }

    [Required] public BoardStatus Status { get; init; }

    [Required] public DateTime CreatedAt { get; init; }
}
