using Application.DTOs.Auth;
using Application.Errors;
using Application.Interfaces.Auth;
using FluentResults;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Auth;

public class AuthService(
    HelpMateDbContext context,
    ILogger<AuthService> logger,
    IPasswordHasher passwordHasher) : IAuthService
{
    public async Task<Result<LoggedInUserDTO>> ValidateUserLogin(LoginDTO dto)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null)
        {
            logger.LogWarning("User with email '{Email}' not found.",
                dto.Email);
            return Result.Fail(new NoUserWithEmailError(dto.Email));
        }

        if (!passwordHasher.VerifyPassword(dto.Password, user.Password))
        {
            logger.LogWarning("Provided password does not match.");
            return Result.Fail(new InvalidCredentialsError());
        }

        if (!user.IsActive)
        {
            logger.LogWarning("User with email '{Email}' is not active.", dto.Email);
            return Result.Fail(new NotActiveUserError());
        }

        var resultDto = new LoggedInUserDTO(
            user.Id,
            user.Email,
            user.FullName,
            user.Role,
            user.Status,
            user.CreatedAt
        );

        return Result.Ok(resultDto);
    }

    public async Task<Result<LoggedInUserDTO>> GetUserInfo(int id)
    {
        var user = await context.Users.FindAsync(id);

        if (user == null)
            return Result.Fail<LoggedInUserDTO>(new UserNotFoundError(id));

        var resultDto = new LoggedInUserDTO(
            user.Id,
            user.Email,
            user.FullName,
            user.Role,
            user.Status,
            user.CreatedAt
        );

        return Result.Ok(resultDto);
    }
}
