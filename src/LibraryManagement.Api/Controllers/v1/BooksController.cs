using Asp.Versioning;
using LibraryManagement.Application.DTOs;
using LibraryManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace LibraryManagement.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[SwaggerTag("Gestión de libros")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Listar libros", Description = "Retorna la lista paginada de libros registrados, incluyendo el nombre del autor.")]
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

    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Obtener libro por ID", Description = "Retorna los datos de un libro específico. Devuelve 404 si no existe.")]
    [ProducesResponseType(typeof(BookResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var book = await _bookService.GetByIdAsync(id, cancellationToken);
        if (book is null)
            return NotFound();

        return Ok(MapToResponse(book));
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Registrar libro", Description = "Crea un nuevo libro. Lanza 404 si el autor no existe, 400 si se alcanzó el máximo permitido.")]
    [ProducesResponseType(typeof(BookResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateBookDto dto, CancellationToken cancellationToken)
    {
        var book = await _bookService.CreateAsync(dto.Title, dto.Year, dto.Genre, dto.Pages, dto.AuthorId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, MapToResponse(book));
    }

    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "Actualizar libro", Description = "Actualiza los datos de un libro existente. Devuelve 404 si el libro o el autor no existen.")]
    [ProducesResponseType(typeof(BookResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBookDto dto, CancellationToken cancellationToken)
    {
        var book = await _bookService.UpdateAsync(id, dto.Title, dto.Year, dto.Genre, dto.Pages, dto.AuthorId, cancellationToken);
        return Ok(MapToResponse(book));
    }

    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "Eliminar libro", Description = "Marca el libro como eliminado (soft delete). No se elimina físicamente de la base de datos. Devuelve 404 si no existe.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _bookService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

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
