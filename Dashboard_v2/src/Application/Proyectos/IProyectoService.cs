using Dashboard_v2.Application.Common.Models;

namespace Dashboard_v2.Application.Proyectos;

/// <summary>
/// Servicio CRUD para la gestión de proyectos y sus operaciones relacionadas.
/// </summary>
public interface IProyectoService
{
    Task<List<ProyectoResumenDto>> GetAllAsync(CancellationToken ct = default);
    Task<List<ProyectoResumenDto>> GetMisProyectosParticipacionAsync(CancellationToken ct = default);
    Task<Result> SetParticipantesAsync(string proyectoId, IList<string> participantesIds, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetTiposEjecucionAsync(CancellationToken ct = default);
    Task<List<ProyectoCatalogoDto>> GetCatalogoAsync(CancellationToken ct = default);
    Task<List<ProyectoPublicacionDto>> GetPublicacionesDelProyectoAsync(string proyectoId, CancellationToken ct = default);
    Task<List<ProyectoPublicacionDto>> GetPublicacionesDisponiblesAsync(CancellationToken ct = default);
    Task<Result> LinkPublicacionAsync(string proyectoId, string publicationId, CancellationToken ct = default);
    Task<Result> UnlinkPublicacionAsync(string proyectoId, string publicationId, CancellationToken ct = default);
    Task<Result> DeleteAsync(string id, CancellationToken ct = default);
    Task<ProyectoEnRevisionDto?> GetEnRevisionByIdAsync(string id, CancellationToken ct = default);
    Task<(Result Result, string? Id)> CreateEnRevisionAsync(ProyectoEnRevisionUpsertRequest request, CancellationToken ct = default);
    Task<Result> UpdateEnRevisionAsync(string id, ProyectoEnRevisionUpsertRequest request, CancellationToken ct = default);
    Task<ProyectoEmpresarialDto?> GetEmpresarialByIdAsync(string id, CancellationToken ct = default);
    Task<(Result Result, string? Id)> CreateEmpresarialAsync(ProyectoEmpresarialUpsertRequest request, CancellationToken ct = default);
    Task<Result> UpdateEmpresarialAsync(string id, ProyectoEmpresarialUpsertRequest request, CancellationToken ct = default);
    Task<ProyectoApoyoProgramaDto?> GetApoyoProgramaByIdAsync(string id, CancellationToken ct = default);
    Task<(Result Result, string? Id)> CreateApoyoProgramaAsync(ProyectoApoyoProgramaUpsertRequest request, CancellationToken ct = default);
    Task<Result> UpdateApoyoProgramaAsync(string id, ProyectoApoyoProgramaUpsertRequest request, CancellationToken ct = default);
    Task<ProyectoDesarrolloLocalDto?> GetDesarrolloLocalByIdAsync(string id, CancellationToken ct = default);
    Task<(Result Result, string? Id)> CreateDesarrolloLocalAsync(ProyectoDesarrolloLocalUpsertRequest request, CancellationToken ct = default);
    Task<Result> UpdateDesarrolloLocalAsync(string id, ProyectoDesarrolloLocalUpsertRequest request, CancellationToken ct = default);
    Task<ProyectoNoEmpresarialDto?> GetNoEmpresarialByIdAsync(string id, CancellationToken ct = default);
    Task<(Result Result, string? Id)> CreateNoEmpresarialAsync(ProyectoNoEmpresarialUpsertRequest request, CancellationToken ct = default);
    Task<Result> UpdateNoEmpresarialAsync(string id, ProyectoNoEmpresarialUpsertRequest request, CancellationToken ct = default);
    Task<ProyectoColabInternacionalDto?> GetColabInternacionalByIdAsync(string id, CancellationToken ct = default);
    Task<(Result Result, string? Id)> CreateColabInternacionalAsync(ProyectoColabInternacionalUpsertRequest request, CancellationToken ct = default);
    Task<Result> UpdateColabInternacionalAsync(string id, ProyectoColabInternacionalUpsertRequest request, CancellationToken ct = default);
    Task<ProyectoPNAPDto?> GetPNAPByIdAsync(string id, CancellationToken ct = default);
    Task<(Result Result, string? Id)> CreatePNAPAsync(ProyectoPNAPUpsertRequest request, CancellationToken ct = default);
    Task<Result> UpdatePNAPAsync(string id, ProyectoPNAPUpsertRequest request, CancellationToken ct = default);
}
