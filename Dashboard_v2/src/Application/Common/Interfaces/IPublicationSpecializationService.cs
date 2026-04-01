using Dashboard_v2.Application.Publications;
using Dashboard_v2.Domain.Entities;

namespace Dashboard_v2.Application.Common.Interfaces;

/// <summary>
/// Gestiona la creación, actualización y eliminación de las entidades de especialización
/// (<see cref="JournalPublication"/>, <see cref="JournalGroup1Publication"/>,
/// <see cref="IndexedPublication"/>) asociadas a una <see cref="Publication"/>.<br/>
/// Centraliza la lógica condicional por <see cref="Dashboard_v2.Domain.Enums.PublicationType"/> para cumplir con SRP y OCP:
/// al añadir un nuevo tipo de especialización solo es necesario modificar esta interfaz
/// y su implementación, sin tocar los handlers de comando.<br/>
/// Recibe datos a través de <see cref="PublicationSpecializationData"/> (Parameter Object)
/// para cumplir con DIP: la interfaz no depende de tipos de comando concretos.
/// </summary>
public interface IPublicationSpecializationService
{
    /// <summary>
    /// Valida que los datos de especialización sean coherentes con el tipo de publicación.<br/>
    /// Retorna <c>null</c> si son válidos, o un mensaje de error si no lo son.
    /// </summary>
    string? Validate(PublicationSpecializationData data);

    /// <summary>
    /// Construye y vincula la entidad de especialización correcta a la publicación recién creada,
    /// según su <see cref="Dashboard_v2.Domain.Enums.PublicationType"/>.
    /// </summary>
    void AttachSpecialization(Publication publication, PublicationSpecializationData data);

    /// <summary>
    /// Actualiza (o reemplaza) la entidad de especialización de una publicación existente,
    /// gestionando las transiciones de tipo (journal → indexed o viceversa).
    /// </summary>
    Task ApplySpecializationUpdateAsync(
        Publication publication,
        PublicationSpecializationData data,
        CancellationToken cancellationToken = default);
}
