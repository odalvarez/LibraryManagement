using LibraryManagement.Domain.Entities;

namespace LibraryManagement.Domain.Interfaces;

public interface IAuthorService
{
    Task<IEnumerable<Author>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    Task<Author?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Author> CreateAsync(string fullName, DateTime birthDate, string city, string email, CancellationToken cancellationToken = default);
    Task<Author> UpdateAsync(int id, string fullName, DateTime birthDate, string city, string email, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
