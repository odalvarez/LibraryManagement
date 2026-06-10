using FluentAssertions;
using LibraryManagement.Application.DTOs;
using LibraryManagement.Application.Validators;

namespace LibraryManagement.Tests.Validators;

public class CreateBookValidatorTests
{
    private readonly CreateBookValidator _validator = new();

    private static CreateBookDto ValidDto() => new()
    {
        Title = "Cien años de soledad",
        Year = 1967,
        Genre = "Realismo mágico",
        Pages = 417,
        AuthorId = 1
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
    public async Task Validate_EmptyTitle_Fails(string title)
    {
        var dto = ValidDto();
        dto.Title = title;

        var result = await _validator.ValidateAsync(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Title));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_InvalidYear_Fails(int year)
    {
        var dto = ValidDto();
        dto.Year = year;

        var result = await _validator.ValidateAsync(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Year));
    }

    [Fact]
    public async Task Validate_FutureYear_Fails()
    {
        var dto = ValidDto();
        dto.Year = DateTime.Today.Year + 1;

        var result = await _validator.ValidateAsync(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Year));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Validate_InvalidPages_Fails(int pages)
    {
        var dto = ValidDto();
        dto.Pages = pages;

        var result = await _validator.ValidateAsync(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Pages));
    }

    [Fact]
    public async Task Validate_MissingAuthor_Fails()
    {
        var dto = ValidDto();
        dto.AuthorId = 0;

        var result = await _validator.ValidateAsync(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.AuthorId));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_EmptyGenre_Fails(string genre)
    {
        var dto = ValidDto();
        dto.Genre = genre;

        var result = await _validator.ValidateAsync(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Genre));
    }
}
