using Dashboard_v2.Application.Documents.Reports;
using Dashboard_v2.Domain.Entities;
using Dashboard_v2.Domain.Enums;
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

    private static readonly string AreaId = "area-1";
    private static readonly string ClasifId = "clasif-1";
    private static readonly string JefeId = "jefe-1";

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

    private async Task SeedBaseAsync()
    {
        _db.Areas.Add(new Area { Id = AreaId, Nombre = "MATCOM", Descripcion = "d", UniversidadId = "uh" });
        _db.Clasificaciones.Add(new Clasificacion { Id = ClasifId, Nombre = "Básica" });
        _db.Users.Add(new User
        {
            Id = JefeId,
            UserName = "juan",
            UserLastName1 = "Pérez",
            Email = "jefe@uh.cu",
            PasswordHash = "x",
            IsActive = true,
            BirthDate = DateTime.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            AreaId = AreaId
        });
        await _db.SaveChangesAsync();
    }

    private T BaseProject<T>(T p) where T : ProyectoEnEjecucion
    {
        p.Titulo = typeof(T).Name;
        p.JefeId = JefeId;
        p.ClasificacionId = ClasifId;
        p.AreaId = AreaId;
        p.NumeroMiembros = 3;
        p.CantidadMiembrosUH = 2;
        p.FechaInicio = DateOnly.FromDateTime(DateTime.Today);
        p.EstadoDeEjecucion = "En ejecución";
        p.CodigoProyecto = $"{typeof(T).Name}-001";
        p.EntidadEjecutoraPrincipal = "UH";
        return p;
    }

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

    [Test]
    public async Task GatherVariablesAsync_WithProyectoEmpresarial_PopulatesPERow()
    {
        await SeedBaseAsync();
        _db.Proyectos.Add(BaseProject(new ProyectoEmpresarial { Empresa = "Acme S.A." }));
        await _db.SaveChangesAsync();

        var result = await _sut.GatherVariablesAsync(null, default);

        var pe = (result["PE"] as System.Collections.IList)!;
        pe.Count.ShouldBe(1);
        var row = (ProyectoPERowDto)pe[0]!;
        row.Empresa.ShouldBe("Acme S.A.");
    }

    [Test]
    public async Task GatherVariablesAsync_WithProyectoPAP_Nacional_PopulatesPAPNRow()
    {
        await SeedBaseAsync();
        _db.Proyectos.Add(BaseProject(new ProyectoApoyoPrograma
        {
            NombrePrograma = "Prog Nacional",
            TipoPAP = TipoPAP.Nacional
        }));
        await _db.SaveChangesAsync();

        var result = await _sut.GatherVariablesAsync(null, default);

        var papn = (result["PAPN"] as System.Collections.IList)!;
        papn.Count.ShouldBe(1);
        var row = (ProyectoPAPRowDto)papn[0]!;
        row.NombrePrograma.ShouldBe("Prog Nacional");
    }

    [Test]
    public async Task GatherVariablesAsync_WithProyectoPAP_Sectorial_PopulatesPAPSRow()
    {
        await SeedBaseAsync();
        _db.Proyectos.Add(BaseProject(new ProyectoApoyoPrograma
        {
            NombrePrograma = "Prog Sectorial",
            TipoPAP = TipoPAP.Sectorial
        }));
        await _db.SaveChangesAsync();

        var result = await _sut.GatherVariablesAsync(null, default);

        var paps = (result["PAPS"] as System.Collections.IList)!;
        paps.Count.ShouldBe(1);
    }

    [Test]
    public async Task GatherVariablesAsync_WithProyectoPAP_Territorial_PopulatesPAPTRow()
    {
        await SeedBaseAsync();
        _db.Proyectos.Add(BaseProject(new ProyectoApoyoPrograma
        {
            NombrePrograma = "Prog Territorial",
            TipoPAP = TipoPAP.Territorial
        }));
        await _db.SaveChangesAsync();

        var result = await _sut.GatherVariablesAsync(null, default);

        var papt = (result["PAPT"] as System.Collections.IList)!;
        papt.Count.ShouldBe(1);
    }

    [Test]
    public async Task GatherVariablesAsync_WithProyectoNoEmpresarial_PopulatesPNERow()
    {
        await SeedBaseAsync();
        _db.Proyectos.Add(BaseProject(new ProyectoNoEmpresarial { EntidadNoEmpresarial = "Ministerio" }));
        await _db.SaveChangesAsync();

        var result = await _sut.GatherVariablesAsync(null, default);

        var pne = (result["PNE"] as System.Collections.IList)!;
        pne.Count.ShouldBe(1);
        var row = (ProyectoPNERowDto)pne[0]!;
        row.EntidadNoEmpresarial.ShouldBe("Ministerio");
    }

    [Test]
    public async Task GatherVariablesAsync_WithProyectoDesarrolloLocal_PopulatesPDLRow()
    {
        await SeedBaseAsync();
        _db.Proyectos.Add(BaseProject(new ProyectoDesarrolloLocal { Municipio = "Plaza" }));
        await _db.SaveChangesAsync();

        var result = await _sut.GatherVariablesAsync(null, default);

        var pdl = (result["PDL"] as System.Collections.IList)!;
        pdl.Count.ShouldBe(1);
        var row = (ProyectoPDLRowDto)pdl[0]!;
        row.Municipio.ShouldBe("Plaza");
    }

    [Test]
    public async Task GatherVariablesAsync_WithProyectoColabInternacional_PopulatesPRCIRow()
    {
        await SeedBaseAsync();
        _db.Proyectos.Add(BaseProject(new ProyectoColabInternacional
        {
            FuenteFinanciacion = "EU",
            TerminosReferencia = "TDR"
        }));
        await _db.SaveChangesAsync();

        var result = await _sut.GatherVariablesAsync(null, default);

        var prci = (result["PRCI"] as System.Collections.IList)!;
        prci.Count.ShouldBe(1);
        var row = (ProyectoPRCIRowDto)prci[0]!;
        row.FuenteFinanciacion.ShouldBe("EU");
    }

    [Test]
    public async Task GatherVariablesAsync_WithProyectoPNAP_PopulatesPNAPRow()
    {
        await SeedBaseAsync();
        _db.Proyectos.Add(BaseProject(new ProyectoPNAP { FinanciamientoUH = "1M CUP" }));
        await _db.SaveChangesAsync();

        var result = await _sut.GatherVariablesAsync(null, default);

        var pnap = (result["PNAP"] as System.Collections.IList)!;
        pnap.Count.ShouldBe(1);
        var row = (ProyectoPNAPRowDto)pnap[0]!;
        row.FinanciamientoUH.ShouldBe("1M CUP");
    }

    [Test]
    public async Task GatherVariablesAsync_WithProyectoEnRevision_PopulatesNuevasAplicacionesRow()
    {
        await SeedBaseAsync();
        _db.Proyectos.Add(new ProyectoEnRevision
        {
            Titulo = "En Revisión Test",
            JefeId = JefeId,
            ClasificacionId = ClasifId,
            AreaId = AreaId,
            NumeroMiembros = 2,
            CantidadMiembrosUH = 1,
            Situacion = "Pendiente",
            Tipo = "PE"
        });
        await _db.SaveChangesAsync();

        var result = await _sut.GatherVariablesAsync(null, default);

        var nuevas = (result["NuevasAplicaciones"] as System.Collections.IList)!;
        nuevas.Count.ShouldBe(1);
        var row = (ProyectoNuevasAplicacionesRowDto)nuevas[0]!;
        row.TituloProyecto.ShouldBe("En Revisión Test");
    }
}
