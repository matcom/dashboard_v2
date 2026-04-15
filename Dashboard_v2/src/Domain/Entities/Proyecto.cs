namespace Dashboard_v2.Domain.Entities;

/// <summary>
/// Entidad base abstracta para todos los tipos de proyecto.
/// Especializada en <see cref="ProyectoEnRevision"/> y <see cref="ProyectoEnEjecucion"/>.
/// Mapeada con TPH en la tabla "Proyectos".
/// </summary>
public abstract class Proyecto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Titulo { get; set; } = default!;
    /// <summary>Nombre completo del jefe del proyecto.</summary>
    public string Jefe { get; set; } = default!;
    public string CorreoJefe { get; set; } = default!;

    public int NumeroMiembros { get; set; }
    public int CantidadMiembrosUH { get; set; }
    public int CantidadEstudiantes { get; set; }
    public int CantidadEstudiantesContratados { get; set; }
    public bool TributaFormacionDoctoral { get; set; }

    public string ClasificacionId { get; set; } = default!;
    public Clasificacion Clasificacion { get; set; } = default!;
}
