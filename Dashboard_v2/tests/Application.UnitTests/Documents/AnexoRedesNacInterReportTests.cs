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
public class AnexoRedesNacInterReportTests
{
    private ApplicationDbContext _db = null!;
    private Mock<IUser> _userMock = null!;
    private AnexoRedesNacInterReport _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(options);
        _userMock = new Mock<IUser>();
        _userMock.Setup(u => u.Id).Returns((string?)null);
        _sut = new AnexoRedesNacInterReport(_db, _userMock.Object);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    [Test]
    public void ReportName_ReturnsExpected()
    {
        _sut.ReportName.ShouldBe("anexo-redes-nac-inter");
    }

    [Test]
    public void TemplateName_ReturnsExpected()
    {
        _sut.TemplateName.ShouldBe("AnexoRedesNacInter");
    }

    [Test]
    public async Task GatherVariablesAsync_Empty_ReturnsBothKeys()
    {
        var result = await _sut.GatherVariablesAsync(null, default);
        result.ShouldContainKey("RedesNacionales");
        result.ShouldContainKey("RedesInternacionales");
        (result["RedesNacionales"] as List<AnexoRedNacionalRowDto>)!.ShouldBeEmpty();
        (result["RedesInternacionales"] as List<AnexoRedInternacionalRowDto>)!.ShouldBeEmpty();
    }

    [Test]
    public async Task GatherVariablesAsync_WithNacionalRed_AppearsInNacionales()
    {
        _db.Reds.Add(new Red { Nombre = "Red Nacional A", Tipo = TipoRed.Nacional, CantidadProfesores = 5 });
        await _db.SaveChangesAsync();

        var result = await _sut.GatherVariablesAsync(null, default);
        var nac = result["RedesNacionales"] as List<AnexoRedNacionalRowDto>;
        nac.ShouldHaveSingleItem();
        nac![0].Nombre.ShouldBe("Red Nacional A");
        nac[0].CantidadProfesores.ShouldBe(5);
    }

    [Test]
    public async Task GatherVariablesAsync_WithInternacionalRed_AppearsInInternacionales()
    {
        _db.Countries.Add(new Country { Id = 1, Name = "Cuba" });
        _db.Reds.Add(new Red { Nombre = "Red Inter A", Tipo = TipoRed.Internacional, CountryId = 1, CantidadProfesores = 3 });
        await _db.SaveChangesAsync();

        var result = await _sut.GatherVariablesAsync(null, default);
        var inter = result["RedesInternacionales"] as List<AnexoRedInternacionalRowDto>;
        inter.ShouldHaveSingleItem();
        inter![0].Nombre.ShouldBe("Red Inter A");
        inter[0].Pais.ShouldBe("Cuba");
    }

    [Test]
    public async Task GatherVariablesAsync_UniversitariaRed_IsFiltered()
    {
        _db.Reds.Add(new Red { Nombre = "Red Univ", Tipo = TipoRed.Universitaria });
        await _db.SaveChangesAsync();

        var result = await _sut.GatherVariablesAsync(null, default);
        (result["RedesNacionales"] as List<AnexoRedNacionalRowDto>)!.ShouldBeEmpty();
        (result["RedesInternacionales"] as List<AnexoRedInternacionalRowDto>)!.ShouldBeEmpty();
    }

    [Test]
    public async Task GatherVariablesAsync_WithAreaFilter_IncludesRedWhenCoordinadorInUserArea()
    {
        _db.Users.Add(new User { Id = "req-rc1", AreaId = "area-a", Email = "r@t.cu", UserName = "req", UserLastName1 = "R" });
        _db.Users.Add(new User { Id = "coord-a1", AreaId = "area-a", Email = "c@t.cu", UserName = "coord", UserLastName1 = "C" });
        _db.Reds.Add(new Red { Id = "red-r1", Nombre = "Red Coordinada", Tipo = TipoRed.Nacional, CoordinadorId = "coord-a1" });
        await _db.SaveChangesAsync();

        _userMock.Setup(u => u.Id).Returns("req-rc1");

        var result = await _sut.GatherVariablesAsync(null, default);
        var nac = result["RedesNacionales"] as List<AnexoRedNacionalRowDto>;
        nac.ShouldNotBeNull();
        nac!.Count.ShouldBe(1);
        nac[0].Nombre.ShouldBe("Red Coordinada");
    }

    [Test]
    public async Task GatherVariablesAsync_WithAreaFilter_IncludesRedWhenParticipantInUserArea()
    {
        _db.Users.Add(new User { Id = "req-rp1", AreaId = "area-a", Email = "r@t.cu", UserName = "req", UserLastName1 = "R" });
        _db.Users.Add(new User { Id = "member-rp1", AreaId = "area-a", Email = "m@t.cu", UserName = "mem", UserLastName1 = "M" });
        _db.Authors.Add(new Author { Id = "auth-rp1", LastName = "M", Name = "M", SearchKey = "m", LastNameKey = "m", UserId = "member-rp1" });
        _db.Reds.Add(new Red { Id = "red-rp1", Nombre = "Red con Participante", Tipo = TipoRed.Nacional });
        await _db.SaveChangesAsync();

        _db.Add(new ParticipacionEnRed { RedId = "red-rp1", AuthorId = "auth-rp1" });
        await _db.SaveChangesAsync();

        _userMock.Setup(u => u.Id).Returns("req-rp1");

        var result = await _sut.GatherVariablesAsync(null, default);
        var nac = result["RedesNacionales"] as List<AnexoRedNacionalRowDto>;
        nac.ShouldNotBeNull();
        nac!.Count.ShouldBe(1);
        nac[0].Nombre.ShouldBe("Red con Participante");
    }

    [Test]
    public async Task GatherVariablesAsync_WithAreaFilter_ExcludesRedWithNoAreaMatch()
    {
        _db.Users.Add(new User { Id = "req-re1", AreaId = "area-a", Email = "r@t.cu", UserName = "req", UserLastName1 = "R" });
        _db.Users.Add(new User { Id = "coord-b1", AreaId = "area-b", Email = "cb@t.cu", UserName = "cb", UserLastName1 = "CB" });
        _db.Reds.Add(new Red { Id = "red-re1", Nombre = "Red Excluida", Tipo = TipoRed.Nacional, CoordinadorId = "coord-b1" });
        await _db.SaveChangesAsync();

        _userMock.Setup(u => u.Id).Returns("req-re1");

        var result = await _sut.GatherVariablesAsync(null, default);
        (result["RedesNacionales"] as List<AnexoRedNacionalRowDto>)!.ShouldBeEmpty();
    }
}
