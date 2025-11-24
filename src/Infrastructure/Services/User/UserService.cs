using Application.DTOs.User;
using Application.Errors;
using Application.Helpers.User;
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
    public async Task<Result<UserDTO>> GetUser(int id)
    {
        var user = await context.Users.FindAsync(id);

        if (user == null)
            return Result.Fail<UserDTO>(new UserNotFoundError(id));

        var resultDto = new UserDTO
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            Status = user.Status,
            CreatedAt = user.CreatedAt
        };

        return Result.Ok(resultDto);
    }

    public async Task<Result<UserDTO>> UpdateUser(int id, UpdateUserDTO dto,
        int requesterId)
    {
        var user = await context.Users.FindAsync(id);

        if (user == null)
            return Result.Fail<UserDTO>(new UserNotFoundError(id));

        var requester = await context.Users.FindAsync(requesterId);
        if (requester == null)
            return Result.Fail<UserDTO>(new UserNotFoundError(requesterId));

        var canUpdate =
            UserRulesHelper.CanUpdateUser(requester.Role, user.Role);
        if (canUpdate.IsFailed) return Result.Fail<UserDTO>(canUpdate.Errors);

        if (dto.Email is not null)
            if (await GetUserByEmail(dto.Email) != null)
            {
                logger.LogWarning(
                    "Can not update User email to '{Email}' because a user with that email already exists.",
                    dto.Email);
                return Result.Fail<UserDTO>(new UserEmailAlreadyInUseError(dto.Email));
            }
            else if (user.Email != dto.Email)
            {
                user.Email = dto.Email;
            }

        if (dto.FullName is not null)
            user.FullName = dto.FullName;

        if (dto.Role is not null)
        {
            var newRole = UserRulesHelper.MapRole(dto.Role.Value);

            var canUpdateUserRole =
                UserRulesHelper.CanUpdateUserRole(requester.Role, user.Role, newRole);
            if (canUpdateUserRole.IsFailed)
                return Result.Fail(canUpdateUserRole.Errors);

            user.Role = newRole;
        }

        if (dto.Status == UpdateUserStatus.Active)
            user.Status = UserStatus.Active;

        var saveResult = await context.SaveChangesResultAsync(
            logger,
            () => new BaseError()
        );
        if (saveResult.IsFailed)
            return saveResult;

        var resultDto = new UserDTO
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            Status = user.Status,
            CreatedAt = user.CreatedAt
        };

        return Result.Ok(resultDto);
    }

    public Task<Result<IQueryable<UserDTO>>> GetUsers(GetUserQueryDTO dto)
    {
        var dbQuery = context.Users.AsQueryable();

        if (dto.Role.HasValue)
            dbQuery = dbQuery.Where(u => dto.Role.Value == u.Role);

        if (dto.Status.HasValue)
            dbQuery = dbQuery.Where(u => dto.Status.Value == u.Status);

        var resultDTOs = dbQuery.Select(u => new UserDTO
        {
            Id = u.Id,
            Email = u.Email,
            FullName = u.FullName,
            Role = u.Role,
            Status = u.Status,
            CreatedAt = u.CreatedAt
        });

        return Task.FromResult(Result.Ok(resultDTOs));
    }

    public async Task<Result> DeactivateUser(int id, int requesterId)
    {
        var user = await context.Users.FindAsync(id);
        if (user == null)
            return Result.Fail(new UserNotFoundError(id));

        var requester = await context.Users.FindAsync(requesterId);
        if (requester == null)
            return Result.Fail(new UserNotFoundError(requesterId));

        var canDeactivate =
            UserRulesHelper.CanDeactivateUser(requester.Role, user.Role);
        if (canDeactivate.IsFailed) return Result.Fail(canDeactivate.Errors);

        user.Status = UserStatus.Inactive;
        var saveResult = await context.SaveChangesResultAsync(
            logger,
            () => new BaseError()
        );
        return saveResult.IsFailed ? saveResult : Result.Ok();
    }

    public async Task<Result<UserDTO>> CreateUser(CreateUserDTO dto, int requesterId)
    {
        if (await GetUserByEmail(dto.Email) != null)
        {
            logger.LogWarning(
                "Can not create User with email '{Email}' because a user with that email already exists.",
                dto.Email);
            return Result.Fail(new UserEmailAlreadyInUseError(dto.Email));
        }

        var requester = await context.Users.FindAsync(requesterId);
        if (requester == null)
            return Result.Fail<UserDTO>(new UserNotFoundError(requesterId));

        var newRole = UserRulesHelper.MapRole(dto.Role);
        var canCreate = UserRulesHelper.CanCreateUser(requester.Role, newRole);

        if (canCreate.IsFailed) return Result.Fail<UserDTO>(canCreate.Errors);

        var user = new Domain.Entities.User.User
        {
            Email = dto.Email,
            Password = passwordHasher.HashPassword(dto.Password),
            FullName = dto.FullName,
            Status = UserStatus.Active,
            Role = newRole
        };
        context.Users.Add(user);

        var saveResult = await context.SaveChangesResultAsync(
            logger,
            () => new BaseError()
        );
        if (saveResult.IsFailed)
            return saveResult;

        var resultDto = new UserDTO
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            Status = user.Status,
            CreatedAt = user.CreatedAt
        };

        return Result.Ok(resultDto);
    }

    private async Task<Domain.Entities.User.User?> GetUserByEmail(string email)
    {
        return await context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
    }
}
