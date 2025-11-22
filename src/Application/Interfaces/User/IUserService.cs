using Application.DTOs.User;
using FluentResults;

namespace Application.Interfaces.User;

public interface IUserService
{
    Task<Result<UserDTO>> CreateUser(CreateUserDTO dto);
    Task<Result<IQueryable<UserDTO>>> GetUsers(GetUserQueryDTO dto);
    Task<Result<UserDTO>> GetUser(int id);
    Task<Result<UserDTO>> UpdateUser(int id, UpdateUserDTO dto);
    Task<Result> DeactivateUser(int id);
}
