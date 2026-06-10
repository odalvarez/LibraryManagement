using FluentValidation;
using LibraryManagement.Application.DTOs;

namespace LibraryManagement.Application.Validators;

public class CreateBookValidator : AbstractValidator<CreateBookDto>
{
    public CreateBookValidator()
    {
        // TITLE RULES
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es obligatorio.")
            .MaximumLength(300).WithMessage("El título no puede superar 300 caracteres.");

        // YEAR RULES
        RuleFor(x => x.Year)
            .GreaterThan(0).WithMessage("El año debe ser mayor a 0.")
            .LessThanOrEqualTo(DateTime.Today.Year).WithMessage("El año no puede ser futuro.");

        // GENRE RULES
        RuleFor(x => x.Genre)
            .NotEmpty().WithMessage("El género es obligatorio.")
            .MaximumLength(100).WithMessage("El género no puede superar 100 caracteres.");

        // PAGES RULES
        RuleFor(x => x.Pages)
            .GreaterThan(0).WithMessage("El número de páginas debe ser mayor a 0.");

        // AUTHOR RULES
        RuleFor(x => x.AuthorId)
            .GreaterThan(0).WithMessage("El autor es obligatorio.");
    }
}
