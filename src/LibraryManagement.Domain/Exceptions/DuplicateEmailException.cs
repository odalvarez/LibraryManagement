namespace LibraryManagement.Domain.Exceptions;

public class DuplicateEmailException : Exception
{
    public DuplicateEmailException() : base("El correo electrónico ya está registrado.") { }
}
