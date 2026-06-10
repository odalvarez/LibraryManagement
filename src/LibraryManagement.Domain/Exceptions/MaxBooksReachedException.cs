namespace LibraryManagement.Domain.Exceptions;

public class MaxBooksReachedException : Exception
{
    public MaxBooksReachedException()
        : base("No es posible registrar el libro, se alcanzó el máximo permitido.") { }
}
