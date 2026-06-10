using LibraryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Domain.Interfaces;

public interface ILibraryDbContext
{
    DbSet<Author> Authors { get; }
    DbSet<Book> Books { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
