namespace Dashboard_v2.Application.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(v => v.UserName)
            .NotEmpty().WithMessage("El nombre de usuario es requerido.")
            .MinimumLength(3).WithMessage("Mínimo 3 caracteres.")
            .MaximumLength(50).WithMessage("Máximo 50 caracteres.");

        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("El email es requerido.")
            .EmailAddress().WithMessage("Email no válido.")
            .MaximumLength(256);

        RuleFor(v => v.Password)
            .NotEmpty().WithMessage("La contraseña es requerida.")
            .MinimumLength(6).WithMessage("Mínimo 6 caracteres.")
            .MaximumLength(100);
    }
}
