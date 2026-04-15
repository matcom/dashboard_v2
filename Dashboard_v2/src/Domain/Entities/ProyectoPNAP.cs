namespace Dashboard_v2.Domain.Entities;

/// <summary>Proyecto PNAP (Plan Nacional de Alto Potencial o similar).</summary>
public class ProyectoPNAP : ProyectoEnEjecucion
{
    public string FinanciamientoUH { get; set; } = default!;
}
