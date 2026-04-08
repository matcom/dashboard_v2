namespace Dashboard_v2.Domain.Entities;

public class GrupoDeInvestigacion
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Nombre { get; set; } = default!;

    // un Grupo de Investigación pertenece a un Área
    public string AreaId { get; set; } = default!;
    public Area Area { get; set; } = default!;

    // un Grupo de Investigación estudia 0 o muchas Líneas de Investigación
    public ICollection<LineaDeInvestigacion> LineasDeInvestigacion { get; set; } = new List<LineaDeInvestigacion>();

    // un Grupo de Investigación tiene como miembros a 0 o muchos Usuarios
    public ICollection<User> Usuarios { get; set; } = new List<User>();
}
