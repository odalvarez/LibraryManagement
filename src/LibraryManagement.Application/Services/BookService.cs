using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Exceptions;
using LibraryManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LibraryManagement.Application.Services;

public class BookServiceOptions
{
    public int MaxAllowed { get; set; } = 10;
}

public class BookService : IBookService
{
    private readonly ILibraryDbContext _context;
    private readonly ILogger<BookService> _logger;
    private readonly int _maxAllowed;

    public BookService(ILibraryDbContext context, ILogger<BookService> logger, IOptions<BookServiceOptions> options)
    {
        _context = context;
        _logger = logger;
        _maxAllowed = options.Value.MaxAllowed;
    }

    public async Task<IEnumerable<Book>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching books - page {Page}, pageSize {PageSize}", page, pageSize);
        return await _context.Books
            .AsNoTracking()
            .Include(b => b.Author)
            .OrderBy(b => b.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Books.CountAsync(cancellationToken);
    }

    public async Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Books
            .AsNoTracking()
            .Include(b => b.Author)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<Book> CreateAsync(string title, int year, string genre, int pages, int authorId, CancellationToken cancellationToken = default)
    {
        var currentCount = await _context.Books.CountAsync(cancellationToken);
        if (currentCount >= _maxAllowed)
            throw new MaxBooksReachedException();

        var authorExists = await _context.Authors.AnyAsync(a => a.Id == authorId, cancellationToken);
        if (!authorExists)
            throw new AuthorNotFoundException();

        var book = new Book
        {
            Title = title,
            Year = year,
            Genre = genre,
            Pages = pages,
            AuthorId = authorId
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Book created with Id {BookId}", book.Id);
        return book;
    }

    public async Task<Book> UpdateAsync(int id, string title, int year, string genre, int pages, int authorId, CancellationToken cancellationToken = default)
    {
        var book = await _context.Books
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontró el libro con Id {id}.");

        var authorExists = await _context.Authors.AnyAsync(a => a.Id == authorId, cancellationToken);
        if (!authorExists)
            throw new AuthorNotFoundException();

        book.Title = title;
        book.Year = year;
        book.Genre = genre;
        book.Pages = pages;
        book.AuthorId = authorId;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Book {BookId} updated", id);
        return book;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var book = await _context.Books
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontró el libro con Id {id}.");

        book.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Book {BookId} soft-deleted", id);
    }
}
