using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Proyectos;
using Dashboard_v2.Domain.Entities;
using Dashboard_v2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;
using RolesEnum = Dashboard_v2.Domain.Enums.Roles;

namespace Dashboard_v2.Application.UnitTests.Proyectos;

/// <summary>
/// Tests for read-only ProyectoService methods: GetAllAsync, GetCatalogoAsync, GetTiposEjecucionAsync,
/// GetPublicacionesDelProyectoAsync, GetPublicacionesDisponiblesAsync, GetXByIdAsync.
/// </summary>
[TestFixture]
public class ProyectoServiceReadTests
{
    private ApplicationDbContext _db = null!;
    private ProyectoService _sut = null!;
    private string _areaId = "area-1";
    private string _clasificId = "clasif-1";
    private string _jefeId = "jefe-1";

    [SetUp]
    public void SetUp()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(opts);

        var jefe = new User
        {
            Id = _jefeId,
            UserName = "jefe",
            UserLastName1 = "Perez",
            Email = "jefe@uh.cu",
            PasswordHash = "hash",
            IsActive = true,
            BirthDate = DateTime.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            AreaId = _areaId,
            UserRoles = new List<UserRole> { new() { UserId = _jefeId, Role = RolesEnum.Jefe_de_Proyecto } }
        };
        _db.Areas.Add(new Area { Id = _areaId, Nombre = "MATCOM", Descripcion = "d", UniversidadId = "uh" });
        _db.Clasificaciones.Add(new Clasificacion { Id = _clasificId, Nombre = "Básica" });
        _db.Users.Add(jefe);
        _db.SaveChanges();

        var validationMock = new Mock<IRequestValidationService>();
        validationMock.Setup(v => v.ValidateAndThrowAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var userMock = new Mock<IUser>();
        userMock.Setup(u => u.Id).Returns("super-1");
        userMock.Setup(u => u.Roles).Returns(new List<string> { nameof(RolesEnum.Superuser) });

        _sut = new ProyectoService(_db, userMock.Object, validationMock.Object);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    // ─── GetAllAsync ──────────────────────────────────────────────────────────

    [Test]
    public async Task GetAllAsync_Empty_ReturnsEmpty()
    {
        var result = await _sut.GetAllAsync();
        result.ShouldBeEmpty();
    }

    // ─── GetCatalogoAsync ─────────────────────────────────────────────────────

    [Test]
    public async Task GetCatalogoAsync_Empty_ReturnsEmpty()
    {
        var result = await _sut.GetCatalogoAsync();
        result.ShouldBeEmpty();
    }

    // ─── GetTiposEjecucionAsync ───────────────────────────────────────────────

    [Test]
    public async Task GetTiposEjecucionAsync_ReturnsNonEmpty()
    {
        var result = await _sut.GetTiposEjecucionAsync();
        result.ShouldNotBeEmpty();
    }

    // ─── GetPublicacionesDelProyectoAsync ─────────────────────────────────────

    [Test]
    public async Task GetPublicacionesDelProyectoAsync_NoPublications_ReturnsEmpty()
    {
        var result = await _sut.GetPublicacionesDelProyectoAsync("any-project-id");
        result.ShouldBeEmpty();
    }

    // ─── GetPublicacionesDisponiblesAsync ─────────────────────────────────────

    [Test]
    public async Task GetPublicacionesDisponiblesAsync_Empty_ReturnsEmpty()
    {
        var result = await _sut.GetPublicacionesDisponiblesAsync();
        result.ShouldBeEmpty();
    }

    // ─── GetEnRevisionByIdAsync ───────────────────────────────────────────────

    [Test]
    public async Task GetEnRevisionByIdAsync_NotFound_ReturnsNull()
    {
        var result = await _sut.GetEnRevisionByIdAsync("nonexistent");
        result.ShouldBeNull();
    }

    // ─── GetEmpresarialByIdAsync ──────────────────────────────────────────────

    [Test]
    public async Task GetEmpresarialByIdAsync_NotFound_ReturnsNull()
    {
        var result = await _sut.GetEmpresarialByIdAsync("nonexistent");
        result.ShouldBeNull();
    }

    // ─── GetApoyoProgramaByIdAsync ────────────────────────────────────────────

    [Test]
    public async Task GetApoyoProgramaByIdAsync_NotFound_ReturnsNull()
    {
        var result = await _sut.GetApoyoProgramaByIdAsync("nonexistent");
        result.ShouldBeNull();
    }

    // ─── GetDesarrolloLocalByIdAsync ──────────────────────────────────────────

    [Test]
    public async Task GetDesarrolloLocalByIdAsync_NotFound_ReturnsNull()
    {
        var result = await _sut.GetDesarrolloLocalByIdAsync("nonexistent");
        result.ShouldBeNull();
    }

    // ─── GetNoEmpresarialByIdAsync ────────────────────────────────────────────

    [Test]
    public async Task GetNoEmpresarialByIdAsync_NotFound_ReturnsNull()
    {
        var result = await _sut.GetNoEmpresarialByIdAsync("nonexistent");
        result.ShouldBeNull();
    }

    // ─── GetColabInternacionalByIdAsync ───────────────────────────────────────

    [Test]
    public async Task GetColabInternacionalByIdAsync_NotFound_ReturnsNull()
    {
        var result = await _sut.GetColabInternacionalByIdAsync("nonexistent");
        result.ShouldBeNull();
    }

    // ─── GetPNAPByIdAsync ─────────────────────────────────────────────────────

    [Test]
    public async Task GetPNAPByIdAsync_NotFound_ReturnsNull()
    {
        var result = await _sut.GetPNAPByIdAsync("nonexistent");
        result.ShouldBeNull();
    }

    // ─── UpdateEnRevisionAsync NonExisting ────────────────────────────────────

    [Test]
    public async Task UpdateEnRevisionAsync_NotFound_ReturnsFailure()
    {
        var request = new ProyectoEnRevisionUpsertRequest
        {
            Titulo = "T",
            JefeId = _jefeId,
            ClasificacionId = _clasificId,
            AreaId = _areaId,
            NumeroMiembros = 1,
            CantidadMiembrosUH = 1,
            Situacion = "En Ejecución",
            Tipo = "PE"
        };
        var result = await _sut.UpdateEnRevisionAsync("nonexistent", request);
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("no encontrado"));
    }

    // ─── UpdateEmpresarialAsync NonExisting ───────────────────────────────────

    [Test]
    public async Task UpdateEmpresarialAsync_NotFound_ReturnsFailure()
    {
        var request = new ProyectoEmpresarialUpsertRequest
        {
            Titulo = "T",
            JefeId = _jefeId,
            ClasificacionId = _clasificId,
            AreaId = _areaId,
            NumeroMiembros = 1,
            CantidadMiembrosUH = 1,
            FechaInicio = DateOnly.FromDateTime(DateTime.Today),
            EstadoDeEjecucion = "En Ejecución",
            CodigoProyecto = "PE-001",
            EntidadEjecutoraPrincipal = "UH",
            Empresa = "EmpresaX"
        };
        var result = await _sut.UpdateEmpresarialAsync("nonexistent", request);
        result.Succeeded.ShouldBeFalse();
    }
}
