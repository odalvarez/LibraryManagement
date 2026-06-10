namespace LibraryManagement.Application.DTOs;

public class CreateAuthorDto
{
    public string FullName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string City { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class UpdateAuthorDto
{
    public string FullName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string City { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class AuthorResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string City { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
