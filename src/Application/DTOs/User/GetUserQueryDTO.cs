using Application.DTOs.Pagination;
using Domain.Enums;

namespace Application.DTOs.User;

public record GetUserQueryDTO : PaginationQuery
{
    public UserRole? Role { get; init; }
    public UserStatus? Status { get; init; }
}
