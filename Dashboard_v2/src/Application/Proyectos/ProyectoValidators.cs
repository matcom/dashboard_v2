using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Domain.Constants;

namespace Dashboard_v2.Application.Proyectos;

/// <summary>
/// Validación específica para proyectos en revisión.
/// </summary>
public sealed class ProyectoEnRevisionUpsertRequestValidator
    : ProyectoBaseValidator<ProyectoEnRevisionUpsertRequest>
{
    public ProyectoEnRevisionUpsertRequestValidator(IApplicationDbContext context)
        : base(context)
    {
        RuleFor(x => x.Tipo)
            .NotEmpty().WithMessage("El tipo de proyecto es obligatorio.")
            .Must(tipo => TiposProyectoEjecucion.Validos.Contains(tipo))
            .WithMessage($"El tipo debe ser uno de: {string.Join(", ", TiposProyectoEjecucion.Validos.Order())}.");
    }
}

/// <summary>
/// Validación específica para proyectos empresariales.
/// </summary>
public sealed class ProyectoEmpresarialUpsertRequestValidator
    : ProyectoBaseValidator<ProyectoEmpresarialUpsertRequest>
{
    public ProyectoEmpresarialUpsertRequestValidator(IApplicationDbContext context)
        : base(context)
    {
        RuleFor(x => x.Empresa)
            .NotEmpty()
            .WithMessage("El nombre de la empresa es obligatorio.");
    }
}

/// <summary>
/// Validación específica para proyectos de apoyo a programa.
/// </summary>
public sealed class ProyectoApoyoProgramaUpsertRequestValidator
    : ProyectoBaseValidator<ProyectoApoyoProgramaUpsertRequest>
{
    public ProyectoApoyoProgramaUpsertRequestValidator(IApplicationDbContext context)
        : base(context)
    {
        RuleFor(x => x.NombrePrograma)
            .NotEmpty()
            .WithMessage("El nombre del programa es obligatorio.");
    }
}

/// <summary>
/// Validación específica para proyectos de desarrollo local.
/// </summary>
public sealed class ProyectoDesarrolloLocalUpsertRequestValidator
    : ProyectoBaseValidator<ProyectoDesarrolloLocalUpsertRequest>
{
    public ProyectoDesarrolloLocalUpsertRequestValidator(IApplicationDbContext context)
        : base(context)
    {
        RuleFor(x => x.Municipio)
            .NotEmpty()
            .WithMessage("El municipio es obligatorio.");
    }
}

/// <summary>
/// Validación específica para proyectos no empresariales.
/// </summary>
public sealed class ProyectoNoEmpresarialUpsertRequestValidator
    : ProyectoBaseValidator<ProyectoNoEmpresarialUpsertRequest>
{
    public ProyectoNoEmpresarialUpsertRequestValidator(IApplicationDbContext context)
        : base(context)
    {
        RuleFor(x => x.EntidadNoEmpresarial)
            .NotEmpty()
            .WithMessage("La entidad no empresarial es obligatoria.");
    }
}

/// <summary>
/// Validación específica para proyectos de colaboración internacional.
/// </summary>
public sealed class ProyectoColabInternacionalUpsertRequestValidator
    : ProyectoBaseValidator<ProyectoColabInternacionalUpsertRequest>
{
    public ProyectoColabInternacionalUpsertRequestValidator(IApplicationDbContext context)
        : base(context)
    {
        RuleFor(x => x.FuenteFinanciacion)
            .NotEmpty()
            .WithMessage("La fuente de financiación es obligatoria.");

        RuleFor(x => x.TerminosReferencia)
            .NotEmpty()
            .WithMessage("Los términos de referencia son obligatorios.");
    }
}

/// <summary>
/// Validación específica para proyectos PNAP.
/// </summary>
public sealed class ProyectoPNAPUpsertRequestValidator
    : ProyectoBaseValidator<ProyectoPNAPUpsertRequest>
{
    public ProyectoPNAPUpsertRequestValidator(IApplicationDbContext context)
        : base(context)
    {
        RuleFor(x => x.FinanciamientoUH)
            .NotEmpty()
            .WithMessage("El financiamiento UH es obligatorio.");
    }
}
