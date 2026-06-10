using LibraryManagement.Domain.Entities;

namespace LibraryManagement.Domain.Interfaces;

public interface IBookService
{
    Task<IEnumerable<Book>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Book> CreateAsync(string title, int year, string genre, int pages, int authorId, CancellationToken cancellationToken = default);
    Task<Book> UpdateAsync(int id, string title, int year, string genre, int pages, int authorId, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
