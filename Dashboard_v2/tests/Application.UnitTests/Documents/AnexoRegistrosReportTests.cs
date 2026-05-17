using Dashboard_v2.Application.Documents.Reports;
using Dashboard_v2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shouldly;

namespace Dashboard_v2.Application.UnitTests.Documents.Reports;

[TestFixture]
public class AnexoRegistrosReportTests
{
    private ApplicationDbContext _db = null!;
    private AnexoRegistrosReport _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(options);
        _sut = new AnexoRegistrosReport(_db);
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
}
