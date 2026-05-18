using Dashboard_v2.Application.LineasDeInvestigacion;
using Dashboard_v2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shouldly;

namespace Dashboard_v2.Application.UnitTests.LineasDeInvestigacion;

[TestFixture]
public class LineaDeInvestigacionServiceTests
{
    private ApplicationDbContext _db = null!;
    private LineaDeInvestigacionService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(opts);
        _sut = new LineaDeInvestigacionService(_db);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    // ── GetAllAsync ──────────────────────────────────────────────────────────

    [Test]
    public async Task GetAllAsync_EmptyDb_ReturnsEmptyList()
    {
        var result = await _sut.GetAllAsync();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GetAllAsync_WithItems_ReturnsOrderedByNombre()
    {
        _db.LineasDeInvestigacion.AddRange(
            new Domain.Entities.LineaDeInvestigacion { Id = Guid.NewGuid().ToString(), Nombre = "Zeta" },
            new Domain.Entities.LineaDeInvestigacion { Id = Guid.NewGuid().ToString(), Nombre = "Alpha" });
        await _db.SaveChangesAsync();

        var result = await _sut.GetAllAsync();

        result.Count.ShouldBe(2);
        result[0].Nombre.ShouldBe("Alpha");
        result[1].Nombre.ShouldBe("Zeta");
    }

    // ── CreateAsync ──────────────────────────────────────────────────────────

    [Test]
    public async Task CreateAsync_ValidRequest_ReturnsSuccessAndId()
    {
        var (result, id) = await _sut.CreateAsync(
            new CreateLineaDeInvestigacionRequest { Nombre = "Machine Learning" });

        result.Succeeded.ShouldBeTrue();
        id.ShouldNotBeNullOrEmpty();
        (await _db.LineasDeInvestigacion.CountAsync()).ShouldBe(1);
    }

    [Test]
    public async Task CreateAsync_EmptyNombre_ReturnsFailure()
    {
        var (result, id) = await _sut.CreateAsync(
            new CreateLineaDeInvestigacionRequest { Nombre = "  " });

        result.Succeeded.ShouldBeFalse();
        id.ShouldBeNull();
    }

    [Test]
    public async Task CreateAsync_TrimsNombre()
    {
        var (_, id) = await _sut.CreateAsync(
            new CreateLineaDeInvestigacionRequest { Nombre = "  Sistemas  " });

        var entity = await _db.LineasDeInvestigacion.FindAsync(id);
        entity!.Nombre.ShouldBe("Sistemas");
    }

    // ── UpdateAsync ──────────────────────────────────────────────────────────

    [Test]
    public async Task UpdateAsync_ValidRequest_UpdatesNombre()
    {
        var (_, id) = await _sut.CreateAsync(
            new CreateLineaDeInvestigacionRequest { Nombre = "Old" });

        var result = await _sut.UpdateAsync(id!, new UpdateLineaDeInvestigacionRequest { Nombre = "New" });

        result.Succeeded.ShouldBeTrue();
        var entity = await _db.LineasDeInvestigacion.FindAsync(id);
        entity!.Nombre.ShouldBe("New");
    }

    [Test]
    public async Task UpdateAsync_EmptyNombre_ReturnsFailure()
    {
        var result = await _sut.UpdateAsync("l1", new UpdateLineaDeInvestigacionRequest { Nombre = "" });
        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task UpdateAsync_NonExistentId_ReturnsFailure()
    {
        var result = await _sut.UpdateAsync("no-existe", new UpdateLineaDeInvestigacionRequest { Nombre = "Test" });
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("no encontrada"));
    }

    // ── DeleteAsync ──────────────────────────────────────────────────────────

    [Test]
    public async Task DeleteAsync_ExistingId_Removes()
    {
        var (_, id) = await _sut.CreateAsync(
            new CreateLineaDeInvestigacionRequest { Nombre = "Test" });

        var result = await _sut.DeleteAsync(id!);

        result.Succeeded.ShouldBeTrue();
        (await _db.LineasDeInvestigacion.CountAsync()).ShouldBe(0);
    }

    [Test]
    public async Task DeleteAsync_NonExistentId_ReturnsFailure()
    {
        var result = await _sut.DeleteAsync("no-existe");
        result.Succeeded.ShouldBeFalse();
    }
}
