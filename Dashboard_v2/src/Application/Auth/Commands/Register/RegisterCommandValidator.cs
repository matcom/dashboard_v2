namespace Dashboard_v2.Application.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(v => v.UserName)
            .NotEmpty().WithMessage("El nombre de usuario es requerido.")
            .MinimumLength(3).WithMessage("El nombre de usuario debe tener al menos 3 caracteres.")
            .MaximumLength(100).WithMessage("El nombre de usuario no debe exceder 100 caracteres.");

        RuleFor(v => v.UserLastName)
            .NotEmpty().WithMessage("Los apellidos son requeridos.")
            .MaximumLength(256).WithMessage("Los apellidos no deben exceder 256 caracteres.");

        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("El email es requerido.")
            .EmailAddress().WithMessage("El email no es válido.")
            .MaximumLength(256).WithMessage("El email no debe exceder 256 caracteres.");

        RuleFor(v => v.Password)
            .NotEmpty().WithMessage("La contraseña es requerida.")
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.")
            .MaximumLength(100).WithMessage("La contraseña no debe exceder 100 caracteres.");

        RuleFor(v => v.BirthDate)
            .NotEmpty().WithMessage("La fecha de nacimiento es requerida.")
            .LessThan(DateTime.Today).WithMessage("La fecha de nacimiento debe ser anterior a hoy.");
    }
}
