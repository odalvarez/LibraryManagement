using FluentAssertions;
using LibraryManagement.Application.Services;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Exceptions;
using LibraryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace LibraryManagement.Tests.Services;

public class BookServiceTests : IDisposable
{
    private readonly LibraryDbContext _context;
    private readonly BookService _sut;

    public BookServiceTests()
    {
        var dbOptions = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new LibraryDbContext(dbOptions);

        var serviceOptions = Options.Create(new BookServiceOptions { MaxAllowed = 3 });
        _sut = new BookService(_context, NullLogger<BookService>.Instance, serviceOptions);
    }

    // SEED HELPER
    private async Task<Author> SeedAuthorAsync()
    {
        var author = new Author
        {
            FullName = "Gabriel García Márquez",
            BirthDate = new DateTime(1927, 3, 6),
            City = "Aracataca",
            Email = "ggm@test.com"
        };
        _context.Authors.Add(author);
        await _context.SaveChangesAsync();
        return author;
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsBook()
    {
        var author = await SeedAuthorAsync();

        var result = await _sut.CreateAsync("Cien años de soledad", 1967, "Realismo mágico", 417, author.Id);

        result.Should().NotBeNull();
        result.Title.Should().Be("Cien años de soledad");
        result.AuthorId.Should().Be(author.Id);
    }

    [Fact]
    public async Task Create_WhenMaxReached_ThrowsMaxBooksReachedException()
    {
        var author = await SeedAuthorAsync();

        for (int i = 1; i <= 3; i++)
            await _sut.CreateAsync($"Libro {i}", 2000, "Ficción", 100, author.Id);

        var act = async () => await _sut.CreateAsync("Libro extra", 2001, "Ficción", 200, author.Id);

        await act.Should().ThrowAsync<MaxBooksReachedException>()
            .WithMessage("No es posible registrar el libro, se alcanzó el máximo permitido.");
    }

    [Fact]
    public async Task Create_WithNonExistentAuthor_ThrowsAuthorNotFoundException()
    {
        var act = async () => await _sut.CreateAsync("Título", 2020, "Drama", 300, authorId: 999);

        await act.Should().ThrowAsync<AuthorNotFoundException>()
            .WithMessage("El autor no está registrado.");
    }

    [Fact]
    public async Task GetAll_ReturnsPaginatedResults()
    {
        var author = await SeedAuthorAsync();
        await _sut.CreateAsync("Libro A", 2000, "Terror", 100, author.Id);
        await _sut.CreateAsync("Libro B", 2001, "Terror", 200, author.Id);
        await _sut.CreateAsync("Libro C", 2002, "Terror", 300, author.Id);

        var page1 = await _sut.GetAllAsync(page: 1, pageSize: 2);
        var page2 = await _sut.GetAllAsync(page: 2, pageSize: 2);

        page1.Should().HaveCount(2);
        page2.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetById_WithExistingId_ReturnsBook()
    {
        var author = await SeedAuthorAsync();
        var created = await _sut.CreateAsync("El amor en los tiempos del cólera", 1985, "Novela", 348, author.Id);

        var result = await _sut.GetByIdAsync(created.Id);

        result.Should().NotBeNull();
        result!.Title.Should().Be("El amor en los tiempos del cólera");
    }

    [Fact]
    public async Task GetById_WithMissingId_ReturnsNull()
    {
        var result = await _sut.GetByIdAsync(9999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task Delete_WithExistingId_RemovesBook()
    {
        var author = await SeedAuthorAsync();
        var book = await _sut.CreateAsync("Para borrar", 2000, "Drama", 100, author.Id);

        await _sut.DeleteAsync(book.Id);

        var result = await _sut.GetByIdAsync(book.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task Delete_WithMissingId_ThrowsBookNotFoundException()
    {
        var act = async () => await _sut.DeleteAsync(9999);
        await act.Should().ThrowAsync<BookNotFoundException>();
    }

    [Fact]
    public async Task Update_WithNonExistentAuthor_ThrowsAuthorNotFoundException()
    {
        var author = await SeedAuthorAsync();
        var book = await _sut.CreateAsync("Original", 2000, "Drama", 100, author.Id);

        var act = async () => await _sut.UpdateAsync(book.Id, "Nuevo", 2001, "Comedia", 200, authorId: 999);

        await act.Should().ThrowAsync<AuthorNotFoundException>();
    }

    public void Dispose() => _context.Dispose();
}
