using Dashboard_v2.Application.Common;
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
    private Mock<IAuthorResolutionService> _authorResolution = null!;
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
        _authorResolution = new Mock<IAuthorResolutionService>();
        _sut = new EventService(_db, _currentUser.Object, _authorResolution.Object);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    private async Task SeedBaseDataAsync()
    {
        _db.Countries.Add(new Country { Id = 1, Name = "Cuba" });
        _db.EventTypes.Add(new EventType { Id = 1, Name = "Conferencia" });
        await _db.SaveChangesAsync();
    }

    // ─── GetAllEventsAsync ────────────────────────────────────────────────────

    [Test]
    public async Task GetAllEventsAsync_Empty_ReturnsEmptyList()
    {
        var result = await _sut.GetAllEventsAsync();
        result.ShouldBeEmpty();
    }

    // ─── GetMyEventsAsync ─────────────────────────────────────────────────────

    [Test]
    public async Task GetMyEventsAsync_NoLinkedAuthor_ReturnsEmpty()
    {
        var result = await _sut.GetMyEventsAsync();
        result.ShouldBeEmpty();
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

        var (result, country) = await _sut.CreateCountryAsync(new CreateCountryRequest("Cuba"));
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
        var (result, id) = await _sut.CreateEventAsync(new CreateEventRequest { Name = "", CountryId = 1, EventType = 1, Institutions = new() });
        result.Succeeded.ShouldBeFalse();
        id.ShouldBeNull();
    }

    [Test]
    public async Task CreateEventAsync_InvalidCountry_Fails()
    {
        await SeedBaseDataAsync();
        var (result, id) = await _sut.CreateEventAsync(new CreateEventRequest { Name = "Evento X", CountryId = 999, EventType = 1, Institutions = new() });
        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task CreateEventAsync_InvalidEventType_Fails()
    {
        await SeedBaseDataAsync();
        var (result, id) = await _sut.CreateEventAsync(new CreateEventRequest { Name = "Evento X", CountryId = 1, EventType = 999, Institutions = new() });
        result.Succeeded.ShouldBeFalse();
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
            Institutions = new List<string> { "UH" },
            AreaIdsPatrocinadoras = new List<string>()
        });

        result.Succeeded.ShouldBeTrue();
        id.ShouldNotBeNull();
    }

    // ─── UpdateEventAsync ─────────────────────────────────────────────────────

    [Test]
    public async Task UpdateEventAsync_EmptyName_Fails()
    {
        var result = await _sut.UpdateEventAsync(1, new UpdateEventRequest { Name = "", CountryId = 1, EventType = 1, Institutions = new() });
        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task UpdateEventAsync_NonExistingEvent_Fails()
    {
        await SeedBaseDataAsync();
        var result = await _sut.UpdateEventAsync(999, new UpdateEventRequest { Name = "Test", CountryId = 1, EventType = 1, Institutions = new() });
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("no encontrado"));
    }

    [Test]
    public async Task UpdateEventAsync_InvalidCountry_Fails()
    {
        await SeedBaseDataAsync();
        _db.Events.Add(new Event { Id = 5, Name = "Ev", CountryId = 1, EventTypeId = 1, Institutions = new List<Institution>() });
        await _db.SaveChangesAsync();

        var result = await _sut.UpdateEventAsync(5, new UpdateEventRequest { Name = "Test", CountryId = 999, EventType = 1, Institutions = new() });
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("País"));
    }

    [Test]
    public async Task UpdateEventAsync_InvalidEventType_Fails()
    {
        await SeedBaseDataAsync();
        _db.Events.Add(new Event { Id = 6, Name = "Ev", CountryId = 1, EventTypeId = 1, Institutions = new List<Institution>() });
        await _db.SaveChangesAsync();

        var result = await _sut.UpdateEventAsync(6, new UpdateEventRequest { Name = "Test", CountryId = 1, EventType = 999, Institutions = new() });
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Tipo"));
    }

    [Test]
    public async Task UpdateEventAsync_Valid_Succeeds()
    {
        await SeedBaseDataAsync();
        _db.Events.Add(new Event { Id = 7, Name = "Original", CountryId = 1, EventTypeId = 1, Institutions = new List<Institution>() });
        await _db.SaveChangesAsync();

        var result = await _sut.UpdateEventAsync(7, new UpdateEventRequest
        {
            Name = "Updated",
            CountryId = 1,
            EventType = 1,
            Institutions = new List<string>(),
            AreaIdsPatrocinadoras = new List<string>()
        });
        result.Succeeded.ShouldBeTrue();
        var ev = await _db.Events.FindAsync(7);
        ev!.Name.ShouldBe("Updated");
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
        _db.Events.Add(new Event { Id = 1, Name = "Evento A", CountryId = 1, EventTypeId = 1, Institutions = new List<Institution>() });
        await _db.SaveChangesAsync();

        var result = await _sut.DeleteEventAsync(1);
        result.Succeeded.ShouldBeTrue();
        _db.Events.Count().ShouldBe(0);
    }

    // ─── GetAllPresentationsAsync ─────────────────────────────────────────────

    [Test]
    public async Task GetAllPresentationsAsync_Empty_ReturnsEmpty()
    {
        var result = await _sut.GetAllPresentationsAsync();
        result.ShouldBeEmpty();
    }

    // ─── GetMyPresentationsAsync ──────────────────────────────────────────────

    [Test]
    public async Task GetMyPresentationsAsync_Empty_ReturnsEmpty()
    {
        var result = await _sut.GetMyPresentationsAsync();
        result.ShouldBeEmpty();
    }

    // ─── CreatePresentationAsync ─────────────────────────────────────────────

    [Test]
    public async Task CreatePresentationAsync_EmptyName_Fails()
    {
        var request = new CreatePresentationRequest
        {
            Name = "",
            EventId = 1,
            CoauthorIds = new List<string>(),
            CoauthorUserIds = new List<string>(),
            CoauthorNames = new List<string>()
        };
        var (result, _) = await _sut.CreatePresentationAsync(request);
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("nombre"));
    }

    [Test]
    public async Task CreatePresentationAsync_NonExistingEvent_Fails()
    {
        var request = new CreatePresentationRequest
        {
            Name = "Mi Ponencia",
            EventId = 999,
            CoauthorIds = new List<string>(),
            CoauthorUserIds = new List<string>(),
            CoauthorNames = new List<string>()
        };
        var (result, _) = await _sut.CreatePresentationAsync(request);
        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task CreatePresentationAsync_AuthorResolutionReturnsNull_Fails()
    {
        await SeedBaseDataAsync();
        _db.Events.Add(new Event { Id = 1, Name = "Conf", CountryId = 1, EventTypeId = 1, Institutions = new List<Institution>() });
        await _db.SaveChangesAsync();

        _authorResolution.Setup(a => a.GetOrCreateForUserAsync("user-1", default))
            .ReturnsAsync((Author?)null);

        var request = new CreatePresentationRequest
        {
            Name = "Ponencia X",
            EventId = 1,
            CoauthorIds = new List<string>(),
            CoauthorUserIds = new List<string>(),
            CoauthorNames = new List<string>()
        };
        var (result, _) = await _sut.CreatePresentationAsync(request);
        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task CreatePresentationAsync_Valid_Succeeds()
    {
        await SeedBaseDataAsync();
        _db.Events.Add(new Event { Id = 1, Name = "Conf", CountryId = 1, EventTypeId = 1, Institutions = new List<Institution>() });
        await _db.SaveChangesAsync();

        var author = new Author { Id = "a1", LastName = "Perez", Name = "J Perez", SearchKey = "jperez", LastNameKey = "perez" };
        _db.Authors.Add(author);
        await _db.SaveChangesAsync();

        _authorResolution.Setup(a => a.GetOrCreateForUserAsync("user-1", default))
            .ReturnsAsync(author);

        var request = new CreatePresentationRequest
        {
            Name = "Ponencia Valid",
            EventId = 1,
            CoauthorIds = new List<string>(),
            CoauthorUserIds = new List<string>(),
            CoauthorNames = new List<string>()
        };
        var (result, id) = await _sut.CreatePresentationAsync(request);
        result.Succeeded.ShouldBeTrue();
        id.ShouldNotBeNull();
    }

    // ─── UpdatePresentationAsync ──────────────────────────────────────────────

    [Test]
    public async Task UpdatePresentationAsync_EmptyName_Fails()
    {
        var request = new UpdatePresentationRequest
        {
            Name = "",
            EventId = 1,
            CoauthorIds = new List<string>(),
            CoauthorUserIds = new List<string>(),
            CoauthorNames = new List<string>()
        };
        var result = await _sut.UpdatePresentationAsync(1, request);
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("nombre"));
    }

    [Test]
    public async Task UpdatePresentationAsync_NoLinkedAuthor_Fails()
    {
        var request = new UpdatePresentationRequest
        {
            Name = "Updated",
            EventId = 1,
            CoauthorIds = new List<string>(),
            CoauthorUserIds = new List<string>(),
            CoauthorNames = new List<string>()
        };
        var result = await _sut.UpdatePresentationAsync(999, request);
        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task UpdatePresentationAsync_Valid_Succeeds()
    {
        await SeedBaseDataAsync();
        _db.Events.Add(new Event { Id = 2, Name = "Conf", CountryId = 1, EventTypeId = 1, Institutions = new List<Institution>() });
        var author = new Author { Id = "a2", LastName = "Garcia", Name = "A Garcia", SearchKey = "agarcia", LastNameKey = "garcia" };
        author.UserId = "user-1";
        _db.Authors.Add(author);
        var pres = new Presentation
        {
            Id = 10,
            Name = "Original Pres",
            EventId = 2,
            AuthorPresentations = new List<AuthorPresentation> { new AuthorPresentation { AuthorId = "a2" } }
        };
        _db.Presentations.Add(pres);
        await _db.SaveChangesAsync();

        var request = new UpdatePresentationRequest
        {
            Name = "Updated Pres",
            EventId = 2,
            CoauthorIds = new List<string>(),
            CoauthorUserIds = new List<string>(),
            CoauthorNames = new List<string>()
        };
        var result = await _sut.UpdatePresentationAsync(10, request);
        result.Succeeded.ShouldBeTrue();
        var updated = await _db.Presentations.FindAsync(10);
        updated!.Name.ShouldBe("Updated Pres");
    }

    // ─── DeletePresentationAsync ──────────────────────────────────────────────

    [Test]
    public async Task DeletePresentationAsync_NonExisting_Fails()
    {
        var result = await _sut.DeletePresentationAsync(999);
        result.Succeeded.ShouldBeFalse();
    }
}
