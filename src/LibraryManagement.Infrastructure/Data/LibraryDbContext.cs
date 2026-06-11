using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Data;

public class LibraryDbContext : DbContext, ILibraryDbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.FullName).IsRequired().HasMaxLength(200);
            entity.Property(a => a.City).IsRequired().HasMaxLength(100);
            entity.Property(a => a.Email).IsRequired().HasMaxLength(150);
            entity.HasIndex(a => a.Email).IsUnique().HasFilter("[IsDeleted] = 0");
            entity.HasQueryFilter(a => !a.IsDeleted);
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Title).IsRequired().HasMaxLength(300);
            entity.Property(b => b.Genre).IsRequired().HasMaxLength(100);
            entity.Property(b => b.Year).IsRequired();
            entity.Property(b => b.Pages).IsRequired();
            entity.HasQueryFilter(b => !b.IsDeleted);

            // BOOK -> AUTHOR RELATIONSHIP
            entity.HasOne(b => b.Author)
                  .WithMany(a => a.Books)
                  .HasForeignKey(b => b.AuthorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
