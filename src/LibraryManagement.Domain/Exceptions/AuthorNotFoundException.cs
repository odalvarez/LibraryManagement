namespace LibraryManagement.Domain.Exceptions;

public class AuthorNotFoundException : Exception
{
    public AuthorNotFoundException()
        : base("El autor no está registrado.") { }
}
