using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.GruposEstudiantiles;
using Dashboard_v2.Domain.Entities;
using Dashboard_v2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Dashboard_v2.Application.UnitTests.GruposEstudiantiles;

[TestFixture]
public class GrupoEstudiantilServiceTests
{
    private ApplicationDbContext _db = null!;
    private Mock<IUser> _userMock = null!;
    private GrupoEstudiantilService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(opts);
        _userMock = new Mock<IUser>();
        _sut = new GrupoEstudiantilService(_db, _userMock.Object);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    private async Task AddAreaAsync(string id = "area-1")
    {
        _db.Areas.Add(new Area { Id = id, Nombre = "Ciencias" });
        await _db.SaveChangesAsync();
    }

    // ── GetAllAsync ──────────────────────────────────────────────────────────

    [Test]
    public async Task GetAllAsync_EmptyDb_ReturnsEmptyList()
    {
        var result = await _sut.GetAllAsync();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GetAllAsync_WithData_ReturnsGrupoEstudiantilDtos()
    {
        await AddAreaAsync();
        _db.GruposEstudiantiles.Add(new GrupoEstudiantil { Id = "g-1", Nombre = "Grupo Alpha", AreaId = "area-1" });
        await _db.SaveChangesAsync();

        var result = await _sut.GetAllAsync();

        result.Count.ShouldBe(1);
        result[0].Nombre.ShouldBe("Grupo Alpha");
        result[0].AreaId.ShouldBe("area-1");
    }

    // ── CreateAsync ──────────────────────────────────────────────────────────

    [Test]
    public async Task CreateAsync_ValidRequest_ReturnsSuccessAndId()
    {
        await AddAreaAsync();

        var (result, id) = await _sut.CreateAsync(
            new CreateGrupoEstudiantilRequest { Nombre = "Grupo B", AreaId = "area-1" });

        result.Succeeded.ShouldBeTrue();
        id.ShouldNotBeNullOrEmpty();
    }

    [Test]
    public async Task CreateAsync_EmptyNombre_ReturnsFailure()
    {
        await AddAreaAsync();
        var (result, _) = await _sut.CreateAsync(
            new CreateGrupoEstudiantilRequest { Nombre = "  ", AreaId = "area-1" });
        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task CreateAsync_NonExistentArea_ReturnsFailure()
    {
        var (result, _) = await _sut.CreateAsync(
            new CreateGrupoEstudiantilRequest { Nombre = "Grupo B", AreaId = "no-existe" });
        result.Succeeded.ShouldBeFalse();
    }

    // ── UpdateAsync ──────────────────────────────────────────────────────────

    [Test]
    public async Task UpdateAsync_EmptyNombre_ReturnsFailure()
    {
        var result = await _sut.UpdateAsync("g1",
            new UpdateGrupoEstudiantilRequest { Nombre = "", AreaId = "area-1" });
        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task UpdateAsync_NonExistentGrupo_ReturnsFailure()
    {
        _userMock.Setup(u => u.Roles).Returns(["Superuser"]);
        var result = await _sut.UpdateAsync("no-existe",
            new UpdateGrupoEstudiantilRequest { Nombre = "Test", AreaId = "area-1" });
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("no encontrado"));
    }

    [Test]
    public async Task UpdateAsync_WithoutSuperuserRole_ReturnsFailure()
    {
        await AddAreaAsync();
        var (_, id) = await _sut.CreateAsync(
            new CreateGrupoEstudiantilRequest { Nombre = "Grupo B", AreaId = "area-1" });

        _userMock.Setup(u => u.Roles).Returns(["Estudiante"]);

        var result = await _sut.UpdateAsync(id!,
            new UpdateGrupoEstudiantilRequest { Nombre = "Modified", AreaId = "area-1" });

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("permisos"));
    }

    [Test]
    public async Task UpdateAsync_Superuser_UpdatesGrupo()
    {
        await AddAreaAsync();
        var (_, id) = await _sut.CreateAsync(
            new CreateGrupoEstudiantilRequest { Nombre = "Grupo B", AreaId = "area-1" });

        _userMock.Setup(u => u.Roles).Returns(["Superuser"]);

        var result = await _sut.UpdateAsync(id!,
            new UpdateGrupoEstudiantilRequest { Nombre = "Modified", AreaId = "area-1" });

        result.Succeeded.ShouldBeTrue();
        var grupo = await _db.GruposEstudiantiles.FindAsync(id);
        grupo!.Nombre.ShouldBe("Modified");
    }

    // ── DeleteAsync ──────────────────────────────────────────────────────────

    [Test]
    public async Task DeleteAsync_NonExistentGrupo_ReturnsFailure()
    {
        _userMock.Setup(u => u.Roles).Returns(["Superuser"]);
        var result = await _sut.DeleteAsync("no-existe");
        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task DeleteAsync_WithoutPermissions_ReturnsFailure()
    {
        await AddAreaAsync();
        var (_, id) = await _sut.CreateAsync(
            new CreateGrupoEstudiantilRequest { Nombre = "Grupo B", AreaId = "area-1" });

        _userMock.Setup(u => u.Roles).Returns(["Estudiante"]);

        var result = await _sut.DeleteAsync(id!);
        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task DeleteAsync_Superuser_Removes()
    {
        await AddAreaAsync();
        var (_, id) = await _sut.CreateAsync(
            new CreateGrupoEstudiantilRequest { Nombre = "Grupo B", AreaId = "area-1" });

        _userMock.Setup(u => u.Roles).Returns(["Superuser"]);

        var result = await _sut.DeleteAsync(id!);

        result.Succeeded.ShouldBeTrue();
        (await _db.GruposEstudiantiles.CountAsync()).ShouldBe(0);
    }
}
