namespace Dashboard_v2.Domain.Entities;

public class AreaDelConocimiento
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Nombre { get; set; } = default!;
    public string? Descripcion { get; set; }

    // un Área del Conocimiento es investigada por 0 o muchas Áreas
    public ICollection<Area> Areas { get; set; } = new List<Area>();

    // un Área del Conocimiento posee 0 o muchas Líneas de Investigación
    public ICollection<LineaDeInvestigacion> LineasDeInvestigacion { get; set; } = new List<LineaDeInvestigacion>();
}
