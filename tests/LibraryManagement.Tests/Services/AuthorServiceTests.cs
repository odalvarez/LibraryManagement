using FluentAssertions;
using LibraryManagement.Application.Services;
using LibraryManagement.Domain.Exceptions;
using LibraryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace LibraryManagement.Tests.Services;

public class AuthorServiceTests : IDisposable
{
    private readonly LibraryDbContext _context;
    private readonly AuthorService _sut;

    public AuthorServiceTests()
    {
        var dbOptions = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new LibraryDbContext(dbOptions);
        _sut = new AuthorService(_context, NullLogger<AuthorService>.Instance);
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsAuthorWithId()
    {
        var result = await _sut.CreateAsync("Isabel Allende", new DateTime(1942, 8, 2), "Lima", "isabel@test.com");

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.FullName.Should().Be("Isabel Allende");
        result.Email.Should().Be("isabel@test.com");
    }

    [Fact]
    public async Task GetAll_ReturnsPaginatedAuthors()
    {
        await _sut.CreateAsync("Autor A", new DateTime(1970, 1, 1), "Bogotá", "a@test.com");
        await _sut.CreateAsync("Autor B", new DateTime(1975, 1, 1), "Medellín", "b@test.com");
        await _sut.CreateAsync("Autor C", new DateTime(1980, 1, 1), "Cali", "c@test.com");

        var page1 = await _sut.GetAllAsync(page: 1, pageSize: 2);
        var total = await _sut.GetTotalCountAsync();

        page1.Should().HaveCount(2);
        total.Should().Be(3);
    }

    [Fact]
    public async Task GetById_WithExistingId_ReturnsAuthor()
    {
        var created = await _sut.CreateAsync("Julio Cortázar", new DateTime(1914, 8, 26), "Bruselas", "julio@test.com");

        var result = await _sut.GetByIdAsync(created.Id);

        result.Should().NotBeNull();
        result!.FullName.Should().Be("Julio Cortázar");
    }

    [Fact]
    public async Task GetById_WithMissingId_ReturnsNull()
    {
        var result = await _sut.GetByIdAsync(9999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task Update_WithExistingId_UpdatesFields()
    {
        var created = await _sut.CreateAsync("Nombre Original", new DateTime(1970, 1, 1), "Ciudad A", "original@test.com");

        var updated = await _sut.UpdateAsync(created.Id, "Nombre Nuevo", new DateTime(1971, 2, 2), "Ciudad B", "nuevo@test.com");

        updated.FullName.Should().Be("Nombre Nuevo");
        updated.City.Should().Be("Ciudad B");
        updated.Email.Should().Be("nuevo@test.com");
    }

    [Fact]
    public async Task Update_WithMissingId_ThrowsAuthorNotFoundException()
    {
        var act = async () => await _sut.UpdateAsync(9999, "X", DateTime.Today.AddYears(-1), "Y", "z@test.com");
        await act.Should().ThrowAsync<AuthorNotFoundException>();
    }

    [Fact]
    public async Task Delete_WithExistingId_RemovesAuthor()
    {
        var created = await _sut.CreateAsync("Para borrar", new DateTime(1980, 1, 1), "Ciudad", "borrar@test.com");

        await _sut.DeleteAsync(created.Id);

        var result = await _sut.GetByIdAsync(created.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task Delete_WithMissingId_ThrowsAuthorNotFoundException()
    {
        var act = async () => await _sut.DeleteAsync(9999);
        await act.Should().ThrowAsync<AuthorNotFoundException>();
    }

    [Fact]
    public async Task Create_WithDuplicateEmail_ThrowsDuplicateEmailException()
    {
        await _sut.CreateAsync("Autor Original", new DateTime(1970, 1, 1), "Ciudad", "duplicado@test.com");

        var act = async () => await _sut.CreateAsync("Otro Autor", new DateTime(1980, 1, 1), "Otra Ciudad", "duplicado@test.com");

        await act.Should().ThrowAsync<DuplicateEmailException>();
    }

    [Fact]
    public async Task Update_WithDuplicateEmail_ThrowsDuplicateEmailException()
    {
        await _sut.CreateAsync("Autor A", new DateTime(1970, 1, 1), "Ciudad A", "autora@test.com");
        var autorB = await _sut.CreateAsync("Autor B", new DateTime(1975, 1, 1), "Ciudad B", "autorb@test.com");

        var act = async () => await _sut.UpdateAsync(autorB.Id, "Autor B", new DateTime(1975, 1, 1), "Ciudad B", "autora@test.com");

        await act.Should().ThrowAsync<DuplicateEmailException>();
    }

    public void Dispose() => _context.Dispose();
}
