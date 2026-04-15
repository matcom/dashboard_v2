namespace Dashboard_v2.Domain.Entities;

/// <summary>Proyecto No Empresarial (PNE).</summary>
public class ProyectoNoEmpresarial : ProyectoEnEjecucion
{
    public string EntidadNoEmpresarial { get; set; } = default!;
}
