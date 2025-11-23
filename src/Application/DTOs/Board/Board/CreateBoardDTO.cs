using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Board;

public record CreateBoardDTO(
    [Required] [StringLength(4)] string Code,
    [Required] string Name,
    [Required] string Description
);
