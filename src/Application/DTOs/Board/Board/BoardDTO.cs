using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.Board.Board;

public record BoardDTO(
    int Id,
    [StringLength(4)] string Code,
    string Name,
    string Description,
    int CreatedById,
    BoardStatus Status,
    DateTime CreatedAt
);
