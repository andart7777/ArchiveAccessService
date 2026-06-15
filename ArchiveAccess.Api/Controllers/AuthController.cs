using System.Security.Claims;
using ArchiveAccess.Api.Dtos;
using ArchiveAccess.Api.Repositories;
using ArchiveAccess.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArchiveAccess.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IArchiveRepository _repository;
    private readonly IAuditService _auditService;

    public AuthController(
        IAuthService authService,
        IArchiveRepository repository,
        IAuditService auditService)
    {
        _authService = authService;
        _repository = repository;
        _auditService = auditService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
    {
        var user = await _authService.ValidateCredentialsAsync(
            request.Username,
            request.Password);

        if (user is null)
        {
            await _auditService.WriteAsync(
                null,
                "auth.login.failed",
                "User",
                null,
                $"Username: {request.Username}");

            return Unauthorized(new { error = "Неверное имя пользователя или пароль." });
        }

        var token = _authService.CreateToken(user);

        await _auditService.WriteAsync(
            user.Id,
            "auth.login.success",
            "User",
            user.Id,
            "User was authenticated.");

        return Ok(new LoginResponseDto(token, user));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserSessionDto>> Me()
    {
        var userId = GetCurrentUserId();
        var user = await _repository.GetUserByIdAsync(userId);

        if (user is null)
        {
            return NotFound(new { error = "Пользователь не найден." });
        }

        return Ok(new UserSessionDto(
            user.Id,
            user.Username,
            user.FullName,
            user.RoleCode,
            user.RoleName,
            user.Department));
    }

    private int GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException("Пользователь не определен.");
        }

        return userId;
    }
}