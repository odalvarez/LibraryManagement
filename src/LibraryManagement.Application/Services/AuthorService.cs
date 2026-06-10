using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Exceptions;
using LibraryManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LibraryManagement.Application.Services;

public class AuthorService : IAuthorService
{
    private readonly ILibraryDbContext _context;
    private readonly ILogger<AuthorService> _logger;

    public AuthorService(ILibraryDbContext context, ILogger<AuthorService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET PAGINATED AUTHORS
    public async Task<IEnumerable<Author>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching authors - page {Page}, pageSize {PageSize}", page, pageSize);
        return await _context.Authors
            .AsNoTracking()
            .OrderBy(a => a.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    // GET TOTAL AUTHOR COUNT
    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Authors.CountAsync(cancellationToken);
    }

    // GET AUTHOR BY ID
    public async Task<Author?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Authors
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    // CREATE NEW AUTHOR
    public async Task<Author> CreateAsync(string fullName, DateTime birthDate, string city, string email, CancellationToken cancellationToken = default)
    {
        var author = new Author
        {
            FullName = fullName,
            BirthDate = birthDate,
            City = city,
            Email = email
        };

        _context.Authors.Add(author);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Author created with Id {AuthorId}", author.Id);
        return author;
    }

    // UPDATE EXISTING AUTHOR
    public async Task<Author> UpdateAsync(int id, string fullName, DateTime birthDate, string city, string email, CancellationToken cancellationToken = default)
    {
        var author = await _context.Authors.FindAsync([id], cancellationToken)
            ?? throw new AuthorNotFoundException();

        author.FullName = fullName;
        author.BirthDate = birthDate;
        author.City = city;
        author.Email = email;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Author {AuthorId} updated", id);
        return author;
    }

    // DELETE AUTHOR
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var author = await _context.Authors.FindAsync([id], cancellationToken)
            ?? throw new AuthorNotFoundException();

        _context.Authors.Remove(author);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Author {AuthorId} deleted", id);
    }
}
