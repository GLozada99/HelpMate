using Application.DTOs.User;
using Application.Errors;
using Application.Interfaces.Auth;
using Application.Interfaces.User;
using Domain.Enums;
using FluentResults;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.User;

public class UserService(
    HelpMateDbContext context,
    ILogger<UserService> logger,
    IPasswordHasher passwordHasher)
    : IUserService
{
    public async Task<Result<UserDTO>> CreateUser(CreateUserDTO dto)
    {
        if (await GetUserByEmail(dto.Email) != null)
        {
            logger.LogWarning("User with email '{Email}' already exists.",
                dto.Email);
            return Result.Fail(new UserEmailAlreadyInUseError(dto.Email));
        }

        var user = new Domain.Entities.User.User
        {
            Email = dto.Email,
            Password = passwordHasher.HashPassword(dto.Password),
            FullName = dto.FullName,
            Status = UserStatus.Active,
            Role = MapRole(dto.Role)
        };
        context.Users.Add(user);

        var saveResult = await context.SaveChangesResultAsync(
            logger,
            () => new BaseError()
        );
        if (saveResult.IsFailed)
            return saveResult;

        var resultDto = new UserDTO(
            user.Id,
            user.Email,
            user.FullName,
            user.Role,
            user.Status,
            user.CreatedAt
        );

        return Result.Ok(resultDto);
    }

    public async Task<Result<UserDTO>> GetUser(int id)
    {
        var user = await context.Users.FindAsync(id);

        if (user == null)
            return Result.Fail<UserDTO>(new UserNotFoundError(id));

        var resultDto = new UserDTO(
            user.Id,
            user.Email,
            user.FullName,
            user.Role,
            user.Status,
            user.CreatedAt
        );

        return Result.Ok(resultDto);
    }

    public async Task<Result<UserDTO>> UpdateUser(int id, UpdateUserDTO dto)
    {
        var user = await context.Users.FindAsync(id);

        if (user == null)
            return Result.Fail<UserDTO>(new UserNotFoundError(id));

        if (dto.Email is not null)
            if (await GetUserByEmail(dto.Email) != null)
            {
                logger.LogWarning("Registration failed — Email already exists: {Email}",
                    dto.Email);
                return Result.Fail(new UserEmailAlreadyInUseError(dto.Email));
            }
            else if (user.Email != dto.Email)
            {
                user.Email = dto.Email;
            }

        if (dto.FullName is not null)
            user.FullName = dto.FullName;

        if (dto.Role is not null)
            user.Role = MapRole((UpdateUserRole)dto.Role);

        if (dto.Status == UpdateUserStatus.Active)
            user.Status = UserStatus.Active;

        var saveResult = await context.SaveChangesResultAsync(
            logger,
            () => new BaseError()
        );
        if (saveResult.IsFailed)
            return saveResult;

        var resultDto = new UserDTO(
            user.Id,
            user.Email,
            user.FullName,
            user.Role,
            user.Status,
            user.CreatedAt
        );

        return Result.Ok(resultDto);
    }

    public async Task<Result> DeactivateUser(int id)
    {
        var user = await context.Users.FindAsync(id);
        if (user == null)
            return Result.Fail(new UserNotFoundError(id));

        user.Status = UserStatus.Inactive;
        var saveResult = await context.SaveChangesResultAsync(
            logger,
            () => new BaseError()
        );
        return saveResult.IsFailed ? saveResult : Result.Ok();
    }

    public Task<Result<IQueryable<UserDTO>>> GetUsers(GetUserQueryDTO dto)
    {
        var dbQuery = context.Users.AsQueryable();

        if (dto.Role.HasValue)
            dbQuery = dbQuery.Where(u => dto.Role.Value == u.Role);

        if (dto.Status.HasValue)
            dbQuery = dbQuery.Where(u => dto.Status.Value == u.Status);

        var resultDTOs = dbQuery.Select(u => new UserDTO(
            u.Id,
            u.Email,
            u.FullName,
            u.Role,
            u.Status,
            u.CreatedAt
        ));

        return Task.FromResult(Result.Ok(resultDTOs));
    }

    private UserRole MapRole(CreateUserRole role)
    {
        return role switch
        {
            CreateUserRole.Customer => UserRole.Customer,
            CreateUserRole.Agent => UserRole.Agent,
            CreateUserRole.Admin => UserRole.Admin,
            _ => throw new ArgumentOutOfRangeException(nameof(role))
        };
    }

    private UserRole MapRole(UpdateUserRole role)
    {
        return role switch
        {
            UpdateUserRole.Customer => UserRole.Customer,
            UpdateUserRole.Agent => UserRole.Agent,
            UpdateUserRole.Admin => UserRole.Admin,
            _ => throw new ArgumentOutOfRangeException(nameof(role))
        };
    }

    private async Task<Domain.Entities.User.User?> GetUserByEmail(string email)
    {
        return await context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
    }
}
