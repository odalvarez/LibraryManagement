using Asp.Versioning;
using LibraryManagement.Application.DTOs;
using LibraryManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    // GET PAGINATED BOOKS
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<BookResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var books = await _bookService.GetAllAsync(page, pageSize, cancellationToken);
        var total = await _bookService.GetTotalCountAsync(cancellationToken);

        var result = new PagedResultDto<BookResponseDto>
        {
            Items = books.Select(MapToResponse),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };

        return Ok(result);
    }

    // GET BOOK BY ID
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BookResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var book = await _bookService.GetByIdAsync(id, cancellationToken);
        if (book is null)
            return NotFound();

        return Ok(MapToResponse(book));
    }

    // CREATE BOOK
    [HttpPost]
    [ProducesResponseType(typeof(BookResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateBookDto dto, CancellationToken cancellationToken)
    {
        var book = await _bookService.CreateAsync(dto.Title, dto.Year, dto.Genre, dto.Pages, dto.AuthorId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, MapToResponse(book));
    }

    // UPDATE BOOK
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(BookResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBookDto dto, CancellationToken cancellationToken)
    {
        var book = await _bookService.UpdateAsync(id, dto.Title, dto.Year, dto.Genre, dto.Pages, dto.AuthorId, cancellationToken);
        return Ok(MapToResponse(book));
    }

    // DELETE BOOK
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _bookService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    // MAP ENTITY TO RESPONSE DTO
    private static BookResponseDto MapToResponse(Domain.Entities.Book book) => new()
    {
        Id = book.Id,
        Title = book.Title,
        Year = book.Year,
        Genre = book.Genre,
        Pages = book.Pages,
        AuthorId = book.AuthorId,
        AuthorName = book.Author?.FullName ?? string.Empty
    };
}
