using API.Controllers;
using API.Helpers.Response;
using API.Interfaces.Response;
using API.Tests.Helpers;
using Application.DTOs.Shared;
using Application.DTOs.User;
using Application.Errors;
using Application.Interfaces.Tracking;
using Application.Interfaces.User;
using Domain.Enums;
using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace API.Tests.Controllers;

public class UserControllerTests
{
    private readonly IApiResponseHelper _responseHelper =
        new ApiResponseHelper(new Mock<ITrackingIdProvider>().Object);

    private readonly Mock<IUserService> _userServiceMock = new();

    private UserController CreateController()
    {
        return new UserController(_responseHelper, _userServiceMock.Object);
    }

    [Fact]
    public async Task CreateUser_WhenServiceSucceeds_ReturnsCreated()
    {
        var controller = CreateController();
        var user = ClaimsHelper.InsertUserInClaims(controller);

        var dto = new CreateUserDTO
        {
            Email = "test@example.com",
            Password = "Password1!",
            FullName = "Test User",
            Role = CreateUserRole.Customer
        };

        var userDto = new UserDTO
        {
            Id = 123,
            Email = dto.Email,
            FullName = dto.FullName,
            Role = UserRole.Customer,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        _userServiceMock
            .Setup(s => s.CreateUser(dto, user.Id))
            .ReturnsAsync(Result.Ok(userDto));

        var result = await controller.CreateUser(dto);

        var ok = result.Result.Should()
            .BeOfType<CreatedAtActionResult>()
            .Subject;

        var response = ok.Value.Should()
            .BeOfType<ApiResponse<UserDTO>>()
            .Subject;

        response.Result.Should().BeEquivalentTo(userDto);

        _userServiceMock.Verify(s => s.CreateUser(dto, user.Id), Times.Once);
    }

    [Fact]
    public async Task CreateUser_WhenEmailAlreadyInUse_ReturnsConflict()
    {
        var controller = CreateController();
        var user = ClaimsHelper.InsertUserInClaims(controller);

        var dto = new CreateUserDTO
        {
            Email = "test@example.com",
            Password = "Password1!",
            FullName = "Test User",
            Role = CreateUserRole.Customer
        };

        var error = new UserEmailAlreadyInUseError(dto.Email);

        _userServiceMock
            .Setup(s => s.CreateUser(dto, user.Id))
            .ReturnsAsync(Result.Fail<UserDTO>(error));

        var result = await controller.CreateUser(dto);

        var conflict = result.Result.Should()
            .BeOfType<ConflictObjectResult>()
            .Subject;

        var response = conflict.Value.Should()
            .BeOfType<ApiResponse<object>>()
            .Subject;

        response.Errors.Should().Contain(error.Message);
    }

    [Fact]
    public async Task GetUser_WhenFound_ReturnsOk()
    {
        var controller = CreateController();
        const int userId = 123;

        var userDto = new UserDTO
        {
            Id = userId,
            Email = "test@example.com",
            FullName = "Test User",
            Role = UserRole.Customer,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        _userServiceMock
            .Setup(s => s.GetUser(userId))
            .ReturnsAsync(Result.Ok(userDto));

        var result = await controller.GetUser(userId);

        var ok = result.Result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        var response = ok.Value.Should()
            .BeOfType<ApiResponse<UserDTO>>()
            .Subject;

        response.Result.Should().BeEquivalentTo(userDto);
    }

    [Fact]
    public async Task GetUser_WhenNotFound_ReturnsNotFound()
    {
        var controller = CreateController();
        const int userId = 999;

        var error = new UserNotFoundError(userId);

        _userServiceMock
            .Setup(s => s.GetUser(userId))
            .ReturnsAsync(Result.Fail<UserDTO>(error));

        var result = await controller.GetUser(userId);

        var notFound = result.Result.Should()
            .BeOfType<NotFoundObjectResult>()
            .Subject;

        var response = notFound.Value.Should()
            .BeOfType<ApiResponse<object>>()
            .Subject;

        response.Errors.Should().Contain(error.Message);
    }

    [Fact]
    public async Task DeactivateUser_WhenFound_ReturnsOk()
    {
        var controller = CreateController();
        var user = ClaimsHelper.InsertUserInClaims(controller);

        const int userId = 123;

        _userServiceMock
            .Setup(s => s.DeactivateUser(userId, user.Id))
            .ReturnsAsync(Result.Ok());

        var result = await controller.DeactivateUser(userId);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeactivateUser_WhenInsufficientPermission_ReturnsConflict()
    {
        var controller = CreateController();
        var user = ClaimsHelper.InsertUserInClaims(controller);

        const int userId = 123;

        var error = new InsufficientPermissionError();

        _userServiceMock
            .Setup(s => s.DeactivateUser(userId, user.Id))
            .ReturnsAsync(error);

        var result = await controller.DeactivateUser(userId);

        var conflict = result.Result.Should()
            .BeOfType<ConflictObjectResult>()
            .Subject;

        var response = conflict.Value.Should()
            .BeOfType<ApiResponse<object>>()
            .Subject;

        response.Errors.Should().Contain(error.Message);
    }

    [Fact]
    public async Task UpdateUser_WhenServiceSucceeds_ReturnsOk()
    {
        var controller = CreateController();
        var user = ClaimsHelper.InsertUserInClaims(controller);
        const int userId = 123;

        var dto = new UpdateUserDTO
        {
            Email = "new@example.com",
            FullName = "New Name",
            Role = UpdateUserRole.Customer,
            Status = UpdateUserStatus.Active
        };

        var updatedUser = new UserDTO
        {
            Id = userId,
            Email = dto.Email!,
            FullName = dto.FullName!,
            Role = UserRole.Customer,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        _userServiceMock
            .Setup(s => s.UpdateUser(userId, dto, user.Id))
            .ReturnsAsync(Result.Ok(updatedUser));

        var result = await controller.UpdateUser(userId, dto);

        var ok = result.Result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        var response = ok.Value.Should()
            .BeOfType<ApiResponse<UserDTO>>()
            .Subject;

        response.Result.Should().BeEquivalentTo(updatedUser);

        _userServiceMock.Verify(s => s.UpdateUser(userId, dto, user.Id), Times.Once);
    }

    [Fact]
    public async Task UpdateUser_WhenUserNotFound_ReturnsNotFound()
    {
        var controller = CreateController();
        var user = ClaimsHelper.InsertUserInClaims(controller);
        const int userId = 123;

        var dto = new UpdateUserDTO
        {
            Email = "new@example.com",
            FullName = "New Name",
            Role = UpdateUserRole.Customer,
            Status = UpdateUserStatus.Active
        };

        var error = new UserNotFoundError(userId);

        _userServiceMock
            .Setup(s => s.UpdateUser(userId, dto, user.Id))
            .ReturnsAsync(Result.Fail<UserDTO>(error));

        var result = await controller.UpdateUser(userId, dto);

        var notFound = result.Result.Should()
            .BeOfType<NotFoundObjectResult>()
            .Subject;

        var response = notFound.Value.Should()
            .BeOfType<ApiResponse<object>>()
            .Subject;

        response.Errors.Should().Contain(error.Message);

        _userServiceMock.Verify(s => s.UpdateUser(userId, dto, user.Id), Times.Once);
    }

    [Fact]
    public async Task UpdateUser_WhenEmailAlreadyInUse_ReturnsBadRequest()
    {
        var controller = CreateController();
        var user = ClaimsHelper.InsertUserInClaims(controller);
        const int userId = 123;

        var dto = new UpdateUserDTO
        {
            Email = "taken@example.com",
            FullName = "Test User",
            Role = UpdateUserRole.Customer,
            Status = UpdateUserStatus.Active
        };

        var error = new UserEmailAlreadyInUseError(dto.Email!);

        _userServiceMock
            .Setup(s => s.UpdateUser(userId, dto, user.Id))
            .ReturnsAsync(Result.Fail<UserDTO>(error));

        var result = await controller.UpdateUser(userId, dto);

        var badRequest = result.Result.Should()
            .BeOfType<BadRequestObjectResult>()
            .Subject;

        var response = badRequest.Value.Should()
            .BeOfType<ApiResponse<object>>()
            .Subject;

        response.Errors.Should().Contain(error.Message);

        _userServiceMock.Verify(s => s.UpdateUser(userId, dto, user.Id), Times.Once);
    }

    [Fact]
    public async Task DeactivateUser_WhenNotFound_ReturnsNotFound()
    {
        var controller = CreateController();
        var user = ClaimsHelper.InsertUserInClaims(controller);
        const int userId = 123;

        var error = new UserNotFoundError(userId);

        _userServiceMock
            .Setup(s => s.DeactivateUser(userId, user.Id))
            .ReturnsAsync(Result.Fail(error));

        var result = await controller.DeactivateUser(userId);

        var notFound = result.Result.Should()
            .BeOfType<NotFoundObjectResult>()
            .Subject;

        var response = notFound.Value.Should()
            .BeOfType<ApiResponse<object>>()
            .Subject;

        response.Errors.Should().Contain(error.Message);
    }
}
