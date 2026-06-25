using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Documents.Reports;
using Dashboard_v2.Domain.Entities;
using Dashboard_v2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Dashboard_v2.Application.UnitTests.Documents.Reports;

[TestFixture]
public class AnexoRegistrosReportTests
{
    private ApplicationDbContext _db = null!;
    private Mock<IUser> _userMock = null!;
    private AnexoRegistrosReport _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(options);
        _userMock = new Mock<IUser>();
        _userMock.Setup(u => u.Id).Returns((string?)null);
        _sut = new AnexoRegistrosReport(_db, _userMock.Object);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    [Test]
    public void ReportName_ReturnsExpected()
    {
        _sut.ReportName.ShouldBe("anexo-registros");
    }

    [Test]
    public void TemplateName_ReturnsExpected()
    {
        _sut.TemplateName.ShouldBe("AnexoRegistros");
    }

    [Test]
    public async Task GatherVariablesAsync_Empty_ReturnsAllExpectedKeys()
    {
        var result = await _sut.GatherVariablesAsync(null, default);

        result.ShouldContainKey("Patentes");
        result.ShouldContainKey("RegistrosInformaticos");
        result.ShouldContainKey("RegistrosNoInformaticos");
        result.ShouldContainKey("Normas");
        result.ShouldContainKey("ProductosTipos");
    }

    [Test]
    public async Task GatherVariablesAsync_Empty_AllListsAreEmpty()
    {
        var result = await _sut.GatherVariablesAsync(null, default);

        (result["Patentes"] as List<AnexoRegistrosPatenteRowDto>)!.ShouldBeEmpty();
        (result["RegistrosInformaticos"] as List<AnexoRegistroRowDto>)!.ShouldBeEmpty();
        (result["RegistrosNoInformaticos"] as List<AnexoRegistroRowDto>)!.ShouldBeEmpty();
        (result["Normas"] as List<AnexoNormaRowDto>)!.ShouldBeEmpty();
        (result["ProductosTipos"] as List<AnexoProductoTipoRowDto>)!.ShouldBeEmpty();
    }

    [Test]
    public async Task GatherVariablesAsync_WithPatente_PopulatesPatenteRow()
    {
        _db.Patentes.Add(new Patente
        {
            Titulo = "Patente de Prueba",
            NumeroSolicitudConcesion = "P-2024-001",
            EsNacional = true,
        });
        await _db.SaveChangesAsync();

        var result = await _sut.GatherVariablesAsync(null, default);

        var patentes = (result["Patentes"] as List<AnexoRegistrosPatenteRowDto>)!;
        patentes.Count.ShouldBe(1);
        patentes[0].Titulo.ShouldBe("Patente de Prueba");
        patentes[0].NumeroSolicitudConcesion.ShouldBe("P-2024-001");
        patentes[0].EsNacional.ShouldBeTrue();
    }

    [Test]
    public async Task GatherVariablesAsync_WithRegistro_PopulatesRow()
    {
        var institution = new Institution { Id = "inst-1", Nombre = "IDICT" };
        var country = new Country { Id = 1, Name = "Cuba" };
        _db.Institutions.Add(institution);
        _db.Countries.Add(country);
        await _db.SaveChangesAsync();

        _db.Registros.Add(new Registro
        {
            Titulo = "Registro de Prueba",
            NumeroCertificado = "R-001",
            EsInformatico = false,
            InstitutionId = institution.Id,
            CountryId = country.Id
        });
        await _db.SaveChangesAsync();

        var result = await _sut.GatherVariablesAsync(null, default);

        var noInf = (result["RegistrosNoInformaticos"] as List<AnexoRegistroRowDto>)!;
        noInf.Count.ShouldBe(1);
        noInf[0].Titulo.ShouldBe("Registro de Prueba");
        noInf[0].NumeroCertificado.ShouldBe("R-001");
        (result["RegistrosInformaticos"] as List<AnexoRegistroRowDto>)!.ShouldBeEmpty();
    }

    [Test]
    public async Task GatherVariablesAsync_WithRegistroInformatico_PopulatesInformaticoList()
    {
        var institution = new Institution { Id = "inst-2", Nombre = "UH" };
        var country = new Country { Id = 2, Name = "Cuba" };
        _db.Institutions.Add(institution);
        _db.Countries.Add(country);
        await _db.SaveChangesAsync();

        _db.Registros.Add(new Registro
        {
            Titulo = "Software Educativo",
            NumeroCertificado = "S-001",
            EsInformatico = true,
            InstitutionId = institution.Id,
            CountryId = country.Id
        });
        await _db.SaveChangesAsync();

        var result = await _sut.GatherVariablesAsync(null, default);

        var inf = (result["RegistrosInformaticos"] as List<AnexoRegistroRowDto>)!;
        inf.Count.ShouldBe(1);
        inf[0].EsInformatico.ShouldBeTrue();
    }

    [Test]
    public async Task GatherVariablesAsync_WithNorma_PopulatesNormaRow()
    {
        var institution = new Institution { Id = "inst-3", Nombre = "CITMA" };
        _db.Institutions.Add(institution);
        var tipoNorma = new TipoNorma { Nombre = "ISO" };
        _db.TiposNorma.Add(tipoNorma);
        await _db.SaveChangesAsync();

        _db.Normas.Add(new Norma
        {
            Titulo = "Norma ISO-001",
            TipoNormaId = tipoNorma.Id,
            InstitutionId = institution.Id
        });
        await _db.SaveChangesAsync();

        var result = await _sut.GatherVariablesAsync(null, default);

        var normas = (result["Normas"] as List<AnexoNormaRowDto>)!;
        normas.Count.ShouldBe(1);
        normas[0].Titulo.ShouldBe("Norma ISO-001");
        normas[0].Tipo.ShouldBe("ISO");
    }

    [Test]
    public async Task GatherVariablesAsync_WithProducto_PopulatesProductoTipoRow()
    {
        var institution = new Institution { Id = "inst-4", Nombre = "Empresa TI" };
        var tipo = new TipoProductoComercializado { Id = "tipo-1", Nombre = "Software" };
        _db.Institutions.Add(institution);
        _db.TipoProductosComercializados.Add(tipo);
        await _db.SaveChangesAsync();

        _db.ProductosComercializados.Add(new ProductoComercializado
        {
            Titulo = "App Móvil",
            TipoProductoComercializadoId = tipo.Id,
            InstitutionId = institution.Id
        });
        await _db.SaveChangesAsync();

        var result = await _sut.GatherVariablesAsync(null, default);

        var tipos = (result["ProductosTipos"] as List<AnexoProductoTipoRowDto>)!;
        tipos.Count.ShouldBe(1);
        tipos[0].TipoProductoComercializadoNombre.ShouldBe("Software");
        tipos[0].Productos.Count.ShouldBe(1);
        tipos[0].Productos[0].Titulo.ShouldBe("App Móvil");
    }

    [Test]
    public async Task GatherVariablesAsync_WithAreaFilter_IncludesPatenteWhenCreadorInUserArea()
    {
        _db.Users.Add(new User { Id = "req-rg1", AreaId = "area-a", Email = "r@t.cu", UserName = "req", UserLastName1 = "R" });
        _db.Users.Add(new User { Id = "creator-a1", AreaId = "area-a", Email = "ca@t.cu", UserName = "ca", UserLastName1 = "CA" });
        _db.Authors.Add(new Author { Id = "auth-pa1", LastName = "CA", Name = "CA", SearchKey = "ca", LastNameKey = "ca", UserId = "creator-a1" });
        _db.Patentes.Add(new Patente { Id = "pat-a1", Titulo = "Patente del Área", NumeroSolicitudConcesion = "P-001", EsNacional = true });
        await _db.SaveChangesAsync();

        _db.Add(new AuthorPatente { PatenteId = "pat-a1", AuthorId = "auth-pa1" });
        await _db.SaveChangesAsync();

        _userMock.Setup(u => u.Id).Returns("req-rg1");

        var result = await _sut.GatherVariablesAsync(null, default);
        var patentes = result["Patentes"] as List<AnexoRegistrosPatenteRowDto>;
        patentes.ShouldNotBeNull();
        patentes!.Count.ShouldBe(1);
        patentes[0].Titulo.ShouldBe("Patente del Área");
    }

    [Test]
    public async Task GatherVariablesAsync_WithAreaFilter_ExcludesPatenteFromOtherArea()
    {
        _db.Users.Add(new User { Id = "req-rg2", AreaId = "area-a", Email = "r@t.cu", UserName = "req", UserLastName1 = "R" });
        _db.Users.Add(new User { Id = "creator-b1", AreaId = "area-b", Email = "cb@t.cu", UserName = "cb", UserLastName1 = "CB" });
        _db.Authors.Add(new Author { Id = "auth-pb1", LastName = "CB", Name = "CB", SearchKey = "cb", LastNameKey = "cb", UserId = "creator-b1" });
        _db.Patentes.Add(new Patente { Id = "pat-b1", Titulo = "Patente Otra Área", NumeroSolicitudConcesion = "P-002", EsNacional = false });
        await _db.SaveChangesAsync();

        _db.Add(new AuthorPatente { PatenteId = "pat-b1", AuthorId = "auth-pb1" });
        await _db.SaveChangesAsync();

        _userMock.Setup(u => u.Id).Returns("req-rg2");

        var result = await _sut.GatherVariablesAsync(null, default);
        (result["Patentes"] as List<AnexoRegistrosPatenteRowDto>)!.ShouldBeEmpty();
    }

    [Test]
    public async Task GatherVariablesAsync_WithAreaFilter_IncludesRegistroWhenCreadorInUserArea()
    {
        var inst = new Institution { Id = "inst-r1", Nombre = "UH" };
        var country = new Country { Id = 10, Name = "Cuba" };
        _db.Institutions.Add(inst);
        _db.Countries.Add(country);
        _db.Users.Add(new User { Id = "req-rr1", AreaId = "area-a", Email = "r@t.cu", UserName = "req", UserLastName1 = "R" });
        _db.Users.Add(new User { Id = "creator-rr1", AreaId = "area-a", Email = "cr@t.cu", UserName = "cr", UserLastName1 = "CR" });
        _db.Authors.Add(new Author { Id = "auth-rr1", LastName = "CR", Name = "CR", SearchKey = "cr", LastNameKey = "cr", UserId = "creator-rr1" });
        _db.Registros.Add(new Registro { Id = "reg-r1", Titulo = "Registro del Área", NumeroCertificado = "R-001", EsInformatico = false, InstitutionId = inst.Id, CountryId = country.Id });
        await _db.SaveChangesAsync();

        _db.Add(new AuthorRegistro { RegistroId = "reg-r1", AuthorId = "auth-rr1" });
        await _db.SaveChangesAsync();

        _userMock.Setup(u => u.Id).Returns("req-rr1");

        var result = await _sut.GatherVariablesAsync(null, default);
        var registros = result["RegistrosNoInformaticos"] as List<AnexoRegistroRowDto>;
        registros.ShouldNotBeNull();
        registros!.Count.ShouldBe(1);
        registros[0].Titulo.ShouldBe("Registro del Área");
    }

    [Test]
    public async Task GatherVariablesAsync_WithAreaFilter_IncludesNormaWhenCreadorInUserArea()
    {
        var inst = new Institution { Id = "inst-n1", Nombre = "CITMA" };
        var tipoNorma = new TipoNorma { Nombre = "ISO" };
        _db.Institutions.Add(inst);
        _db.TiposNorma.Add(tipoNorma);
        _db.Users.Add(new User { Id = "req-rn1", AreaId = "area-a", Email = "r@t.cu", UserName = "req", UserLastName1 = "R" });
        _db.Users.Add(new User { Id = "creator-rn1", AreaId = "area-a", Email = "cn@t.cu", UserName = "cn", UserLastName1 = "CN" });
        _db.Authors.Add(new Author { Id = "auth-rn1", LastName = "CN", Name = "CN", SearchKey = "cn", LastNameKey = "cn", UserId = "creator-rn1" });
        _db.Normas.Add(new Norma { Id = "norma-r1", Titulo = "Norma del Área", TipoNormaId = tipoNorma.Id, InstitutionId = inst.Id });
        await _db.SaveChangesAsync();

        _db.Add(new AuthorNorma { NormaId = "norma-r1", AuthorId = "auth-rn1" });
        await _db.SaveChangesAsync();

        _userMock.Setup(u => u.Id).Returns("req-rn1");

        var result = await _sut.GatherVariablesAsync(null, default);
        var normas = result["Normas"] as List<AnexoNormaRowDto>;
        normas.ShouldNotBeNull();
        normas!.Count.ShouldBe(1);
        normas[0].Titulo.ShouldBe("Norma del Área");
    }

    [Test]
    public async Task GatherVariablesAsync_WithAreaFilter_IncludesProductoWhenCreadorInUserArea()
    {
        var inst = new Institution { Id = "inst-p1", Nombre = "UH" };
        var tipo = new TipoProductoComercializado { Id = "tipo-rp1", Nombre = "Software" };
        _db.Institutions.Add(inst);
        _db.TipoProductosComercializados.Add(tipo);
        _db.Users.Add(new User { Id = "req-rprod1", AreaId = "area-a", Email = "r@t.cu", UserName = "req", UserLastName1 = "R" });
        _db.Users.Add(new User { Id = "creator-rprod1", AreaId = "area-a", Email = "cp@t.cu", UserName = "cp", UserLastName1 = "CP" });
        _db.Authors.Add(new Author { Id = "auth-rprod1", LastName = "CP", Name = "CP", SearchKey = "cp", LastNameKey = "cp", UserId = "creator-rprod1" });
        _db.ProductosComercializados.Add(new ProductoComercializado { Id = "prod-r1", Titulo = "Producto del Área", TipoProductoComercializadoId = tipo.Id, InstitutionId = inst.Id });
        await _db.SaveChangesAsync();

        _db.Add(new AuthorProductoComercializado { ProductoComercializadoId = "prod-r1", AuthorId = "auth-rprod1" });
        await _db.SaveChangesAsync();

        _userMock.Setup(u => u.Id).Returns("req-rprod1");

        var result = await _sut.GatherVariablesAsync(null, default);
        var tipos = result["ProductosTipos"] as List<AnexoProductoTipoRowDto>;
        tipos.ShouldNotBeNull();
        tipos!.Count.ShouldBe(1);
        tipos[0].Productos.Count.ShouldBe(1);
        tipos[0].Productos[0].Titulo.ShouldBe("Producto del Área");
    }
}
