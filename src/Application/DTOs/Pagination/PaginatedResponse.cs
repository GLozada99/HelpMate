namespace Application.DTOs.Pagination;

public record PaginatedResponse<T>
{
    public List<T> Items { get; init; } = [];
    public int TotalItems { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}
