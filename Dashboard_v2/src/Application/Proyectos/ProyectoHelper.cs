using Dashboard_v2.Domain.Entities;

namespace Dashboard_v2.Application.Proyectos;

/// <summary>Métodos estáticos compartidos por todos los handlers de proyectos.</summary>
internal static class ProyectoHelper
{
    internal static void SetBase(Proyecto p, string titulo, string jefe, string correoJefe,
        int numMiembros, int cantUH, int cantEst, int cantEstCont, bool tributaFormacion, string clasificId)
    {
        p.Titulo = titulo.Trim();
        p.Jefe = jefe?.Trim() ?? string.Empty;
        p.CorreoJefe = correoJefe?.Trim() ?? string.Empty;
        p.NumeroMiembros = numMiembros;
        p.CantidadMiembrosUH = cantUH;
        p.CantidadEstudiantes = cantEst;
        p.CantidadEstudiantesContratados = cantEstCont;
        p.TributaFormacionDoctoral = tributaFormacion;
        p.ClasificacionId = clasificId;
    }

    internal static void SetEjecucion(ProyectoEnEjecucion pe,
        DateOnly fechaInicio, DateOnly? fechaCierre,
        string estadoEjecucion, string codigoProyecto, string entidadPrincipal,
        string? entidadParticipante, string? contribSectores, string? contribEjes,
        bool tributaDesarrolloLocal)
    {
        pe.FechaInicio = fechaInicio;
        pe.FechaCierre = fechaCierre;
        pe.EstadoDeEjecucion = estadoEjecucion.Trim();
        pe.CodigoProyecto = codigoProyecto.Trim();
        pe.EntidadEjecutoraPrincipal = entidadPrincipal.Trim();
        pe.EntidadEjecutoraParticipante = entidadParticipante?.Trim();
        pe.ContribucionSectoresEstrategicos = contribSectores?.Trim();
        pe.ContribucionEjesEstrategicos = contribEjes?.Trim();
        pe.TributaDesarrolloLocal = tributaDesarrolloLocal;
    }
}
