using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.Board.Board;

public class BoardDTO(
    [Required] [StringLength(4)] string Code,
    [Required] string Name,
    [Required] string Description,
    [Required] int CreatedById,
    [Required] BoardStatus Status,
    [Required] DateTime CreatedAt
);
