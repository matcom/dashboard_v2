using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Events;
using Dashboard_v2.Domain.Entities;
using Dashboard_v2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Dashboard_v2.Application.UnitTests.Events;

[TestFixture]
public class EventServiceTests
{
    private ApplicationDbContext _db = null!;
    private Mock<IUser> _currentUser = null!;
    private EventService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(options);
        _currentUser = new Mock<IUser>();
        _currentUser.Setup(u => u.Id).Returns("user-1");
        _sut = new EventService(_db, _currentUser.Object);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    private async Task SeedBaseDataAsync()
    {
        _db.Countries.Add(new Country { Id = 1, Name = "Cuba" });
        _db.EventTypes.Add(new EventType { Id = 1, Name = "Conferencia" });
        await _db.SaveChangesAsync();
    }

    private static User MakeUser(string id) =>
        new() { Id = id, UserName = "user", UserLastName1 = "User", Email = $"{id}@test.cu" };

    private static Event MakeEvent(int id, string? name = null) =>
        new() { Id = id, Name = name ?? $"Evento {id}", CountryId = 1, EventTypeId = 1 };

    // ─── GetAllEventsAsync ────────────────────────────────────────────────────

    [Test]
    public async Task GetAllEventsAsync_Empty_ReturnsEmptyList()
    {
        var result = await _sut.GetAllEventsAsync();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GetAllEventsAsync_WithData_ReturnsEventDtos()
    {
        await SeedBaseDataAsync();
        _db.Events.Add(MakeEvent(5, "Congreso IA"));
        await _db.SaveChangesAsync();

        var result = await _sut.GetAllEventsAsync();

        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Congreso IA");
        result[0].CountryId.ShouldBe(1);
    }

    [Test]
    public async Task GetAllEventsAsync_CountsPresentations()
    {
        await SeedBaseDataAsync();
        _db.Users.Add(MakeUser("user-1"));
        _db.Events.Add(MakeEvent(10));
        await _db.SaveChangesAsync();

        _db.Presentations.Add(new Presentation { Name = "P1", EventId = 10, UserId = "user-1", Fecha = DateTime.UtcNow });
        _db.Presentations.Add(new Presentation { Name = "P2", EventId = 10, UserId = "user-1", Fecha = DateTime.UtcNow });
        await _db.SaveChangesAsync();

        var result = await _sut.GetAllEventsAsync();
        result[0].PresentationCount.ShouldBe(2);
    }

    // ─── GetMyEventsAsync ─────────────────────────────────────────────────────

    [Test]
    public async Task GetMyEventsAsync_NoParticipaciones_ReturnsEmpty()
    {
        var result = await _sut.GetMyEventsAsync();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GetMyEventsAsync_WithParticipacion_ReturnsEvent()
    {
        await SeedBaseDataAsync();
        _db.Users.Add(MakeUser("user-1"));
        _db.Events.Add(MakeEvent(60, "Mi Congreso"));
        await _db.SaveChangesAsync();

        _db.Presentations.Add(new Presentation { Name = "Mi Ponencia", EventId = 60, UserId = "user-1", Fecha = DateTime.UtcNow });
        await _db.SaveChangesAsync();

        var result = await _sut.GetMyEventsAsync();
        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Mi Congreso");
        result[0].PresentationCount.ShouldBe(1);
    }

    [Test]
    public async Task GetMyEventsAsync_AsOrganizador_ReturnsEvent()
    {
        await SeedBaseDataAsync();
        _db.Users.Add(MakeUser("user-1"));
        _db.Events.Add(MakeEvent(70, "Evento Organizado"));
        await _db.SaveChangesAsync();

        _db.EventOrganizadores.Add(new EventOrganizador { EventId = 70, UserId = "user-1" });
        await _db.SaveChangesAsync();

        var result = await _sut.GetMyEventsAsync();
        result.Count.ShouldBe(1);
        result[0].OrganizadorIds.ShouldContain("user-1");
    }

    // ─── GetCountriesAsync ────────────────────────────────────────────────────

    [Test]
    public async Task GetCountriesAsync_Empty_ReturnsEmpty()
    {
        var result = await _sut.GetCountriesAsync();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GetCountriesAsync_WithData_ReturnsOrdered()
    {
        _db.Countries.Add(new Country { Id = 1, Name = "Zambia" });
        _db.Countries.Add(new Country { Id = 2, Name = "Argentina" });
        await _db.SaveChangesAsync();

        var result = await _sut.GetCountriesAsync();
        result.Count.ShouldBe(2);
        result[0].Name.ShouldBe("Argentina");
    }

    // ─── CreateCountryAsync ───────────────────────────────────────────────────

    [Test]
    public async Task CreateCountryAsync_EmptyName_Fails()
    {
        var (result, country) = await _sut.CreateCountryAsync(new CreateCountryRequest(""));
        result.Succeeded.ShouldBeFalse();
        country.ShouldBeNull();
    }

    [Test]
    public async Task CreateCountryAsync_DuplicateName_Fails()
    {
        _db.Countries.Add(new Country { Id = 1, Name = "Cuba" });
        await _db.SaveChangesAsync();

        var (result, _) = await _sut.CreateCountryAsync(new CreateCountryRequest("Cuba"));
        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task CreateCountryAsync_ValidName_Succeeds()
    {
        var (result, country) = await _sut.CreateCountryAsync(new CreateCountryRequest("España"));
        result.Succeeded.ShouldBeTrue();
        country.ShouldNotBeNull();
        country!.Name.ShouldBe("España");
    }

    // ─── GetEventTypesAsync ───────────────────────────────────────────────────

    [Test]
    public async Task GetEventTypesAsync_Empty_ReturnsEmpty()
    {
        var result = await _sut.GetEventTypesAsync();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GetEventTypesAsync_WithData_ReturnsOrdered()
    {
        _db.EventTypes.Add(new EventType { Id = 2, Name = "Simposio" });
        _db.EventTypes.Add(new EventType { Id = 1, Name = "Conferencia" });
        await _db.SaveChangesAsync();

        var result = await _sut.GetEventTypesAsync();
        result.Count.ShouldBe(2);
        result[0].Id.ShouldBe(1);
    }

    // ─── CreateEventAsync ─────────────────────────────────────────────────────

    [Test]
    public async Task CreateEventAsync_EmptyName_Fails()
    {
        await SeedBaseDataAsync();
        var (result, id) = await _sut.CreateEventAsync(new CreateEventRequest { Name = "", CountryId = 1, EventType = 1 });
        result.Succeeded.ShouldBeFalse();
        id.ShouldBeNull();
    }

    [Test]
    public async Task CreateEventAsync_InvalidCountry_Fails()
    {
        await SeedBaseDataAsync();
        var (result, _) = await _sut.CreateEventAsync(new CreateEventRequest { Name = "Evento X", CountryId = 999, EventType = 1 });
        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task CreateEventAsync_InvalidEventType_Fails()
    {
        await SeedBaseDataAsync();
        var (result, _) = await _sut.CreateEventAsync(new CreateEventRequest { Name = "Evento X", CountryId = 1, EventType = 999 });
        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task CreateEventAsync_InvalidRedId_Fails()
    {
        await SeedBaseDataAsync();
        var (result, _) = await _sut.CreateEventAsync(new CreateEventRequest
        {
            Name = "Evento con Red",
            CountryId = 1,
            EventType = 1,
            RedId = "red-inexistente",
        });
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Red"));
    }

    [Test]
    public async Task CreateEventAsync_Valid_Succeeds()
    {
        await SeedBaseDataAsync();
        var (result, id) = await _sut.CreateEventAsync(new CreateEventRequest
        {
            Name = "Congreso Internacional",
            CountryId = 1,
            EventType = 1,
            Institutions = ["UH"],
        });
        result.Succeeded.ShouldBeTrue();
        id.ShouldNotBeNull();
    }

    [Test]
    public async Task CreateEventAsync_WithOrganizadorId_AddsOrganizador()
    {
        await SeedBaseDataAsync();
        _db.Users.Add(MakeUser("user-1"));
        await _db.SaveChangesAsync();

        var (result, id) = await _sut.CreateEventAsync(new CreateEventRequest
        {
            Name = "Evento con Organizador",
            CountryId = 1,
            EventType = 1,
            OrganizadorIds = ["user-1"],
        });

        result.Succeeded.ShouldBeTrue();
        var organizadores = await _db.EventOrganizadores.Where(o => o.EventId == id).ToListAsync();
        organizadores.Count.ShouldBe(1);
        organizadores[0].UserId.ShouldBe("user-1");
    }

    // ─── UpdateEventAsync ─────────────────────────────────────────────────────

    [Test]
    public async Task UpdateEventAsync_EmptyName_Fails()
    {
        var result = await _sut.UpdateEventAsync(1, new UpdateEventRequest { Name = "", CountryId = 1, EventType = 1 });
        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task UpdateEventAsync_NonExistingEvent_Fails()
    {
        await SeedBaseDataAsync();
        var result = await _sut.UpdateEventAsync(999, new UpdateEventRequest { Name = "Test", CountryId = 1, EventType = 1 });
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("no encontrado"));
    }

    [Test]
    public async Task UpdateEventAsync_InvalidCountry_Fails()
    {
        await SeedBaseDataAsync();
        _db.Events.Add(MakeEvent(5));
        await _db.SaveChangesAsync();

        var result = await _sut.UpdateEventAsync(5, new UpdateEventRequest { Name = "Test", CountryId = 999, EventType = 1 });
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("País"));
    }

    [Test]
    public async Task UpdateEventAsync_InvalidEventType_Fails()
    {
        await SeedBaseDataAsync();
        _db.Events.Add(MakeEvent(6));
        await _db.SaveChangesAsync();

        var result = await _sut.UpdateEventAsync(6, new UpdateEventRequest { Name = "Test", CountryId = 1, EventType = 999 });
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Tipo"));
    }

    [Test]
    public async Task UpdateEventAsync_InvalidRedId_Fails()
    {
        await SeedBaseDataAsync();
        _db.Events.Add(MakeEvent(100));
        await _db.SaveChangesAsync();

        var result = await _sut.UpdateEventAsync(100, new UpdateEventRequest
        {
            Name = "Updated",
            CountryId = 1,
            EventType = 1,
            RedId = "red-inexistente",
        });
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Red"));
    }

    [Test]
    public async Task UpdateEventAsync_Valid_Succeeds()
    {
        await SeedBaseDataAsync();
        _db.Events.Add(MakeEvent(7, "Original"));
        await _db.SaveChangesAsync();

        var result = await _sut.UpdateEventAsync(7, new UpdateEventRequest { Name = "Updated", CountryId = 1, EventType = 1 });
        result.Succeeded.ShouldBeTrue();
        (await _db.Events.FindAsync(7))!.Name.ShouldBe("Updated");
    }

    [Test]
    public async Task UpdateEventAsync_UpdatesOrganizadores()
    {
        await SeedBaseDataAsync();
        _db.Users.Add(MakeUser("user-1"));
        _db.Users.Add(MakeUser("user-2"));
        _db.Events.Add(MakeEvent(20));
        await _db.SaveChangesAsync();

        _db.EventOrganizadores.Add(new EventOrganizador { EventId = 20, UserId = "user-1" });
        await _db.SaveChangesAsync();

        var result = await _sut.UpdateEventAsync(20, new UpdateEventRequest
        {
            Name = "Evento 20",
            CountryId = 1,
            EventType = 1,
            OrganizadorIds = ["user-2"],
        });

        result.Succeeded.ShouldBeTrue();
        var orgs = await _db.EventOrganizadores.Where(o => o.EventId == 20).ToListAsync();
        orgs.Count.ShouldBe(1);
        orgs[0].UserId.ShouldBe("user-2");
    }

    // ─── DeleteEventAsync ─────────────────────────────────────────────────────

    [Test]
    public async Task DeleteEventAsync_NonExistingId_Fails()
    {
        var result = await _sut.DeleteEventAsync(999);
        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task DeleteEventAsync_ExistingEvent_Succeeds()
    {
        await SeedBaseDataAsync();
        _db.Events.Add(MakeEvent(1));
        await _db.SaveChangesAsync();

        var result = await _sut.DeleteEventAsync(1);
        result.Succeeded.ShouldBeTrue();
        _db.Events.Count().ShouldBe(0);
    }

    [Test]
    public async Task DeleteEventAsync_WithPresentations_Fails()
    {
        await SeedBaseDataAsync();
        _db.Users.Add(MakeUser("user-1"));
        _db.Events.Add(MakeEvent(2));
        await _db.SaveChangesAsync();

        _db.Presentations.Add(new Presentation { Name = "P", EventId = 2, UserId = "user-1", Fecha = DateTime.UtcNow });
        await _db.SaveChangesAsync();

        var result = await _sut.DeleteEventAsync(2);
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("presentaciones"));
    }

    // ─── GetAllPresentationsAsync ─────────────────────────────────────────────

    [Test]
    public async Task GetAllPresentationsAsync_Empty_ReturnsEmpty()
    {
        var result = await _sut.GetAllPresentationsAsync();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GetAllPresentationsAsync_WithData_ReturnsPresentationDtos()
    {
        await SeedBaseDataAsync();
        _db.Users.Add(MakeUser("user-a"));
        _db.Events.Add(MakeEvent(6, "CCIA"));
        await _db.SaveChangesAsync();

        var fecha = new DateTime(2024, 5, 10, 0, 0, 0, DateTimeKind.Utc);
        _db.Presentations.Add(new Presentation { Id = 20, Name = "Ponencia Datos", EventId = 6, UserId = "user-a", Fecha = fecha });
        await _db.SaveChangesAsync();

        var result = await _sut.GetAllPresentationsAsync();

        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Ponencia Datos");
        result[0].UserId.ShouldBe("user-a");
        result[0].Fecha.ShouldBe(fecha);
    }

    // ─── GetMyPresentationsAsync ──────────────────────────────────────────────

    [Test]
    public async Task GetMyPresentationsAsync_Empty_ReturnsEmpty()
    {
        var result = await _sut.GetMyPresentationsAsync();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GetMyPresentationsAsync_WithParticipacion_ReturnsList()
    {
        await SeedBaseDataAsync();
        _db.Users.Add(MakeUser("user-1"));
        _db.Events.Add(MakeEvent(7, "Jornada"));
        await _db.SaveChangesAsync();

        _db.Presentations.Add(new Presentation { Id = 30, Name = "Mi Ponencia", EventId = 7, UserId = "user-1", Fecha = DateTime.UtcNow });
        await _db.SaveChangesAsync();

        var result = await _sut.GetMyPresentationsAsync();

        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Mi Ponencia");
        result[0].UserId.ShouldBe("user-1");
    }

    [Test]
    public async Task GetMyPresentationsAsync_OtherUserPresentations_NotReturned()
    {
        await SeedBaseDataAsync();
        _db.Users.Add(MakeUser("user-1"));
        _db.Users.Add(MakeUser("user-2"));
        _db.Events.Add(MakeEvent(8));
        await _db.SaveChangesAsync();

        _db.Presentations.Add(new Presentation { Name = "Ponencia Ajena", EventId = 8, UserId = "user-2", Fecha = DateTime.UtcNow });
        await _db.SaveChangesAsync();

        var result = await _sut.GetMyPresentationsAsync();
        result.ShouldBeEmpty();
    }

    // ─── CreatePresentationAsync ──────────────────────────────────────────────

    [Test]
    public async Task CreatePresentationAsync_EmptyName_Fails()
    {
        var (result, _) = await _sut.CreatePresentationAsync(new CreatePresentationRequest { Name = "", EventId = 1, Fecha = DateTime.UtcNow });
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("nombre"));
    }

    [Test]
    public async Task CreatePresentationAsync_NonExistingEvent_Fails()
    {
        var (result, _) = await _sut.CreatePresentationAsync(new CreatePresentationRequest { Name = "Mi Ponencia", EventId = 999, Fecha = DateTime.UtcNow });
        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task CreatePresentationAsync_UserNotFound_Fails()
    {
        await SeedBaseDataAsync();
        _db.Events.Add(MakeEvent(1));
        await _db.SaveChangesAsync();

        // user-1 not seeded in Users table
        var (result, _) = await _sut.CreatePresentationAsync(new CreatePresentationRequest { Name = "Ponencia", EventId = 1, Fecha = DateTime.UtcNow });
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Usuario"));
    }

    [Test]
    public async Task CreatePresentationAsync_Valid_Succeeds()
    {
        await SeedBaseDataAsync();
        _db.Users.Add(MakeUser("user-1"));
        _db.Events.Add(MakeEvent(1));
        await _db.SaveChangesAsync();

        var fecha = new DateTime(2024, 3, 15, 0, 0, 0, DateTimeKind.Utc);
        var (result, id) = await _sut.CreatePresentationAsync(new CreatePresentationRequest
        {
            Name = "Ponencia Valid",
            EventId = 1,
            Fecha = fecha,
        });

        result.Succeeded.ShouldBeTrue();
        id.ShouldNotBeNull();

        var saved = await _db.Presentations.FindAsync(id);
        saved.ShouldNotBeNull();
        saved!.UserId.ShouldBe("user-1");
        saved.Fecha.ShouldBe(fecha);
    }

    // ─── UpdatePresentationAsync ──────────────────────────────────────────────

    [Test]
    public async Task UpdatePresentationAsync_EmptyName_Fails()
    {
        var result = await _sut.UpdatePresentationAsync(1, new UpdatePresentationRequest { Name = "", EventId = 1, Fecha = DateTime.UtcNow });
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("nombre"));
    }

    [Test]
    public async Task UpdatePresentationAsync_PresentationNotFound_Fails()
    {
        var result = await _sut.UpdatePresentationAsync(9999, new UpdatePresentationRequest { Name = "X", EventId = 1, Fecha = DateTime.UtcNow });
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("encontrada"));
    }

    [Test]
    public async Task UpdatePresentationAsync_NotOwner_Fails()
    {
        await SeedBaseDataAsync();
        _db.Users.Add(MakeUser("user-2"));
        _db.Events.Add(MakeEvent(200));
        await _db.SaveChangesAsync();

        _db.Presentations.Add(new Presentation { Id = 200, Name = "Pres200", EventId = 200, UserId = "user-2", Fecha = DateTime.UtcNow });
        await _db.SaveChangesAsync();

        var result = await _sut.UpdatePresentationAsync(200, new UpdatePresentationRequest { Name = "Hack", EventId = 200, Fecha = DateTime.UtcNow });
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("permiso"));
    }

    [Test]
    public async Task UpdatePresentationAsync_InvalidEvent_Fails()
    {
        await SeedBaseDataAsync();
        _db.Users.Add(MakeUser("user-1"));
        _db.Events.Add(MakeEvent(300));
        await _db.SaveChangesAsync();

        _db.Presentations.Add(new Presentation { Id = 300, Name = "Pres300", EventId = 300, UserId = "user-1", Fecha = DateTime.UtcNow });
        await _db.SaveChangesAsync();

        var result = await _sut.UpdatePresentationAsync(300, new UpdatePresentationRequest { Name = "Updated", EventId = 9999, Fecha = DateTime.UtcNow });
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("evento"));
    }

    [Test]
    public async Task UpdatePresentationAsync_Valid_Succeeds()
    {
        await SeedBaseDataAsync();
        _db.Users.Add(MakeUser("user-1"));
        _db.Events.Add(MakeEvent(2));
        await _db.SaveChangesAsync();

        _db.Presentations.Add(new Presentation { Id = 10, Name = "Original Pres", EventId = 2, UserId = "user-1", Fecha = DateTime.UtcNow });
        await _db.SaveChangesAsync();

        var newFecha = new DateTime(2025, 1, 20, 0, 0, 0, DateTimeKind.Utc);
        var result = await _sut.UpdatePresentationAsync(10, new UpdatePresentationRequest { Name = "Updated Pres", EventId = 2, Fecha = newFecha });

        result.Succeeded.ShouldBeTrue();
        var updated = await _db.Presentations.FindAsync(10);
        updated!.Name.ShouldBe("Updated Pres");
        updated.Fecha.ShouldBe(newFecha);
    }

    // ─── DeletePresentationAsync ──────────────────────────────────────────────

    [Test]
    public async Task DeletePresentationAsync_NonExisting_Fails()
    {
        var result = await _sut.DeletePresentationAsync(999);
        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task DeletePresentationAsync_NotOwner_Fails()
    {
        await SeedBaseDataAsync();
        _db.Users.Add(MakeUser("user-2"));
        _db.Events.Add(MakeEvent(50));
        await _db.SaveChangesAsync();

        _db.Presentations.Add(new Presentation { Id = 50, Name = "Pres50", EventId = 50, UserId = "user-2", Fecha = DateTime.UtcNow });
        await _db.SaveChangesAsync();

        var result = await _sut.DeletePresentationAsync(50);
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("permiso"));
    }

    [Test]
    public async Task DeletePresentationAsync_ValidOwner_Succeeds()
    {
        await SeedBaseDataAsync();
        _db.Users.Add(MakeUser("user-1"));
        _db.Events.Add(MakeEvent(51));
        await _db.SaveChangesAsync();

        _db.Presentations.Add(new Presentation { Id = 51, Name = "Pres to Delete", EventId = 51, UserId = "user-1", Fecha = DateTime.UtcNow });
        await _db.SaveChangesAsync();

        var result = await _sut.DeletePresentationAsync(51);
        result.Succeeded.ShouldBeTrue();
        (await _db.Presentations.FindAsync(51)).ShouldBeNull();
    }
}
