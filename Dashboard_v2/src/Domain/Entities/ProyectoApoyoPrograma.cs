namespace Dashboard_v2.Domain.Entities;

/// <summary>Proyecto de Apoyo a Programa (PAP). Tipo: N=Nacional, S=Sectorial, T=Territorial.</summary>
public class ProyectoApoyoPrograma : ProyectoEnEjecucion
{
    public string NombrePrograma { get; set; } = default!;
    public TipoPAP TipoPAP { get; set; }
}
