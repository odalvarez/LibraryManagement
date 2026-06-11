namespace LibraryManagement.Domain.Exceptions;

public class BookNotFoundException : Exception
{
    public BookNotFoundException() : base("El libro no está registrado.") { }
}
