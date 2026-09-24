using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInvoicePro.Application.Common;
using SmartInvoicePro.Application.DTOs.Auth;
using SmartInvoicePro.Application.Interfaces;

namespace SmartInvoicePro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IWebHostEnvironment _environment;

    public AuthController(IAuthService authService, IWebHostEnvironment environment)
    {
        _authService = authService;
        _environment = environment;
    }

    [HttpPost("fix-demo-credentials")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse>> FixDemoCredentials(
        [FromServices] IDataSeeder dataSeeder)
    {
        if (!_environment.IsDevelopment())
            return NotFound();

        await dataSeeder.EnsureDemoCredentialsAsync();
        return Ok(ApiResponse.Ok("Demo credentials reset to Admin@123 / Staff@123"));
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);
        return Ok(ApiResponse<LoginResponseDto>.Ok(result, "Registration successful"));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(ApiResponse<LoginResponseDto>.Ok(result, "Login successful"));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        return Ok(ApiResponse<LoginResponseDto>.Ok(result));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> Me()
    {
        var user = await _authService.GetCurrentUserAsync();
        return Ok(ApiResponse<UserDto>.Ok(user));
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<ForgotPasswordResponseDto>>> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        var token = await _authService.ForgotPasswordAsync(request);
        var data = new ForgotPasswordResponseDto();
        if (_environment.IsDevelopment() && !string.IsNullOrEmpty(token))
            data.ResetToken = token;

        return Ok(ApiResponse<ForgotPasswordResponseDto>.Ok(
            data,
            "If the email exists, a reset link has been sent (demo: logged to EmailLogs)."));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse>> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        await _authService.ResetPasswordAsync(request);
        return Ok(ApiResponse.Ok("Password reset successful"));
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        await _authService.ChangePasswordAsync(request);
        return Ok(ApiResponse.Ok("Password changed successfully"));
    }
}
