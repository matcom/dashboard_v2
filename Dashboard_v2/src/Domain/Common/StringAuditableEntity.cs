namespace Dashboard_v2.Domain.Common;

/// <summary>
/// Clase base para entidades de dominio con Id de tipo <see langword="string"/> (GUID)
/// que requieren metadatos de auditoría estándar.
/// Las entidades con Id entero usan <see cref="BaseAuditableEntity"/>.
/// </summary>
public abstract class StringAuditableEntity : IAuditableEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTimeOffset Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
}
