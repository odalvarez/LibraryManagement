using FluentAssertions;
using LibraryManagement.Application.DTOs;
using LibraryManagement.Application.Validators;

namespace LibraryManagement.Tests.Validators;

public class CreateAuthorValidatorTests
{
    private readonly CreateAuthorValidator _validator = new();

    private static CreateAuthorDto ValidDto() => new()
    {
        FullName = "Gabriel García Márquez",
        BirthDate = new DateTime(1927, 3, 6),
        City = "Aracataca",
        Email = "ggm@test.com"
    };

    [Fact]
    public async Task Validate_WithValidData_Passes()
    {
        var result = await _validator.ValidateAsync(ValidDto());
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_EmptyFullName_Fails(string fullName)
    {
        var dto = ValidDto();
        dto.FullName = fullName;

        var result = await _validator.ValidateAsync(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.FullName));
    }

    [Fact]
    public async Task Validate_FutureBirthDate_Fails()
    {
        var dto = ValidDto();
        dto.BirthDate = DateTime.Today.AddDays(1);

        var result = await _validator.ValidateAsync(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.BirthDate));
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing@")]
    [InlineData("@nodomain")]
    public async Task Validate_InvalidEmail_Fails(string email)
    {
        var dto = ValidDto();
        dto.Email = email;

        var result = await _validator.ValidateAsync(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_EmptyCity_Fails(string city)
    {
        var dto = ValidDto();
        dto.City = city;

        var result = await _validator.ValidateAsync(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.City));
    }
}
