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

    [Test]
    public async Task DeletePresentationAsync_NotAuthor_Fails()
    {
        await SeedBaseDataAsync();
        var evt = new Event { Id = 50, Name = "E50", CountryId = 1, EventTypeId = 1, Institutions = new List<Institution>() };
        var author = new Author { Id = "a-other", Name = "Z", LastName = "Z", SearchKey = "z z", LastNameKey = "z" };
        _db.Events.Add(evt);
        _db.Authors.Add(author);
        await _db.SaveChangesAsync();

        var pres = new Presentation
        {
            Id = 50,
            Name = "Pres50",
            EventId = 50,
            AuthorPresentations = new List<AuthorPresentation> { new() { AuthorId = "a-other" } }
        };
        _db.Presentations.Add(pres);
        await _db.SaveChangesAsync();

        // user-1 has no linked author so returns "No tienes un perfil de autor"
        var result = await _sut.DeletePresentationAsync(50);
        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task DeletePresentationAsync_ValidOwner_Succeeds()
    {
        await SeedBaseDataAsync();
        var evt = new Event { Id = 51, Name = "E51", CountryId = 1, EventTypeId = 1, Institutions = new List<Institution>() };
        var author = new Author { Id = "a-del", Name = "Del", LastName = "User", SearchKey = "user del", LastNameKey = "user", UserId = "user-1" };
        _db.Events.Add(evt);
        _db.Authors.Add(author);
        await _db.SaveChangesAsync();

        var pres = new Presentation
        {
            Id = 51,
            Name = "Pres to Delete",
            EventId = 51,
            AuthorPresentations = new List<AuthorPresentation> { new() { AuthorId = "a-del" } }
        };
        _db.Presentations.Add(pres);
        await _db.SaveChangesAsync();

        var result = await _sut.DeletePresentationAsync(51);
        result.Succeeded.ShouldBeTrue();
        (await _db.Presentations.FindAsync(51)).ShouldBeNull();
    }

    // ─── GetMyEventsAsync with data ───────────────────────────────────────────

    [Test]
    public async Task GetMyEventsAsync_WithLinkedAuthor_ReturnsEvents()
    {
        await SeedBaseDataAsync();
        var evt = new Event { Id = 60, Name = "Mi Congreso", CountryId = 1, EventTypeId = 1, Institutions = new List<Institution>() };
        var author = new Author { Id = "a-myevt", Name = "R", LastName = "S", SearchKey = "s r", LastNameKey = "s", UserId = "user-1" };
        _db.Events.Add(evt);
        _db.Authors.Add(author);
        await _db.SaveChangesAsync();

        var pres = new Presentation
        {
            Id = 60,
            Name = "Mi Ponencia Congreso",
            EventId = 60,
            AuthorPresentations = new List<AuthorPresentation> { new() { AuthorId = "a-myevt" } }
        };
        _db.Presentations.Add(pres);
        await _db.SaveChangesAsync();

        var result = await _sut.GetMyEventsAsync();
        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Mi Congreso");
        result[0].PresentationCount.ShouldBe(1);
    }

    // ─── GetAllEventsAsync with data ──────────────────────────────────────────

    [Test]
    public async Task GetAllEventsAsync_WithData_ReturnsEventDtos()
    {
        await SeedBaseDataAsync();
        _db.Events.Add(new Event { Id = 5, Name = "Congreso IA", CountryId = 1, EventTypeId = 1, Institutions = new List<Institution>() });
        await _db.SaveChangesAsync();

        var result = await _sut.GetAllEventsAsync();

        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Congreso IA");
        result[0].CountryId.ShouldBe(1);
    }

    // ─── GetAllPresentationsAsync with data ───────────────────────────────────

    [Test]
    public async Task GetAllPresentationsAsync_WithData_ReturnsPresentationDtos()
    {
        await SeedBaseDataAsync();
        var evt = new Event { Id = 6, Name = "CCIA", CountryId = 1, EventTypeId = 1, Institutions = new List<Institution>() };
        var author = new Author { Id = "a-p1", Name = "Luis", LastName = "Ramos", SearchKey = "ramos luis", LastNameKey = "ramos" };
        _db.Events.Add(evt);
        _db.Authors.Add(author);
        await _db.SaveChangesAsync();

        var pres = new Presentation
        {
            Id = 20,
            Name = "Ponencia Datos",
            EventId = evt.Id,
            AuthorPresentations = new List<AuthorPresentation> { new() { AuthorId = "a-p1" } }
        };
        _db.Presentations.Add(pres);
        await _db.SaveChangesAsync();

        var result = await _sut.GetAllPresentationsAsync();

        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Ponencia Datos");
        result[0].Authors.Count.ShouldBe(1);
        result[0].Authors[0].Id.ShouldBe("a-p1");
    }

    // ─── GetMyPresentationsAsync with data ────────────────────────────────────

    [Test]
    public async Task GetMyPresentationsAsync_WithLinkedAuthor_ReturnsList()
    {
        await SeedBaseDataAsync();
        var evt = new Event { Id = 7, Name = "Jornada", CountryId = 1, EventTypeId = 1, Institutions = new List<Institution>() };
        var author = new Author { Id = "a-my", Name = "Ana", LastName = "López", SearchKey = "lopez ana", LastNameKey = "lopez", UserId = "user-1" };
        _db.Events.Add(evt);
        _db.Authors.Add(author);
        await _db.SaveChangesAsync();

        var pres = new Presentation
        {
            Id = 30,
            Name = "Mi Ponencia",
            EventId = evt.Id,
            AuthorPresentations = new List<AuthorPresentation> { new() { AuthorId = "a-my" } }
        };
        _db.Presentations.Add(pres);
        await _db.SaveChangesAsync();

        var result = await _sut.GetMyPresentationsAsync();

        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Mi Ponencia");
    }

    // ─── CreateEventAsync – invalid Red ──────────────────────────────────────

    [Test]
    public async Task CreateEventAsync_InvalidRedId_Fails()
    {
        await SeedBaseDataAsync();

        var request = new CreateEventRequest
        {
            Name = "Evento con Red",
            CountryId = 1,
            EventType = 1,
            RedId = "red-inexistente",
            Institutions = new List<string>(),
            AreaIdsPatrocinadoras = new List<string>(),
        };
        var (result, _) = await _sut.CreateEventAsync(request);
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Red"));
    }

    // ─── UpdateEventAsync – invalid Red ──────────────────────────────────────

    [Test]
    public async Task UpdateEventAsync_InvalidRedId_Fails()
    {
        await SeedBaseDataAsync();
        _db.Events.Add(new Event { Id = 100, Name = "Orig", CountryId = 1, EventTypeId = 1, Institutions = new List<Institution>() });
        await _db.SaveChangesAsync();

        var request = new UpdateEventRequest
        {
            Name = "Updated",
            CountryId = 1,
            EventType = 1,
            RedId = "red-inexistente",
            Institutions = new List<string>(),
            AreaIdsPatrocinadoras = new List<string>(),
        };
        var result = await _sut.UpdateEventAsync(100, request);
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Red"));
    }

    // ─── UpdatePresentationAsync – edge cases ────────────────────────────────

    [Test]
    public async Task UpdatePresentationAsync_PresentationNotFound_Fails()
    {
        await SeedBaseDataAsync();
        // Seed an author linked to user-1 so authorId is resolved
        var author = new Author { Id = "a-upd-nf", Name = "X", LastName = "X", SearchKey = "x", LastNameKey = "x", UserId = "user-1" };
        _db.Authors.Add(author);
        await _db.SaveChangesAsync();

        var request = new UpdatePresentationRequest
        {
            Name = "Updated",
            EventId = 1,
            CoauthorIds = new List<string>(),
            CoauthorUserIds = new List<string>(),
            CoauthorNames = new List<string>()
        };
        var result = await _sut.UpdatePresentationAsync(9999, request);
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("encontrada"));
    }

    [Test]
    public async Task UpdatePresentationAsync_NotOwner_Fails()
    {
        await SeedBaseDataAsync();
        var ownerAuthor = new Author { Id = "a-owner", Name = "Owner", LastName = "Owner", SearchKey = "owner", LastNameKey = "owner", UserId = "user-1" };
        var otherAuthor = new Author { Id = "a-other2", Name = "Other", LastName = "Other", SearchKey = "other", LastNameKey = "other" };
        _db.Authors.Add(ownerAuthor);
        _db.Authors.Add(otherAuthor);
        _db.Events.Add(new Event { Id = 200, Name = "E200", CountryId = 1, EventTypeId = 1, Institutions = new List<Institution>() });
        await _db.SaveChangesAsync();

        var pres = new Presentation
        {
            Id = 200,
            Name = "Pres200",
            EventId = 200,
            AuthorPresentations = new List<AuthorPresentation> { new() { AuthorId = "a-other2" } }
        };
        _db.Presentations.Add(pres);
        await _db.SaveChangesAsync();

        var request = new UpdatePresentationRequest
        {
            Name = "Hack",
            EventId = 200,
            CoauthorIds = new List<string>(),
            CoauthorUserIds = new List<string>(),
            CoauthorNames = new List<string>()
        };
        var result = await _sut.UpdatePresentationAsync(200, request);
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("permiso"));
    }

    [Test]
    public async Task UpdatePresentationAsync_InvalidEvent_Fails()
    {
        await SeedBaseDataAsync();
        var author = new Author { Id = "a-inv-evt", Name = "A", LastName = "A", SearchKey = "a", LastNameKey = "a", UserId = "user-1" };
        _db.Authors.Add(author);
        _db.Events.Add(new Event { Id = 300, Name = "E300", CountryId = 1, EventTypeId = 1, Institutions = new List<Institution>() });
        await _db.SaveChangesAsync();

        var pres = new Presentation
        {
            Id = 300,
            Name = "Pres300",
            EventId = 300,
            AuthorPresentations = new List<AuthorPresentation> { new() { AuthorId = "a-inv-evt" } }
        };
        _db.Presentations.Add(pres);
        await _db.SaveChangesAsync();

        var request = new UpdatePresentationRequest
        {
            Name = "Updated",
            EventId = 9999,    // invalid event
            CoauthorIds = new List<string>(),
            CoauthorUserIds = new List<string>(),
            CoauthorNames = new List<string>()
        };
        var result = await _sut.UpdatePresentationAsync(300, request);
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("evento"));
    }

    // ─── CreatePresentationAsync – with coauthors ────────────────────────────

    [Test]
    public async Task CreatePresentationAsync_WithCoauthorById_AddsCoauthor()
    {
        await SeedBaseDataAsync();
        _db.Events.Add(new Event { Id = 400, Name = "E400", CountryId = 1, EventTypeId = 1, Institutions = new List<Institution>() });
        var mainAuthor = new Author { Id = "a-main", Name = "Main", LastName = "Main", SearchKey = "main", LastNameKey = "main" };
        var coAuthor = new Author { Id = "a-co", Name = "Co", LastName = "Author", SearchKey = "co author", LastNameKey = "author" };
        _db.Authors.Add(mainAuthor);
        _db.Authors.Add(coAuthor);
        await _db.SaveChangesAsync();

        _authorResolution.Setup(a => a.GetOrCreateForUserAsync("user-1", default)).ReturnsAsync(mainAuthor);

        var request = new CreatePresentationRequest
        {
            Name = "Pres With Coauthor",
            EventId = 400,
            CoauthorIds = new List<string> { "a-co" },
            CoauthorUserIds = new List<string>(),
            CoauthorNames = new List<string>()
        };
        var (result, id) = await _sut.CreatePresentationAsync(request);
        result.Succeeded.ShouldBeTrue();

        var pres = await _db.Presentations.Include(p => p.AuthorPresentations).FirstAsync(p => p.Id == id);
        pres.AuthorPresentations.Count.ShouldBe(2);
    }

    [Test]
    public async Task CreatePresentationAsync_WithCoauthorByName_AddsCoauthor()
    {
        await SeedBaseDataAsync();
        _db.Events.Add(new Event { Id = 500, Name = "E500", CountryId = 1, EventTypeId = 1, Institutions = new List<Institution>() });
        var mainAuthor = new Author { Id = "a-main2", Name = "Main2", LastName = "Main2", SearchKey = "main2", LastNameKey = "main2" };
        var resolvedCoauthor = new Author { Id = "a-resolved", Name = "Resolved", LastName = "Resolved", SearchKey = "resolved", LastNameKey = "resolved" };
        _db.Authors.Add(mainAuthor);
        await _db.SaveChangesAsync();

        _authorResolution.Setup(a => a.GetOrCreateForUserAsync("user-1", default)).ReturnsAsync(mainAuthor);
        _authorResolution.Setup(a => a.ResolveByNameAsync("García, Juan", default)).ReturnsAsync(resolvedCoauthor);

        var request = new CreatePresentationRequest
        {
            Name = "Pres With Coauthor Name",
            EventId = 500,
            CoauthorIds = new List<string>(),
            CoauthorUserIds = new List<string>(),
            CoauthorNames = new List<string> { "García, Juan" }
        };
        var (result, id) = await _sut.CreatePresentationAsync(request);
        result.Succeeded.ShouldBeTrue();

        var pres = await _db.Presentations.Include(p => p.AuthorPresentations).FirstAsync(p => p.Id == id);
        pres.AuthorPresentations.Count.ShouldBe(2);
    }
}
