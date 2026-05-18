using Dashboard_v2.Application.AreasDelConocimiento;
using Dashboard_v2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shouldly;

namespace Dashboard_v2.Application.UnitTests.AreasDelConocimiento;

[TestFixture]
public class AreaDelConocimientoServiceTests
{
    private ApplicationDbContext _db = null!;
    private AreaDelConocimientoService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(opts);
        _sut = new AreaDelConocimientoService(_db);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    [Test]
    public async Task GetAllAsync_EmptyDb_ReturnsEmptyList()
    {
        var result = await _sut.GetAllAsync();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GetAllAsync_WithItems_ReturnsOrderedByNombre()
    {
        _db.AreasDelConocimiento.AddRange(
            new Domain.Entities.AreaDelConocimiento { Id = "a1", Nombre = "Zeta" },
            new Domain.Entities.AreaDelConocimiento { Id = "a2", Nombre = "Alpha" });
        await _db.SaveChangesAsync();

        var result = await _sut.GetAllAsync();

        result[0].Nombre.ShouldBe("Alpha");
    }

    [Test]
    public async Task CreateAsync_ValidRequest_ReturnsSuccessAndId()
    {
        var (result, id) = await _sut.CreateAsync(new CreateAreaDelConocimientoRequest { Nombre = "Matemáticas" });

        result.Succeeded.ShouldBeTrue();
        id.ShouldNotBeNullOrEmpty();
    }

    [Test]
    public async Task CreateAsync_EmptyNombre_ReturnsFailure()
    {
        var (result, _) = await _sut.CreateAsync(new CreateAreaDelConocimientoRequest { Nombre = "  " });
        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task UpdateAsync_ExistingId_UpdatesNombre()
    {
        var (_, id) = await _sut.CreateAsync(new CreateAreaDelConocimientoRequest { Nombre = "Old" });

        var result = await _sut.UpdateAsync(id!, new UpdateAreaDelConocimientoRequest { Nombre = "New" });

        result.Succeeded.ShouldBeTrue();
        var entity = await _db.AreasDelConocimiento.FindAsync(id);
        entity!.Nombre.ShouldBe("New");
    }

    [Test]
    public async Task UpdateAsync_EmptyNombre_ReturnsFailure()
    {
        var result = await _sut.UpdateAsync("a1", new UpdateAreaDelConocimientoRequest { Nombre = "" });
        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task UpdateAsync_NonExistentId_ReturnsFailure()
    {
        var result = await _sut.UpdateAsync("no-existe", new UpdateAreaDelConocimientoRequest { Nombre = "Test" });
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("no encontrada"));
    }

    [Test]
    public async Task DeleteAsync_ExistingId_Removes()
    {
        var (_, id) = await _sut.CreateAsync(new CreateAreaDelConocimientoRequest { Nombre = "Test" });

        var result = await _sut.DeleteAsync(id!);

        result.Succeeded.ShouldBeTrue();
        (await _db.AreasDelConocimiento.CountAsync()).ShouldBe(0);
    }

    [Test]
    public async Task DeleteAsync_NonExistentId_ReturnsFailure()
    {
        var result = await _sut.DeleteAsync("no-existe");
        result.Succeeded.ShouldBeFalse();
    }
}
