using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Proyectos;
using Dashboard_v2.Domain.Entities;
using Dashboard_v2.Domain.Enums;
using Dashboard_v2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;
using RolesEnum = Dashboard_v2.Domain.Enums.Roles;

namespace Dashboard_v2.Application.UnitTests.Proyectos;

/// <summary>
/// Tests for ProyectoService Create and Update operations that exercise
/// <see cref="ProyectoService.CreateEjecucionAsync"/> and <see cref="ProyectoService.UpdateEjecucionAsync"/>
/// generic paths via concrete typed methods.
/// </summary>
[TestFixture]
public class ProyectoServiceCrudTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static ApplicationDbContext CreateDb()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(opts);
    }

    private static ProyectoService BuildService(ApplicationDbContext db, IUser user)
    {
        var validationMock = new Mock<IRequestValidationService>();
        validationMock
            .Setup(v => v.ValidateAndThrowAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new ProyectoService(db, user, validationMock.Object);
    }

    private static IUser MakeSuperuser(string id = "super-1")
    {
        var mock = new Mock<IUser>();
        mock.Setup(u => u.Id).Returns(id);
        mock.Setup(u => u.Roles).Returns(new List<string> { nameof(RolesEnum.Superuser) });
        return mock.Object;
    }

    private static IUser MakeJefe(string id)
    {
        var mock = new Mock<IUser>();
        mock.Setup(u => u.Id).Returns(id);
        mock.Setup(u => u.Roles).Returns(new List<string> { nameof(RolesEnum.Jefe_de_Proyecto) });
        return mock.Object;
    }

    /// <summary>Seeds Area, Clasificacion and a Jefe user. Returns their IDs.</summary>
    private static (string areaId, string clasificId, string jefeId) SeedBase(ApplicationDbContext db)
    {
        var area = new Area { Id = "area-1", Nombre = "MATCOM", Descripcion = "d", UniversidadId = "uh" };
        var clasif = new Clasificacion { Id = "clasif-1", Nombre = "Básica" };
        var jefe = new User
        {
            Id = "jefe-1",
            UserName = "jefe",
            UserLastName1 = "Pérez",
            Email = "jefe@uh.cu",
            PasswordHash = "hash",
            IsActive = true,
            BirthDate = DateTime.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            AreaId = area.Id,
            UserRoles = new List<UserRole>
            {
                new() { UserId = "jefe-1", Role = RolesEnum.Jefe_de_Proyecto }
            }
        };
        db.Areas.Add(area);
        db.Clasificaciones.Add(clasif);
        db.Users.Add(jefe);
        db.SaveChanges();
        return (area.Id, clasif.Id, jefe.Id);
    }

    private static ProyectoEmpresarialUpsertRequest BuildEmpresarialRequest(
        string clasificId, string areaId, string jefeId) => new()
    {
        Titulo = "Proyecto Empresarial Test",
        JefeId = jefeId,
        ClasificacionId = clasificId,
        AreaId = areaId,
        NumeroMiembros = 5,
        CantidadMiembrosUH = 3,
        FechaInicio = DateOnly.FromDateTime(DateTime.Today),
        EstadoDeEjecucion = "En ejecución",
        CodigoProyecto = "PE-001",
        EntidadEjecutoraPrincipal = "UH",
        Empresa = "Empresa Cubana S.A."
    };

    // ── CreateEmpresarialAsync ────────────────────────────────────────────────

    [Test]
    public async Task CreateEmpresarialAsync_Superuser_Succeeds()
    {
        await using var db = CreateDb();
        var (areaId, clasificId, jefeId) = SeedBase(db);

        var service = BuildService(db, MakeSuperuser());
        var request = BuildEmpresarialRequest(clasificId, areaId, jefeId);

        var (result, id) = await service.CreateEmpresarialAsync(request);

        result.Succeeded.ShouldBeTrue();
        id.ShouldNotBeNullOrWhiteSpace();

        var created = await db.Proyectos.OfType<ProyectoEmpresarial>()
            .FirstOrDefaultAsync(p => p.Id == id);
        created.ShouldNotBeNull();
        created!.Titulo.ShouldBe("Proyecto Empresarial Test");
        created.Empresa.ShouldBe("Empresa Cubana S.A.");
        created.CodigoProyecto.ShouldBe("PE-001");
    }

    [Test]
    public async Task CreateEmpresarialAsync_JefeDeProyecto_UsesOwnArea()
    {
        await using var db = CreateDb();
        var (areaId, clasificId, jefeId) = SeedBase(db);

        // Add a second area that Jefe tries to use
        db.Areas.Add(new Area { Id = "area-2", Nombre = "FIS", Descripcion = "d", UniversidadId = "uh" });
        await db.SaveChangesAsync();

        var service = BuildService(db, MakeJefe(jefeId));
        // Jefe requests area-2 but should get area-1 (their own)
        var request = BuildEmpresarialRequest(clasificId, "area-2", jefeId);

        var (result, id) = await service.CreateEmpresarialAsync(request);

        result.Succeeded.ShouldBeTrue();
        var created = await db.Proyectos.OfType<ProyectoEmpresarial>()
            .FirstOrDefaultAsync(p => p.Id == id);
        created!.AreaId.ShouldBe(areaId, "Jefe's project must use their own area");
    }

    // ── UpdateEmpresarialAsync ────────────────────────────────────────────────

    [Test]
    public async Task UpdateEmpresarialAsync_JefeOwner_Succeeds()
    {
        await using var db = CreateDb();
        var (areaId, clasificId, jefeId) = SeedBase(db);

        // First create the project
        var service = BuildService(db, MakeJefe(jefeId));
        var (createResult, id) = await service.CreateEmpresarialAsync(
            BuildEmpresarialRequest(clasificId, areaId, jefeId));
        createResult.Succeeded.ShouldBeTrue();

        // Now update it
        var updateRequest = new ProyectoEmpresarialUpsertRequest
        {
            Titulo = "Titulo Actualizado",
            JefeId = jefeId,
            ClasificacionId = clasificId,
            AreaId = areaId,
            NumeroMiembros = 7,
            CantidadMiembrosUH = 4,
            FechaInicio = DateOnly.FromDateTime(DateTime.Today),
            EstadoDeEjecucion = "Concluido",
            CodigoProyecto = "PE-002",
            EntidadEjecutoraPrincipal = "MINCOM",
            Empresa = "Nueva Empresa"
        };

        var result = await service.UpdateEmpresarialAsync(id!, updateRequest);

        result.Succeeded.ShouldBeTrue();
        var updated = await db.Proyectos.OfType<ProyectoEmpresarial>()
            .FirstOrDefaultAsync(p => p.Id == id);
        updated!.Titulo.ShouldBe("Titulo Actualizado");
        updated.Empresa.ShouldBe("Nueva Empresa");
    }

    [Test]
    public async Task UpdateEmpresarialAsync_JefeUpdatesOthersProject_ReturnsForbidden()
    {
        await using var db = CreateDb();
        var (areaId, clasificId, jefeId) = SeedBase(db);

        // Seed a second jefe
        db.Users.Add(new User
        {
            Id = "jefe-2",
            UserName = "jefe2",
            UserLastName1 = "G",
            Email = "jefe2@uh.cu",
            PasswordHash = "x",
            IsActive = true,
            BirthDate = DateTime.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            AreaId = areaId,
            UserRoles = new List<UserRole>
            {
                new() { UserId = "jefe-2", Role = RolesEnum.Jefe_de_Proyecto }
            }
        });
        await db.SaveChangesAsync();

        // jefe-1 creates a project
        var service1 = BuildService(db, MakeJefe(jefeId));
        var (_, id) = await service1.CreateEmpresarialAsync(BuildEmpresarialRequest(clasificId, areaId, jefeId));

        // jefe-2 tries to update jefe-1's project
        var service2 = BuildService(db, MakeJefe("jefe-2"));
        var result = await service2.UpdateEmpresarialAsync(id!, BuildEmpresarialRequest(clasificId, areaId, "jefe-2"));

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("permiso"));
    }

    // ── UpdateEnRevisionAsync ─────────────────────────────────────────────────

    [Test]
    public async Task UpdateEnRevisionAsync_Superuser_Succeeds()
    {
        await using var db = CreateDb();
        var (areaId, clasificId, jefeId) = SeedBase(db);

        // Seed a ProyectoEnRevision directly
        var proyecto = new ProyectoEnRevision
        {
            Id = "rev-1",
            Titulo = "Proyecto En Revisión",
            JefeId = jefeId,
            AreaId = areaId,
            ClasificacionId = clasificId,
            NumeroMiembros = 2,
            Situacion = "Pendiente",
            Tipo = "PE"
        };
        db.Proyectos.Add(proyecto);
        await db.SaveChangesAsync();

        var service = BuildService(db, MakeSuperuser());
        var request = new ProyectoEnRevisionUpsertRequest
        {
            Titulo = "Revisión Actualizada",
            JefeId = jefeId,
            ClasificacionId = clasificId,
            AreaId = areaId,
            NumeroMiembros = 4,
            CantidadMiembrosUH = 2,
            Situacion = "En Revisión",
            Tipo = "PI"
        };

        var result = await service.UpdateEnRevisionAsync("rev-1", request);

        result.Succeeded.ShouldBeTrue();
        var updated = await db.Proyectos.OfType<ProyectoEnRevision>()
            .FirstOrDefaultAsync(p => p.Id == "rev-1");
        updated!.Titulo.ShouldBe("Revisión Actualizada");
        updated.Situacion.ShouldBe("En Revisión");
    }

    // ── CreateApoyoProgramaAsync ──────────────────────────────────────────────

    [Test]
    public async Task CreateApoyoProgramaAsync_Superuser_Succeeds()
    {
        await using var db = CreateDb();
        var (areaId, clasificId, jefeId) = SeedBase(db);

        var service = BuildService(db, MakeSuperuser());
        var request = new ProyectoApoyoProgramaUpsertRequest
        {
            Titulo = "PAP Nacional",
            JefeId = jefeId,
            ClasificacionId = clasificId,
            AreaId = areaId,
            NumeroMiembros = 3,
            CantidadMiembrosUH = 2,
            FechaInicio = DateOnly.FromDateTime(DateTime.Today),
            EstadoDeEjecucion = "En ejecución",
            CodigoProyecto = "PAP-001",
            EntidadEjecutoraPrincipal = "UH",
            NombrePrograma = "Programa Nacional de IA",
            TipoPAP = TipoPAP.Nacional
        };

        var (result, id) = await service.CreateApoyoProgramaAsync(request);

        result.Succeeded.ShouldBeTrue();
        id.ShouldNotBeNullOrWhiteSpace();
        var created = await db.Proyectos.OfType<ProyectoApoyoPrograma>()
            .FirstOrDefaultAsync(p => p.Id == id);
        created.ShouldNotBeNull();
        created!.NombrePrograma.ShouldBe("Programa Nacional de IA");
        created.TipoPAP.ShouldBe(TipoPAP.Nacional);
    }

    // ── CreateDesarrolloLocalAsync ────────────────────────────────────────────

    [Test]
    public async Task CreateDesarrolloLocalAsync_Superuser_Succeeds()
    {
        await using var db = CreateDb();
        var (areaId, clasificId, jefeId) = SeedBase(db);

        var service = BuildService(db, MakeSuperuser());
        var request = new ProyectoDesarrolloLocalUpsertRequest
        {
            Titulo = "Desarrollo Local Habana",
            JefeId = jefeId,
            ClasificacionId = clasificId,
            AreaId = areaId,
            NumeroMiembros = 4,
            CantidadMiembrosUH = 3,
            FechaInicio = DateOnly.FromDateTime(DateTime.Today),
            EstadoDeEjecucion = "En ejecución",
            CodigoProyecto = "DL-001",
            EntidadEjecutoraPrincipal = "UH",
            Municipio = "Plaza de la Revolución"
        };

        var (result, id) = await service.CreateDesarrolloLocalAsync(request);

        result.Succeeded.ShouldBeTrue();
        var created = await db.Proyectos.OfType<ProyectoDesarrolloLocal>()
            .FirstOrDefaultAsync(p => p.Id == id);
        created!.Municipio.ShouldBe("Plaza de la Revolución");
    }

    // ── GetEmpresarialByIdAsync with data ─────────────────────────────────────

    [Test]
    public async Task GetEmpresarialByIdAsync_WithData_ReturnsDto()
    {
        await using var db = CreateDb();
        var (areaId, clasificId, jefeId) = SeedBase(db);

        var service = BuildService(db, MakeSuperuser());
        var (_, id) = await service.CreateEmpresarialAsync(
            BuildEmpresarialRequest(clasificId, areaId, jefeId));

        var dto = await service.GetEmpresarialByIdAsync(id!);

        dto.ShouldNotBeNull();
        dto!.Titulo.ShouldBe("Proyecto Empresarial Test");
    }

    // ── GetEnRevisionByIdAsync with data ─────────────────────────────────────

    [Test]
    public async Task GetEnRevisionByIdAsync_WithData_ReturnsDto()
    {
        await using var db = CreateDb();
        var (areaId, clasificId, jefeId) = SeedBase(db);

        var service = BuildService(db, MakeSuperuser());
        var proyecto = new ProyectoEnRevision
        {
            Id = "rev-get-1",
            Titulo = "Proyecto Para Leer",
            JefeId = jefeId,
            AreaId = areaId,
            ClasificacionId = clasificId,
            NumeroMiembros = 1,
            Situacion = "Pendiente",
            Tipo = "PE"
        };
        db.Proyectos.Add(proyecto);
        await db.SaveChangesAsync();

        var dto = await service.GetEnRevisionByIdAsync("rev-get-1");

        dto.ShouldNotBeNull();
        dto!.Titulo.ShouldBe("Proyecto Para Leer");
    }

    // ── GetAllAsync with data ─────────────────────────────────────────────────

    [Test]
    public async Task GetAllAsync_WithMultipleTypes_ReturnsAll()
    {
        await using var db = CreateDb();
        var (areaId, clasificId, jefeId) = SeedBase(db);

        var service = BuildService(db, MakeSuperuser());

        // Create two different types
        await service.CreateEnRevisionAsync(new ProyectoEnRevisionUpsertRequest
        {
            Titulo = "En Revisión",
            JefeId = jefeId,
            ClasificacionId = clasificId,
            AreaId = areaId,
            NumeroMiembros = 1,
            CantidadMiembrosUH = 1,
            Situacion = "Pendiente",
            Tipo = "PE"
        });
        await service.CreateEmpresarialAsync(BuildEmpresarialRequest(clasificId, areaId, jefeId));

        var result = await service.GetAllAsync();
        result.Count.ShouldBe(2);
    }

    // ── GetCatalogoAsync with data ────────────────────────────────────────────

    [Test]
    public async Task GetCatalogoAsync_WithData_ReturnsCatalogDtos()
    {
        await using var db = CreateDb();
        var (areaId, clasificId, jefeId) = SeedBase(db);

        var service = BuildService(db, MakeSuperuser());
        await service.CreateEmpresarialAsync(BuildEmpresarialRequest(clasificId, areaId, jefeId));

        var result = await service.GetCatalogoAsync();

        result.Count.ShouldBe(1);
        result[0].Titulo.ShouldBe("Proyecto Empresarial Test");
    }

    // ── GetPublicacionesDelProyectoAsync with data ────────────────────────────

    [Test]
    public async Task GetPublicacionesDelProyectoAsync_WithData_ReturnsPublicacionDtos()
    {
        await using var db = CreateDb();
        var (areaId, clasificId, jefeId) = SeedBase(db);

        var service = BuildService(db, MakeSuperuser());
        var (_, proyId) = await service.CreateEmpresarialAsync(BuildEmpresarialRequest(clasificId, areaId, jefeId));

        // Link a publication
        var pub = new Publication
        {
            Id = "pub-proj-1",
            Title = "Paper del Proyecto",
            NormalizedTitle = "paper del proyecto",
            PublishedDate = "2024",
            PublicationData = "{}",
            ProyectoId = proyId,
            AuthorPublications = new List<AuthorPublication>()
        };
        db.Publications.Add(pub);
        await db.SaveChangesAsync();

        var result = await service.GetPublicacionesDelProyectoAsync(proyId!);

        result.Count.ShouldBe(1);
        result[0].Title.ShouldBe("Paper del Proyecto");
    }

    // ── GetApoyoProgramaByIdAsync with data ───────────────────────────────────

    [Test]
    public async Task GetApoyoProgramaByIdAsync_WithData_ReturnsDto()
    {
        await using var db = CreateDb();
        var (areaId, clasificId, jefeId) = SeedBase(db);

        var service = BuildService(db, MakeSuperuser());
        var (_, id) = await service.CreateApoyoProgramaAsync(new ProyectoApoyoProgramaUpsertRequest
        {
            Titulo = "PAP Test",
            JefeId = jefeId,
            ClasificacionId = clasificId,
            AreaId = areaId,
            NumeroMiembros = 2,
            CantidadMiembrosUH = 1,
            FechaInicio = DateOnly.FromDateTime(DateTime.Today),
            EstadoDeEjecucion = "En ejecución",
            CodigoProyecto = "PAP-99",
            EntidadEjecutoraPrincipal = "UH",
            NombrePrograma = "PN de Biotecnología",
            TipoPAP = TipoPAP.Sectorial
        });

        var dto = await service.GetApoyoProgramaByIdAsync(id!);

        dto.ShouldNotBeNull();
        dto!.Titulo.ShouldBe("PAP Test");
    }

    // ── CreateNoEmpresarialAsync ──────────────────────────────────────────────

    [Test]
    public async Task CreateNoEmpresarialAsync_Succeeds()
    {
        await using var db = CreateDb();
        var (areaId, clasificId, jefeId) = SeedBase(db);

        var service = BuildService(db, MakeSuperuser());
        var (result, id) = await service.CreateNoEmpresarialAsync(new ProyectoNoEmpresarialUpsertRequest
        {
            Titulo = "No Empresarial",
            JefeId = jefeId,
            ClasificacionId = clasificId,
            AreaId = areaId,
            NumeroMiembros = 4,
            CantidadMiembrosUH = 3,
            FechaInicio = DateOnly.FromDateTime(DateTime.Today),
            EstadoDeEjecucion = "En ejecución",
            CodigoProyecto = "NE-001",
            EntidadEjecutoraPrincipal = "CITMA",
            EntidadNoEmpresarial = "Centro de Investigaciones"
        });

        result.Succeeded.ShouldBeTrue();
        var created = await db.Proyectos.OfType<ProyectoNoEmpresarial>()
            .FirstOrDefaultAsync(p => p.Id == id);
        created!.EntidadNoEmpresarial.ShouldBe("Centro de Investigaciones");
    }

    // ── CreateColabInternacionalAsync ─────────────────────────────────────────

    [Test]
    public async Task CreateColabInternacionalAsync_Succeeds()
    {
        await using var db = CreateDb();
        var (areaId, clasificId, jefeId) = SeedBase(db);

        var service = BuildService(db, MakeSuperuser());
        var (result, id) = await service.CreateColabInternacionalAsync(new ProyectoColabInternacionalUpsertRequest
        {
            Titulo = "Colab Internacional",
            JefeId = jefeId,
            ClasificacionId = clasificId,
            AreaId = areaId,
            NumeroMiembros = 6,
            CantidadMiembrosUH = 4,
            FechaInicio = DateOnly.FromDateTime(DateTime.Today),
            EstadoDeEjecucion = "En ejecución",
            CodigoProyecto = "CI-001",
            EntidadEjecutoraPrincipal = "UH",
            FuenteFinanciacion = "EU Horizon",
            TerminosReferencia = "Colaboración sur-sur"
        });

        result.Succeeded.ShouldBeTrue();
        var created = await db.Proyectos.OfType<ProyectoColabInternacional>()
            .FirstOrDefaultAsync(p => p.Id == id);
        created!.FuenteFinanciacion.ShouldBe("EU Horizon");
    }

    // ── CreatePNAPAsync ───────────────────────────────────────────────────────

    [Test]
    public async Task CreatePNAPAsync_Succeeds()
    {
        await using var db = CreateDb();
        var (areaId, clasificId, jefeId) = SeedBase(db);

        var service = BuildService(db, MakeSuperuser());
        var (result, id) = await service.CreatePNAPAsync(new ProyectoPNAPUpsertRequest
        {
            Titulo = "PNAP Test",
            JefeId = jefeId,
            ClasificacionId = clasificId,
            AreaId = areaId,
            NumeroMiembros = 5,
            CantidadMiembrosUH = 3,
            FechaInicio = DateOnly.FromDateTime(DateTime.Today),
            EstadoDeEjecucion = "En ejecución",
            CodigoProyecto = "PNAP-001",
            EntidadEjecutoraPrincipal = "UH",
            FinanciamientoUH = "1.5M CUP"
        });

        result.Succeeded.ShouldBeTrue();
        var created = await db.Proyectos.OfType<ProyectoPNAP>()
            .FirstOrDefaultAsync(p => p.Id == id);
        created!.FinanciamientoUH.ShouldBe("1.5M CUP");
    }
}
