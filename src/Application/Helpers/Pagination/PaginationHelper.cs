using Application.DTOs.Pagination;

namespace Application.Helpers.Pagination;

public static class PaginationHelper
{
    public static PaginatedResponse<TDto> GetPaginatedResult<TDto>(
        IQueryable<TDto> query,
        PaginationQuery pagination)
    {
        var totalItems = query.Count();

        var items = query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize);

        return new PaginatedResponse<TDto>
        {
            Items = items.ToList(),
            TotalItems = totalItems,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }
}
