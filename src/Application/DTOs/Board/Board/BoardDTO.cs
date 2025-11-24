using Domain.Enums;

namespace Application.DTOs.Board.Board;

public record BoardDTO
{
    public int Id { get; init; }
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string Description { get; init; } = null!;
    public int CreatedById { get; init; }
    public BoardStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
}
