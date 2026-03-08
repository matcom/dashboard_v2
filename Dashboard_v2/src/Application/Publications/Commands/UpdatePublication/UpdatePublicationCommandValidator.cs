namespace Dashboard_v2.Application.Publications.Commands.UpdatePublication;

public class UpdatePublicationCommandValidator : AbstractValidator<UpdatePublicationCommand>
{
    public UpdatePublicationCommandValidator()
    {
        RuleFor(v => v.Id)
            .GreaterThan(0);

        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("El título es requerido.")
            .MaximumLength(500).WithMessage("El título no debe exceder 500 caracteres.");

        RuleFor(v => v.PublicationTypeId)
            .GreaterThan(0).WithMessage("Debe seleccionar un tipo de publicación.");

        RuleFor(v => v.AuthorRelation)
            .MaximumLength(1000).When(v => v.AuthorRelation != null);

        RuleFor(v => v.JournalQuartile)
            .MaximumLength(10).When(v => v.IsJournal && v.JournalQuartile != null);
    }
}
