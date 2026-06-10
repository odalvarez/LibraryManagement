using Asp.Versioning;
using LibraryManagement.Application.DTOs;
using LibraryManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace LibraryManagement.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[SwaggerTag("Gestión de autores")]
public class AuthorsController : ControllerBase
{
    private readonly IAuthorService _authorService;

    public AuthorsController(IAuthorService authorService)
    {
        _authorService = authorService;
    }

    // GET PAGINATED AUTHORS
    [HttpGet]
    [SwaggerOperation(Summary = "Listar autores", Description = "Retorna la lista paginada de autores registrados.")]
    [ProducesResponseType(typeof(PagedResultDto<AuthorResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var authors = await _authorService.GetAllAsync(page, pageSize, cancellationToken);
        var total = await _authorService.GetTotalCountAsync(cancellationToken);

        var result = new PagedResultDto<AuthorResponseDto>
        {
            Items = authors.Select(MapToResponse),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };

        return Ok(result);
    }

    // GET AUTHOR BY ID
    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Obtener autor por ID", Description = "Retorna los datos de un autor específico. Devuelve 404 si no existe.")]
    [ProducesResponseType(typeof(AuthorResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var author = await _authorService.GetByIdAsync(id, cancellationToken);
        if (author is null)
            return NotFound();

        return Ok(MapToResponse(author));
    }

    // CREATE AUTHOR
    [HttpPost]
    [SwaggerOperation(Summary = "Registrar autor", Description = "Crea un nuevo autor. Todos los campos son obligatorios.")]
    [ProducesResponseType(typeof(AuthorResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAuthorDto dto, CancellationToken cancellationToken)
    {
        var author = await _authorService.CreateAsync(dto.FullName, dto.BirthDate, dto.City, dto.Email, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = author.Id }, MapToResponse(author));
    }

    // UPDATE AUTHOR
    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "Actualizar autor", Description = "Actualiza los datos de un autor existente. Devuelve 404 si no existe.")]
    [ProducesResponseType(typeof(AuthorResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAuthorDto dto, CancellationToken cancellationToken)
    {
        var author = await _authorService.UpdateAsync(id, dto.FullName, dto.BirthDate, dto.City, dto.Email, cancellationToken);
        return Ok(MapToResponse(author));
    }

    // DELETE AUTHOR
    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "Eliminar autor", Description = "Elimina un autor por ID. Devuelve 404 si no existe.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _authorService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    // MAP ENTITY TO RESPONSE DTO
    private static AuthorResponseDto MapToResponse(Domain.Entities.Author author) => new()
    {
        Id = author.Id,
        FullName = author.FullName,
        BirthDate = author.BirthDate,
        City = author.City,
        Email = author.Email
    };
}
