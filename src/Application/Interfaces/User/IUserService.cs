using Application.DTOs.User;
using FluentResults;

namespace Application.Interfaces.User;

public interface IUserService
{
    Task<Result<UserDTO>> CreateUser(CreateUserDTO dto, int requesterId);
    Task<Result<IQueryable<UserDTO>>> GetUsers(GetUserQueryDTO dto);
    Task<Result<UserDTO>> GetUser(int id);
    Task<Result<UserDTO>> UpdateUser(int id, UpdateUserDTO dto, int requesterId);
    Task<Result> DeactivateUser(int id, int requesterId);
}
