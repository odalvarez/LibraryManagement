using FluentValidation;
using LibraryManagement.Application.DTOs;

namespace LibraryManagement.Application.Validators;

public class CreateAuthorValidator : AbstractValidator<CreateAuthorDto>
{
    public CreateAuthorValidator()
    {
        // FULL NAME RULES
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("El nombre completo es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede superar 200 caracteres.");

        // BIRTH DATE RULES
        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("La fecha de nacimiento es obligatoria.")
            .LessThan(DateTime.Today).WithMessage("La fecha de nacimiento debe ser anterior a hoy.");

        // CITY RULES
        RuleFor(x => x.City)
            .NotEmpty().WithMessage("La ciudad de procedencia es obligatoria.")
            .MaximumLength(100).WithMessage("La ciudad no puede superar 100 caracteres.");

        // EMAIL RULES
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress().WithMessage("El correo electrónico no tiene un formato válido.")
            .MaximumLength(150).WithMessage("El correo no puede superar 150 caracteres.");
    }
}
