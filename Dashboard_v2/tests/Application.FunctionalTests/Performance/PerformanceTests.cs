using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Application.FunctionalTests.Performance;

using static Testing;

/// <summary>
/// Tests de rendimiento: verifican que los endpoints responden dentro de
/// umbrales de tiempo aceptables bajo carga de datos realista.
/// </summary>
[TestFixture]
public class PerformanceTests : BaseTestFixture
{
    private const int ThresholdMs = 5000; // 5 segundos — umbral generoso para entorno CI/Testcontainers

    [Test]
    public async Task GetPublications_20Records_RespondsWithinThreshold()
    {
        await RunAsUserAsync("perf.pub@local", "Testing1234!", ["Profesor"]);
        using var client = CreateClient();

        // Sembrar 20 publicaciones via API (antes de iniciar el cronómetro)
        for (int i = 1; i <= 20; i++)
        {
            var r = await client.PostAsJsonAsync("/api/Publications", new
            {
                title = $"Publicación de rendimiento #{i:D2}",
                publicationData = $"Revista de Ciencias, Vol. {i}",
                publicationType = i % 5, // tipos variados
                publishedDate = $"202{i % 5 + 1}",
                additionalAuthorIds = Array.Empty<string>(),
                additionalAuthorNames = Array.Empty<string>(),
                additionalUserIds = Array.Empty<string>()
            });
            r.IsSuccessStatusCode.ShouldBeTrue();
        }

        var sw = Stopwatch.StartNew();
        var response = await client.GetAsync("/api/Publications");
        sw.Stop();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetArrayLength().ShouldBe(20);

        sw.ElapsedMilliseconds.ShouldBeLessThan(ThresholdMs,
            $"GET /api/Publications con 20 registros tardó {sw.ElapsedMilliseconds}ms (umbral: {ThresholdMs}ms)");
    }

    [Test]
    public async Task GetProyectosCatalog_RespondsWithinThreshold()
    {
        await RunAsUserAsync("perf.proy@local", "Testing1234!", ["Jefe_de_Proyecto"]);
        using var client = CreateClient();

        var (areaId, clasifId, jefeId) = await SeedBaseForPerformanceAsync();

        // Sembrar 5 proyectos de distintos tipos
        for (int i = 1; i <= 5; i++)
        {
            await client.PostAsJsonAsync("/api/Proyectos/en-revision", new
            {
                titulo = $"Proyecto en revisión #{i}",
                jefeId,
                numeroMiembros = 3,
                cantidadMiembrosUH = 2,
                cantidadEstudiantes = 0,
                cantidadEstudiantesContratados = 0,
                tributaFormacionDoctoral = false,
                clasificacionId = clasifId,
                areaId,
                situacionesIds = Array.Empty<int>(),
                tipo = "Tesis",
            });
        }

        var sw = Stopwatch.StartNew();
        var response = await client.GetAsync("/api/Proyectos/catalogo");
        sw.Stop();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        sw.ElapsedMilliseconds.ShouldBeLessThan(ThresholdMs,
            $"GET /api/Proyectos/catalogo tardó {sw.ElapsedMilliseconds}ms (umbral: {ThresholdMs}ms)");
    }

    [Test]
    public async Task ConcurrentLoad_10ReadRequests_AllWithinThreshold()
    {
        await RunAsUserAsync("perf.load@local", "Testing1234!", ["Superuser"]);
        using var client = CreateClient();

        // Sembrar datos base
        for (int i = 1; i <= 10; i++)
        {
            await client.PostAsJsonAsync("/api/Publications", new
            {
                title = $"Carga de publicación #{i}",
                publicationData = "Datos de carga",
                publicationType = 4,
                publishedDate = "2024",
                additionalAuthorIds = Array.Empty<string>(),
                additionalAuthorNames = Array.Empty<string>(),
                additionalUserIds = Array.Empty<string>()
            });
        }

        const int concurrent = 10;
        var sw = Stopwatch.StartNew();
        var tasks = Enumerable.Range(0, concurrent).Select(_ =>
            client.GetAsync("/api/Publications/todas"));
        var responses = await Task.WhenAll(tasks);
        sw.Stop();

        foreach (var r in responses)
            r.StatusCode.ShouldBe(HttpStatusCode.OK);

        sw.ElapsedMilliseconds.ShouldBeLessThan(ThresholdMs * 2,
            $"10 lecturas concurrentes tardaron {sw.ElapsedMilliseconds}ms en total");
    }

    private static async Task<(string areaId, string clasifId, string jefeId)> SeedBaseForPerformanceAsync()
    {
        return await ExecuteDbContextAsync(async db =>
        {
            var univ = new Universidad { Id = "uh-perf", Nombre = "UH Perf" };
            db.Universidades.Add(univ);

            var area = new Area { Id = "area-perf", Nombre = "Área Perf", Descripcion = "d", UniversidadId = univ.Id };
            db.Areas.Add(area);

            var clasif = new Clasificacion { Id = "clasif-perf", Nombre = "Clasif Perf" };
            db.Clasificaciones.Add(clasif);

            var jefe = new User
            {
                Id = "jefe-perf",
                UserName = "jefe.perf@local",
                Email = "jefe.perf@local",
                UserLastName1 = "Jefe",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Testing1234!"),
                BirthDate = DateTime.SpecifyKind(new DateTime(1985, 1, 1), DateTimeKind.Utc),
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                AreaId = area.Id,
            };
            db.Users.Add(jefe);

            await db.SaveChangesAsync();
            return (area.Id, clasif.Id, jefe.Id);
        });
    }
}
