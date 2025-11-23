using API.Controllers;
using API.Helpers.Response;
using API.Interfaces.Response;
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
        // Arrange
        var controller = CreateController();

        var dto = new CreateUserDTO(
            "test@example.com",
            "Password1!",
            "Test User",
            CreateUserRole.Customer
        );

        var userDto = new UserDTO(
            123,
            dto.Email,
            dto.FullName,
            UserRole.Customer,
            UserStatus.Active,
            DateTime.UtcNow
        );

        _userServiceMock
            .Setup(s => s.CreateUser(dto))
            .ReturnsAsync(Result.Ok(userDto));

        // Act
        var result = await controller.CreateUser(dto);

        // Assert
        var ok = result.Result.Should()
            .BeOfType<CreatedAtActionResult>()
            .Subject;

        var response = ok.Value.Should()
            .BeOfType<ApiResponse<UserDTO>>()
            .Subject;

        response.Result.Should().BeEquivalentTo(userDto);

        _userServiceMock.Verify(s => s.CreateUser(dto), Times.Once);
    }

    [Fact]
    public async Task CreateUser_WhenEmailAlreadyInUse_ReturnsConflict()
    {
        // Arrange
        var controller = CreateController();

        var dto = new CreateUserDTO(
            "used@example.com",
            "Password1!",
            "Used User",
            CreateUserRole.Customer
        );

        var error = new UserEmailAlreadyInUseError(dto.Email);

        _userServiceMock
            .Setup(s => s.CreateUser(dto))
            .ReturnsAsync(Result.Fail<UserDTO>(error));

        // Act
        var result = await controller.CreateUser(dto);

        // Assert
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
        // Arrange
        var controller = CreateController();
        const int userId = 123;

        var userDto = new UserDTO(
            userId,
            "test@example.com",
            "Test User",
            UserRole.Customer,
            UserStatus.Active,
            DateTime.UtcNow
        );

        _userServiceMock
            .Setup(s => s.GetUser(userId))
            .ReturnsAsync(Result.Ok(userDto));

        // Act
        var result = await controller.GetUser(userId);

        // Assert
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
        // Arrange
        var controller = CreateController();
        const int userId = 999;

        var error = new UserNotFoundError(userId);

        _userServiceMock
            .Setup(s => s.GetUser(userId))
            .ReturnsAsync(Result.Fail<UserDTO>(error));

        // Act
        var result = await controller.GetUser(userId);

        // Assert
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
        // Arrange
        var controller = CreateController();
        const int userId = 123;

        _userServiceMock
            .Setup(s => s.DeactivateUser(userId))
            .ReturnsAsync(Result.Ok());

        // Act
        var result = await controller.DeactivateUser(userId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateUser_WhenServiceSucceeds_ReturnsOk()
    {
        // Arrange
        var controller = CreateController();
        const int userId = 123;

        var dto = new UpdateUserDTO(
            "new@example.com",
            "New Name",
            UpdateUserRole.Customer,
            UpdateUserStatus.Active
        );

        var updatedUser = new UserDTO(
            userId,
            dto.Email!,
            dto.FullName!,
            UserRole.Customer,
            UserStatus.Active,
            DateTime.UtcNow
        );

        _userServiceMock
            .Setup(s => s.UpdateUser(userId, dto))
            .ReturnsAsync(Result.Ok(updatedUser));

        // Act
        var result = await controller.UpdateUser(userId, dto);

        // Assert
        var ok = result.Result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        var response = ok.Value.Should()
            .BeOfType<ApiResponse<UserDTO>>()
            .Subject;

        response.Result.Should().BeEquivalentTo(updatedUser);

        _userServiceMock.Verify(s => s.UpdateUser(userId, dto), Times.Once);
    }

    [Fact]
    public async Task UpdateUser_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        var controller = CreateController();
        const int userId = 123;

        var dto = new UpdateUserDTO(
            "new@example.com",
            "New Name",
            UpdateUserRole.Customer,
            UpdateUserStatus.Active
        );

        var error = new UserNotFoundError(userId);

        _userServiceMock
            .Setup(s => s.UpdateUser(userId, dto))
            .ReturnsAsync(Result.Fail<UserDTO>(error));

        // Act
        var result = await controller.UpdateUser(userId, dto);

        // Assert
        var notFound = result.Result.Should()
            .BeOfType<NotFoundObjectResult>()
            .Subject;

        var response = notFound.Value.Should()
            .BeOfType<ApiResponse<object>>()
            .Subject;

        response.Errors.Should().Contain(error.Message);

        _userServiceMock.Verify(s => s.UpdateUser(userId, dto), Times.Once);
    }

    [Fact]
    public async Task UpdateUser_WhenEmailAlreadyInUse_ReturnsBadRequest()
    {
        // Arrange
        var controller = CreateController();
        const int userId = 123;

        var dto = new UpdateUserDTO(
            "taken@example.com",
            "Test User",
            UpdateUserRole.Customer,
            UpdateUserStatus.Active
        );

        var error = new UserEmailAlreadyInUseError(dto.Email!);

        _userServiceMock
            .Setup(s => s.UpdateUser(userId, dto))
            .ReturnsAsync(Result.Fail<UserDTO>(error));

        // Act
        var result = await controller.UpdateUser(userId, dto);

        // Assert
        var badRequest = result.Result.Should()
            .BeOfType<BadRequestObjectResult>()
            .Subject;

        var response = badRequest.Value.Should()
            .BeOfType<ApiResponse<object>>()
            .Subject;

        response.Errors.Should().Contain(error.Message);

        _userServiceMock.Verify(s => s.UpdateUser(userId, dto), Times.Once);
    }

    [Fact]
    public async Task DeactivateUser_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var controller = CreateController();
        const int userId = 123;

        var error = new UserNotFoundError(userId);

        _userServiceMock
            .Setup(s => s.DeactivateUser(userId))
            .ReturnsAsync(Result.Fail(error));

        // Act
        var result = await controller.DeactivateUser(userId);

        // Assert
        var notFound = result.Result.Should()
            .BeOfType<NotFoundObjectResult>()
            .Subject;

        var response = notFound.Value.Should()
            .BeOfType<ApiResponse<object>>()
            .Subject;

        response.Errors.Should().Contain(error.Message);
    }
}
