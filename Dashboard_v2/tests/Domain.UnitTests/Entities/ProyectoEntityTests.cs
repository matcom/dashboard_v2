using Dashboard_v2.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace Dashboard_v2.Domain.UnitTests.Entities;

/// <summary>
/// Cubre las propiedades de la jerarquía Proyecto (base abstracta)
/// usando ProyectoEnRevision como tipo concreto instanciable.
/// </summary>
[TestFixture]
public class ProyectoEntityTests
{
    // ── Defaults ──────────────────────────────────────────────────────────────

    [Test]
    public void ProyectoEnRevision_DefaultId_IsValidGuid()
    {
        var proy = new ProyectoEnRevision { Titulo = "Proyecto Test", JefeId = "j-1", ClasificacionId = "c-1", AreaId = "a-1", Tipo = "Tesis" };

        Guid.TryParse(proy.Id, out _).ShouldBeTrue();
    }

    [Test]
    public void ProyectoEnRevision_EachInstance_GetsUniqueId()
    {
        var p1 = new ProyectoEnRevision { Titulo = "A", JefeId = "j", ClasificacionId = "c", AreaId = "a", Tipo = "PE" };
        var p2 = new ProyectoEnRevision { Titulo = "B", JefeId = "j", ClasificacionId = "c", AreaId = "a", Tipo = "PE" };

        p1.Id.ShouldNotBe(p2.Id);
    }

    [Test]
    public void ProyectoEnRevision_DefaultCounters_AreZero()
    {
        var proy = new ProyectoEnRevision { Titulo = "T", JefeId = "j", ClasificacionId = "c", AreaId = "a", Tipo = "Tesis" };

        proy.NumeroMiembros.ShouldBe(0);
        proy.CantidadMiembrosUH.ShouldBe(0);
        proy.CantidadEstudiantes.ShouldBe(0);
        proy.CantidadEstudiantesContratados.ShouldBe(0);
    }

    [Test]
    public void ProyectoEnRevision_DefaultTributaFormacionDoctoral_IsFalse()
    {
        var proy = new ProyectoEnRevision { Titulo = "T", JefeId = "j", ClasificacionId = "c", AreaId = "a", Tipo = "PE" };

        proy.TributaFormacionDoctoral.ShouldBeFalse();
    }

    [Test]
    public void ProyectoEnRevision_DefaultCollections_AreEmptyNotNull()
    {
        var proy = new ProyectoEnRevision { Titulo = "T", JefeId = "j", ClasificacionId = "c", AreaId = "a", Tipo = "PE" };

        proy.PublicacionesDerivadas.ShouldNotBeNull();
        proy.PublicacionesDerivadas.ShouldBeEmpty();

        proy.PatentesDerivadas.ShouldNotBeNull();
        proy.PatentesDerivadas.ShouldBeEmpty();

        proy.Situaciones.ShouldNotBeNull();
        proy.Situaciones.ShouldBeEmpty();
    }

    // ── Asignación de propiedades base ────────────────────────────────────────

    [Test]
    public void ProyectoEnRevision_CanAssignAllBaseProperties()
    {
        var proy = new ProyectoEnRevision
        {
            Id = "proy-fixed",
            Titulo = "Proyecto de Investigación en IA",
            JefeId = "jefe-1",
            NumeroMiembros = 10,
            CantidadMiembrosUH = 7,
            CantidadEstudiantes = 2,
            CantidadEstudiantesContratados = 1,
            TributaFormacionDoctoral = true,
            ClasificacionId = "clasif-1",
            AreaId = "area-1",
            Tipo = "Tesis",
        };

        proy.Id.ShouldBe("proy-fixed");
        proy.Titulo.ShouldBe("Proyecto de Investigación en IA");
        proy.JefeId.ShouldBe("jefe-1");
        proy.NumeroMiembros.ShouldBe(10);
        proy.CantidadMiembrosUH.ShouldBe(7);
        proy.CantidadEstudiantes.ShouldBe(2);
        proy.CantidadEstudiantesContratados.ShouldBe(1);
        proy.TributaFormacionDoctoral.ShouldBeTrue();
        proy.ClasificacionId.ShouldBe("clasif-1");
        proy.AreaId.ShouldBe("area-1");
        proy.Tipo.ShouldBe("Tesis");
    }

    [Test]
    public void ProyectoEnRevision_TributaFormacionDoctoral_CanBeToggled()
    {
        var proy = new ProyectoEnRevision
        {
            Titulo = "T",
            JefeId = "j",
            ClasificacionId = "c",
            AreaId = "a",
            Tipo = "PE",
            TributaFormacionDoctoral = false
        };

        proy.TributaFormacionDoctoral.ShouldBeFalse();

        proy.TributaFormacionDoctoral = true;
        proy.TributaFormacionDoctoral.ShouldBeTrue();
    }
}
