using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Documents.Reports;
using Dashboard_v2.Domain.Entities;
using Dashboard_v2.Domain.Enums;
using Dashboard_v2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Dashboard_v2.Application.UnitTests.Documents.Reports;

[TestFixture]
public class Anexo1ReportTests
{
    private ApplicationDbContext _db = null!;
    private Mock<IUser> _userMock = null!;
    private Anexo1Report _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(options);
        _userMock = new Mock<IUser>();
        _userMock.Setup(u => u.Id).Returns((string?)null);
        _sut = new Anexo1Report(_db, _userMock.Object);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    // ── Identity ─────────────────────────────────────────────────────────────

    [Test]
    public void ReportName_ReturnsExpected()
    {
        _sut.ReportName.ShouldBe("anexo-1");
    }

    [Test]
    public void TemplateName_ReturnsExpected()
    {
        _sut.TemplateName.ShouldBe("Anexo1");
    }

    // ── Empty DB — all keys present with zero values ──────────────────────────

    [Test]
    public async Task GatherVariablesAsync_Empty_ReturnsAllExpectedKeys()
    {
        var result = await _sut.GatherVariablesAsync(null, default);

        result.ShouldContainKey("RedesUniversitariasProfesores");
        result.ShouldContainKey("RedesTotalProfesores");
        result.ShouldContainKey("PatentesCuba");
        result.ShouldContainKey("PatentesTotal");
        result.ShouldContainKey("RegistrosNoInformaticos");
        result.ShouldContainKey("NormasNacionales");
        result.ShouldContainKey("NormasTotal");
        result.ShouldContainKey("NuevosProductos");
        result.ShouldContainKey("NuevosProductosTecServTotal");
        result.ShouldContainKey("Premios");
        result.ShouldContainKey("PonenciasEventosNacionalesReal");
        result.ShouldContainKey("EventosOrganizadosReal");
        result.ShouldContainKey("PDLTotal");
        result.ShouldContainKey("PDLTerminados");
        result.ShouldContainKey("G1Count");
        result.ShouldContainKey("G2Count");
        result.ShouldContainKey("ArticulosDivulgacionCount");
        result.ShouldContainKey("IndicePublicacionesTotalProfesor");
    }

    [Test]
    public async Task GatherVariablesAsync_Empty_ZeroScalarsAndEmptyList()
    {
        var result = await _sut.GatherVariablesAsync(null, default);

        result["RedesTotalProfesores"].ShouldBe(0);
        result["PatentesTotal"].ShouldBe(0);
        result["NormasTotal"].ShouldBe(0);
        result["PDLTotal"].ShouldBe(0);
        result["G1Count"].ShouldBe(0);
        (result["Premios"] as List<Anexo1PremioRowDto>)!.ShouldBeEmpty();
    }

    // ── Redes ─────────────────────────────────────────────────────────────────

    [Test]
    public async Task GatherVariablesAsync_RedesUniversitarias_SumsProfesores()
    {
        _db.Reds.Add(new Red { Nombre = "Red U1", Tipo = TipoRed.Universitaria, CantidadProfesores = 4 });
        _db.Reds.Add(new Red { Nombre = "Red U2", Tipo = TipoRed.Universitaria, CantidadProfesores = 6 });
        _db.Reds.Add(new Red { Nombre = "Red N1", Tipo = TipoRed.Nacional,      CantidadProfesores = 3 });
        await _db.SaveChangesAsync();

        var result = await _sut.GatherVariablesAsync(null, default);

        result["RedesUniversitariasProfesores"].ShouldBe(10);
        result["RedesNacionalesProfesores"].ShouldBe(3);
        result["RedesTotalProfesores"].ShouldBe(13);
    }

    // ── Patentes ──────────────────────────────────────────────────────────────

    [Test]
    public async Task GatherVariablesAsync_Patentes_CountsByNacionalFlag()
    {
        _db.Patentes.Add(new Patente { Titulo = "P1", NumeroSolicitudConcesion = "A1", EsNacional = true });
        _db.Patentes.Add(new Patente { Titulo = "P2", NumeroSolicitudConcesion = "A2", EsNacional = true });
        _db.Patentes.Add(new Patente { Titulo = "P3", NumeroSolicitudConcesion = "A3", EsNacional = false });
        await _db.SaveChangesAsync();

        var result = await _sut.GatherVariablesAsync(null, default);

        result["PatentesCuba"].ShouldBe(2);
        result["PatentesExtranjero"].ShouldBe(1);
        result["PatentesTotal"].ShouldBe(3);
    }

    // ── Registros ─────────────────────────────────────────────────────────────

    [Test]
    public async Task GatherVariablesAsync_Registros_CountsByInformaticoFlag()
    {
        var inst = new Institution { Id = "inst-r1", Nombre = "UH" };
        var country = new Country { Id = 1, Name = "Cuba" };
        _db.Institutions.Add(inst);
        _db.Countries.Add(country);
        _db.Registros.Add(new Registro { Titulo = "R1", NumeroCertificado = "R001", EsInformatico = false, InstitutionId = inst.Id, CountryId = country.Id });
        _db.Registros.Add(new Registro { Titulo = "R2", NumeroCertificado = "R002", EsInformatico = true,  InstitutionId = inst.Id, CountryId = country.Id });
        await _db.SaveChangesAsync();

        var result = await _sut.GatherVariablesAsync(null, default);

        result["RegistrosNoInformaticos"].ShouldBe(1);
        result["RegistrosInformaticos"].ShouldBe(1);
        result["RegistrosTotal"].ShouldBe(2);
    }

    // ── Normas ────────────────────────────────────────────────────────────────

    [Test]
    public async Task GatherVariablesAsync_Normas_CountsByTipoNombreKeyword()
    {
        var inst = new Institution { Id = "inst-n1", Nombre = "CITMA" };
        _db.Institutions.Add(inst);

        var tNac    = new TipoNorma { Nombre = "Nacional" };
        var tRamal  = new TipoNorma { Nombre = "Ramal" };
        var tEmp    = new TipoNorma { Nombre = "Empresarial" };
        _db.TiposNorma.AddRange(tNac, tRamal, tEmp);
        await _db.SaveChangesAsync();

        _db.Normas.Add(new Norma { Titulo = "N1", TipoNormaId = tNac.Id,   InstitutionId = inst.Id });
        _db.Normas.Add(new Norma { Titulo = "N2", TipoNormaId = tNac.Id,   InstitutionId = inst.Id });
        _db.Normas.Add(new Norma { Titulo = "N3", TipoNormaId = tRamal.Id, InstitutionId = inst.Id });
        _db.Normas.Add(new Norma { Titulo = "N4", TipoNormaId = tEmp.Id,   InstitutionId = inst.Id });
        await _db.SaveChangesAsync();

        var result = await _sut.GatherVariablesAsync(null, default);

        result["NormasNacionales"].ShouldBe(2);
        result["NormasRamales"].ShouldBe(1);
        result["NormasEmpresariales"].ShouldBe(1);
        result["NormasTotal"].ShouldBe(4);
    }

    // ── Premios (list) ───────────────────────────────────────────────────────

    [Test]
    public async Task GatherVariablesAsync_Premios_GroupsByAwardType()
    {
        var tipo1 = new AwardType { Name = "ACC" };
        var tipo2 = new AwardType { Name = "MES" };
        _db.AwardTypes.AddRange(tipo1, tipo2);
        await _db.SaveChangesAsync();

        var award1 = new Award { AwardTypeId = tipo1.Id, Name = "Premio A" };
        var award2 = new Award { AwardTypeId = tipo1.Id, Name = "Premio B" };
        var award3 = new Award { AwardTypeId = tipo2.Id, Name = "Premio C" };
        _db.Awards.AddRange(award1, award2, award3);
        await _db.SaveChangesAsync();

        var user1 = new User { Id = "u1", Email = "u1@t.cu", UserName = "u1", UserLastName1 = "U" };
        var user2 = new User { Id = "u2", Email = "u2@t.cu", UserName = "u2", UserLastName1 = "U" };
        _db.Users.AddRange(user1, user2);
        await _db.SaveChangesAsync();

        _db.UserAwardeds.Add(new UserAwarded { UserId = user1.Id, AwardId = award1.Id, AwardedAt = DateTime.UtcNow });
        _db.UserAwardeds.Add(new UserAwarded { UserId = user2.Id, AwardId = award2.Id, AwardedAt = DateTime.UtcNow });
        _db.UserAwardeds.Add(new UserAwarded { UserId = user1.Id, AwardId = award3.Id, AwardedAt = DateTime.UtcNow });
        await _db.SaveChangesAsync();

        var result = await _sut.GatherVariablesAsync(null, default);
        var premios = result["Premios"] as List<Anexo1PremioRowDto>;

        premios.ShouldNotBeNull();
        premios!.Count.ShouldBe(2);
        premios.Single(p => p.TipoPremio == "ACC").Cantidad.ShouldBe(2);
        premios.Single(p => p.TipoPremio == "MES").Cantidad.ShouldBe(1);
    }

    // ── PDL ──────────────────────────────────────────────────────────────────

    [Test]
    public async Task GatherVariablesAsync_PDL_CountsTotalAndByEstado()
    {
        var provincia = new Provincia { Nombre = "La Habana" };
        _db.Provincias.Add(provincia);
        await _db.SaveChangesAsync();

        var municipio = new Municipio { Nombre = "Playa", ProvinciaId = provincia.Id };
        _db.Municipios.Add(municipio);
        var clasificacion = new Clasificacion { Nombre = "Básica" };
        _db.Clasificaciones.Add(clasificacion);
        var jefe = new User { Id = "jefe-pdl1", Email = "j@t.cu", UserName = "jefe", UserLastName1 = "J" };
        _db.Users.Add(jefe);
        await _db.SaveChangesAsync();

        var estadoTerminado = new EstadoProyecto { Nombre = "Terminado" };
        var estadoEjecucion = new EstadoProyecto { Nombre = "En ejecución normal" };
        _db.EstadosProyecto.AddRange(estadoTerminado, estadoEjecucion);
        await _db.SaveChangesAsync();

        var pdl1 = new ProyectoDesarrolloLocal
        {
            Titulo = "PDL 1", JefeId = jefe.Id, MunicipioId = municipio.Id,
            ClasificacionId = clasificacion.Id, CodigoProyecto = "PDL-001",
            FechaInicio = DateOnly.FromDateTime(DateTime.Today)
        };
        var pdl2 = new ProyectoDesarrolloLocal
        {
            Titulo = "PDL 2", JefeId = jefe.Id, MunicipioId = municipio.Id,
            ClasificacionId = clasificacion.Id, CodigoProyecto = "PDL-002",
            FechaInicio = DateOnly.FromDateTime(DateTime.Today)
        };
        _db.Proyectos.AddRange(pdl1, pdl2);
        await _db.SaveChangesAsync();

        pdl1.EstadosDeEjecucion.Add(estadoTerminado);
        pdl2.EstadosDeEjecucion.Add(estadoEjecucion);
        await _db.SaveChangesAsync();

        var result = await _sut.GatherVariablesAsync(null, default);

        result["PDLTotal"].ShouldBe(2);
        result["PDLTerminados"].ShouldBe(1);
        result["PDLEnEjecucion"].ShouldBe(1);
        result["PDLAtrasados"].ShouldBe(0);
    }

    // ── Publicaciones ─────────────────────────────────────────────────────────

    private static Publication MakeJournalPub(string id, string title, int group, string date)
    {
        var pub = new Publication
        {
            Id = id, Title = title, PublicationData = "datos",
            PublishedDate = date, PublicationType = PublicationType.Diario,
        };
        pub.JournalPublication = new JournalPublication { PublicationId = id, Group = group };
        return pub;
    }

    [Test]
    public async Task GatherVariablesAsync_Publicaciones_CountsGroupsCorrectly()
    {
        _db.Publications.Add(MakeJournalPub("pub-g1", "Pub G1", group: 1, date: "2025-01"));
        _db.Publications.Add(MakeJournalPub("pub-g2", "Pub G2", group: 2, date: "2025-02"));
        await _db.SaveChangesAsync();

        var result = await _sut.GatherVariablesAsync(null, default);

        result["G1Count"].ShouldBe(1);
        result["G2Count"].ShouldBe(1);
        result["G3Count"].ShouldBe(0);
    }

    // ── Date filter (publications) ────────────────────────────────────────────

    [Test]
    public async Task GatherVariablesAsync_WithDateFilter_FiltersPublicaciones()
    {
        _db.Publications.Add(MakeJournalPub("pub-old", "Old pub", group: 1, date: "2020-06"));
        _db.Publications.Add(MakeJournalPub("pub-new", "New pub", group: 1, date: "2025-03"));
        await _db.SaveChangesAsync();

        var result = await _sut.GatherVariablesAsync(
            new Dictionary<string, string> { ["from"] = "2024-01" }, default);

        result["G1Count"].ShouldBe(1);
    }

    // ── Area filter ───────────────────────────────────────────────────────────

    [Test]
    public async Task GatherVariablesAsync_WithAreaFilter_PatentesOnlyFromUserArea()
    {
        _db.Users.Add(new User { Id = "req-ar1", AreaId = "area-a", Email = "r@t.cu", UserName = "req", UserLastName1 = "R" });
        _db.Users.Add(new User { Id = "creator-ar1", AreaId = "area-a", Email = "c@t.cu", UserName = "c", UserLastName1 = "C" });
        _db.Users.Add(new User { Id = "other-ar1",   AreaId = "area-b", Email = "o@t.cu", UserName = "o", UserLastName1 = "O" });
        _db.Authors.Add(new Author { Id = "auth-ar1", LastName = "C", Name = "C", SearchKey = "c", LastNameKey = "c", UserId = "creator-ar1" });
        _db.Authors.Add(new Author { Id = "auth-br1", LastName = "O", Name = "O", SearchKey = "o", LastNameKey = "o", UserId = "other-ar1" });
        _db.Patentes.Add(new Patente { Id = "pat-ar1", Titulo = "Patente área A", NumeroSolicitudConcesion = "P1", EsNacional = true });
        _db.Patentes.Add(new Patente { Id = "pat-br1", Titulo = "Patente área B", NumeroSolicitudConcesion = "P2", EsNacional = false });
        await _db.SaveChangesAsync();

        _db.Add(new AuthorPatente { PatenteId = "pat-ar1", AuthorId = "auth-ar1" });
        _db.Add(new AuthorPatente { PatenteId = "pat-br1", AuthorId = "auth-br1" });
        await _db.SaveChangesAsync();

        _userMock.Setup(u => u.Id).Returns("req-ar1");

        var result = await _sut.GatherVariablesAsync(null, default);

        result["PatentesCuba"].ShouldBe(1);
        result["PatentesExtranjero"].ShouldBe(0);
        result["PatentesTotal"].ShouldBe(1);
    }
}
