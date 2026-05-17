using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.GruposDeInvestigacion;
using Dashboard_v2.Domain.Entities;
using Dashboard_v2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Dashboard_v2.Application.UnitTests.GruposDeInvestigacion;

[TestFixture]
public class GrupoDeInvestigacionServiceTests
{
    private ApplicationDbContext _db = null!;
    private Mock<IUser> _userMock = null!;
    private GrupoDeInvestigacionService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(opts);
        _userMock = new Mock<IUser>();
        _sut = new GrupoDeInvestigacionService(_db, _userMock.Object);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    private async Task<Area> AddAreaAsync(string id = "area-1", string nombre = "Ciencias")
    {
        var area = new Area { Id = id, Nombre = nombre };
        _db.Areas.Add(area);
        await _db.SaveChangesAsync();
        return area;
    }

    // ── GetAllAsync ──────────────────────────────────────────────────────────

    [Test]
    public async Task GetAllAsync_EmptyDb_ReturnsEmptyList()
    {
        var result = await _sut.GetAllAsync();
        result.ShouldBeEmpty();
    }

    // ── CreateAsync ──────────────────────────────────────────────────────────

    [Test]
    public async Task CreateAsync_ValidRequest_ReturnsSuccessAndId()
    {
        await AddAreaAsync();
        _userMock.Setup(u => u.Id).Returns("creator-1");

        var (result, id) = await _sut.CreateAsync(
            new CreateGrupoDeInvestigacionRequest { Nombre = "Grupo A", AreaId = "area-1" });

        result.Succeeded.ShouldBeTrue();
        id.ShouldNotBeNullOrEmpty();
        var grupo = await _db.GruposDeInvestigacion.FindAsync(id);
        grupo!.CreadorId.ShouldBe("creator-1");
    }

    [Test]
    public async Task CreateAsync_EmptyNombre_ReturnsFailure()
    {
        await AddAreaAsync();
        var (result, id) = await _sut.CreateAsync(
            new CreateGrupoDeInvestigacionRequest { Nombre = " ", AreaId = "area-1" });

        result.Succeeded.ShouldBeFalse();
        id.ShouldBeNull();
    }

    [Test]
    public async Task CreateAsync_NonExistentArea_ReturnsFailure()
    {
        var (result, id) = await _sut.CreateAsync(
            new CreateGrupoDeInvestigacionRequest { Nombre = "Grupo A", AreaId = "no-existe" });

        result.Succeeded.ShouldBeFalse();
    }

    // ── UpdateAsync ──────────────────────────────────────────────────────────

    [Test]
    public async Task UpdateAsync_EmptyNombre_ReturnsFailure()
    {
        var result = await _sut.UpdateAsync("g1",
            new UpdateGrupoDeInvestigacionRequest { Nombre = "", AreaId = "area-1" });
        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task UpdateAsync_NonExistentGrupo_ReturnsFailure()
    {
        _userMock.Setup(u => u.Roles).Returns(["Superuser"]);
        var result = await _sut.UpdateAsync("no-existe",
            new UpdateGrupoDeInvestigacionRequest { Nombre = "Test", AreaId = "area-1" });
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("no encontrado"));
    }

    [Test]
    public async Task UpdateAsync_WithoutPermissions_ReturnsFailure()
    {
        await AddAreaAsync();
        var (_, id) = await _sut.CreateAsync(
            new CreateGrupoDeInvestigacionRequest { Nombre = "Grupo A", AreaId = "area-1" });

        _userMock.Setup(u => u.Roles).Returns(["Investigador"]);

        var result = await _sut.UpdateAsync(id!,
            new UpdateGrupoDeInvestigacionRequest { Nombre = "Modified", AreaId = "area-1" });

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("permisos"));
    }

    [Test]
    public async Task UpdateAsync_Superuser_UpdatesGrupo()
    {
        await AddAreaAsync();
        _userMock.Setup(u => u.Id).Returns("creator-1");
        var (_, id) = await _sut.CreateAsync(
            new CreateGrupoDeInvestigacionRequest { Nombre = "Grupo A", AreaId = "area-1" });

        _userMock.Setup(u => u.Roles).Returns(["Superuser"]);

        var result = await _sut.UpdateAsync(id!,
            new UpdateGrupoDeInvestigacionRequest { Nombre = "Modified", AreaId = "area-1" });

        result.Succeeded.ShouldBeTrue();
        var grupo = await _db.GruposDeInvestigacion.FindAsync(id);
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
        _userMock.Setup(u => u.Id).Returns("creator-1");
        var (_, id) = await _sut.CreateAsync(
            new CreateGrupoDeInvestigacionRequest { Nombre = "Grupo A", AreaId = "area-1" });

        _userMock.Setup(u => u.Roles).Returns(["Investigador"]);

        var result = await _sut.DeleteAsync(id!);
        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task DeleteAsync_Superuser_Removes()
    {
        await AddAreaAsync();
        _userMock.Setup(u => u.Id).Returns("creator-1");
        var (_, id) = await _sut.CreateAsync(
            new CreateGrupoDeInvestigacionRequest { Nombre = "Grupo A", AreaId = "area-1" });

        _userMock.Setup(u => u.Roles).Returns(["Superuser"]);

        var result = await _sut.DeleteAsync(id!);

        result.Succeeded.ShouldBeTrue();
        (await _db.GruposDeInvestigacion.CountAsync()).ShouldBe(0);
    }

    // ── SetMiembrosAsync ─────────────────────────────────────────────────────

    [Test]
    public async Task SetMiembrosAsync_NonExistentGrupo_ReturnsFailure()
    {
        _userMock.Setup(u => u.Roles).Returns(["Superuser"]);
        var result = await _sut.SetMiembrosAsync("no-existe", new SetGrupoMiembrosRequest { UsuariosIds = [] });
        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task SetMiembrosAsync_Superuser_SetsMembers()
    {
        await AddAreaAsync();
        _userMock.Setup(u => u.Id).Returns("creator-1");
        var (_, id) = await _sut.CreateAsync(
            new CreateGrupoDeInvestigacionRequest { Nombre = "Grupo A", AreaId = "area-1" });

        _db.Users.Add(new User
        {
            Id = "user-1", UserName = "alice", Email = "a@a.com",
            UserLastName1 = "A"
        });
        await _db.SaveChangesAsync();

        _userMock.Setup(u => u.Roles).Returns(["Superuser"]);

        var result = await _sut.SetMiembrosAsync(id!, new SetGrupoMiembrosRequest { UsuariosIds = ["user-1"] });

        result.Succeeded.ShouldBeTrue();
        var grupo = await _db.GruposDeInvestigacion
            .Include(g => g.Usuarios)
            .FirstAsync(g => g.Id == id);
        grupo.Usuarios.Count.ShouldBe(1);
    }
}
