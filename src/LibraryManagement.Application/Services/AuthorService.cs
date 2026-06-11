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

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Authors.CountAsync(cancellationToken);
    }

    public async Task<Author?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Authors
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Author> CreateAsync(string fullName, DateTime birthDate, string city, string email, CancellationToken cancellationToken = default)
    {
        var emailTaken = await _context.Authors.AnyAsync(a => a.Email == email, cancellationToken);
        if (emailTaken)
            throw new DuplicateEmailException();

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

    public async Task<Author> UpdateAsync(int id, string fullName, DateTime birthDate, string city, string email, CancellationToken cancellationToken = default)
    {
        var author = await _context.Authors
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            ?? throw new AuthorNotFoundException();

        var emailTaken = await _context.Authors.AnyAsync(a => a.Email == email && a.Id != id, cancellationToken);
        if (emailTaken)
            throw new DuplicateEmailException();

        author.FullName = fullName;
        author.BirthDate = birthDate;
        author.City = city;
        author.Email = email;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Author {AuthorId} updated", id);
        return author;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var author = await _context.Authors
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            ?? throw new AuthorNotFoundException();

        author.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Author {AuthorId} soft-deleted", id);
    }
}
