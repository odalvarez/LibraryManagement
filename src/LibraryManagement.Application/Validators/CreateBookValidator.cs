using FluentValidation;
using LibraryManagement.Application.DTOs;

namespace LibraryManagement.Application.Validators;

public class CreateBookValidator : AbstractValidator<CreateBookDto>
{
    public CreateBookValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es obligatorio.")
            .MaximumLength(300).WithMessage("El título no puede superar 300 caracteres.");

        RuleFor(x => x.Year)
            .GreaterThan(0).WithMessage("El año debe ser mayor a 0.")
            .LessThanOrEqualTo(_ => DateTime.Today.Year).WithMessage("El año no puede ser futuro.");

        RuleFor(x => x.Genre)
            .NotEmpty().WithMessage("El género es obligatorio.")
            .MaximumLength(100).WithMessage("El género no puede superar 100 caracteres.");

        RuleFor(x => x.Pages)
            .GreaterThan(0).WithMessage("El número de páginas debe ser mayor a 0.");

        RuleFor(x => x.AuthorId)
            .GreaterThan(0).WithMessage("El autor es obligatorio.");
    }
}
