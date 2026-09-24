using SmartInvoicePro.Application.DTOs.Auth;
using SmartInvoicePro.Application.Validators;

namespace SmartInvoicePro.Tests;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public async Task EmptyEmail_Fails()
    {
        var result = await _validator.ValidateAsync(new LoginRequestDto
        {
            Email = "",
            Password = "Admin@123"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginRequestDto.Email));
    }

    [Fact]
    public async Task InvalidEmail_Fails()
    {
        var result = await _validator.ValidateAsync(new LoginRequestDto
        {
            Email = "not-an-email",
            Password = "Admin@123"
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ValidRequest_Passes()
    {
        var result = await _validator.ValidateAsync(new LoginRequestDto
        {
            Email = "admin@acmeconsulting.in",
            Password = "Admin@123"
        });

        Assert.True(result.IsValid);
    }
}
