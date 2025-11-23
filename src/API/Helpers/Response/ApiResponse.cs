using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Shared;

public enum ResponseStatus
{
    Success,
    Failure
}

public record ApiResponse<T>
{
    [Required] public required T? Result { get; init; }
    [Required] public List<string> Errors { get; init; } = [];
    [Required] public List<string> Messages { get; init; } = [];
    [Required] public required ResponseStatus Status { get; init; }
    [Required] public required string? TrackingId { get; init; }
}
