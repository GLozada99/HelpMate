using Application.DTOs.Auth;
using FluentResults;

namespace Application.Interfaces.Auth;

public interface IAuthService
{
    Task<Result<LoggedInUserDTO>> ValidateUserLogin(LoginDTO dto);

    Task<Result<LoggedInUserDTO>> GetUserInfo(int id);
}
