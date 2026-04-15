namespace Dashboard_v2.Domain.Entities;

/// <summary>
/// Clase base abstracta para proyectos en ejecución.
/// Especializada en PE, PAP, PDL, PNE, PRCI y PNAP.
/// </summary>
public abstract class ProyectoEnEjecucion : Proyecto
{
    public DateOnly FechaInicio { get; set; }
    public DateOnly? FechaCierre { get; set; }
    public string EstadoDeEjecucion { get; set; } = default!;
    public string CodigoProyecto { get; set; } = default!;
    public string EntidadEjecutoraPrincipal { get; set; } = default!;
    public string? EntidadEjecutoraParticipante { get; set; }
    public string? ContribucionSectoresEstrategicos { get; set; }
    public string? ContribucionEjesEstrategicos { get; set; }

    /// <summary>
    /// Indica si el proyecto tributa al desarrollo local.
    /// Para <see cref="ProyectoDesarrolloLocal"/> (PDL) este valor es siempre <c>true</c>:
    /// por definición todo PDL tributa al desarrollo local y no es elegible por el usuario.
    /// Para los demás tipos en ejecución el valor es configurable.
    /// </summary>
    public bool TributaDesarrolloLocal { get; set; }
}
