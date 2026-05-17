using Dashboard_v2.Application.Documents.Reports;
using Dashboard_v2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shouldly;

namespace Dashboard_v2.Application.UnitTests.Documents.Reports;

[TestFixture]
public class ProyectosReportTests
{
    private ApplicationDbContext _db = null!;
    private ProyectosReport _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(options);
        _sut = new ProyectosReport(_db);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    [Test]
    public void ReportName_ReturnsExpected()
    {
        _sut.ReportName.ShouldBe("anexo-proyectos");
    }

    [Test]
    public void TemplateName_ReturnsExpected()
    {
        _sut.TemplateName.ShouldBe("AnexoProyectos");
    }

    [Test]
    public async Task GatherVariablesAsync_Empty_ReturnsAllExpectedKeys()
    {
        var result = await _sut.GatherVariablesAsync(null, default);

        result.ShouldContainKey("PE");
        result.ShouldContainKey("PAPN");
        result.ShouldContainKey("PAPS");
        result.ShouldContainKey("PAPT");
        result.ShouldContainKey("PNE");
        result.ShouldContainKey("PDL");
        result.ShouldContainKey("PRCI");
        result.ShouldContainKey("PNAP");
        result.ShouldContainKey("NuevasAplicaciones");
    }

    [Test]
    public async Task GatherVariablesAsync_Empty_AllListsEmpty()
    {
        var result = await _sut.GatherVariablesAsync(null, default);

        foreach (var key in result.Keys)
        {
            var list = result[key] as System.Collections.IList;
            list.ShouldNotBeNull($"Key '{key}' is not a list");
            list.Count.ShouldBe(0, $"Key '{key}' should be empty");
        }
    }
}
