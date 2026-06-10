using FluentValidation;
using LibraryManagement.Application.DTOs;

namespace LibraryManagement.Application.Validators;

public class UpdateAuthorValidator : AbstractValidator<UpdateAuthorDto>
{
    public UpdateAuthorValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("El nombre completo es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede superar 200 caracteres.");

        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("La fecha de nacimiento es obligatoria.")
            .LessThanOrEqualTo(_ => DateTime.Today.AddYears(-16)).WithMessage("El autor debe tener al menos 16 años.")
            .GreaterThan(new DateTime(1900, 1, 1)).WithMessage("La fecha de nacimiento no es válida.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("La ciudad de procedencia es obligatoria.")
            .MaximumLength(100).WithMessage("La ciudad no puede superar 100 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress().WithMessage("El correo electrónico no tiene un formato válido.")
            .MaximumLength(150).WithMessage("El correo no puede superar 150 caracteres.");
    }
}
