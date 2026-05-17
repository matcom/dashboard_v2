using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Publications;
using Dashboard_v2.Domain.Entities;
using Dashboard_v2.Domain.Enums;
using Dashboard_v2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Dashboard_v2.Application.UnitTests.Publications;

[TestFixture]
public class PublicationServiceTests
{
    private ApplicationDbContext _db = null!;
    private Mock<IUser> _currentUser = null!;
    private Mock<ICrossRefClient> _crossRefClient = null!;
    private Mock<IOpenAireClient> _openAireClient = null!;
    private Mock<IAuthorResolutionService> _authorResolution = null!;
    private Mock<IAuthorCleanupService> _authorCleanup = null!;
    private PublicationService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(options);
        _currentUser = new Mock<IUser>();
        _currentUser.Setup(u => u.Id).Returns("user-1");
        _currentUser.Setup(u => u.Roles).Returns(new List<string> { "Profesor" });
        _crossRefClient = new Mock<ICrossRefClient>();
        _openAireClient = new Mock<IOpenAireClient>();
        _authorResolution = new Mock<IAuthorResolutionService>();
        _authorCleanup = new Mock<IAuthorCleanupService>();

        _sut = new PublicationService(_db, _currentUser.Object, _crossRefClient.Object,
            _openAireClient.Object, _authorResolution.Object, _authorCleanup.Object);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    // ─── GetAllPublicationsAsync ───────────────────────────────────────────────

    [Test]
    public async Task GetAllPublicationsAsync_Empty_ReturnsEmpty()
    {
        var result = await _sut.GetAllPublicationsAsync();
        result.ShouldBeEmpty();
    }

    // ─── GetMyPublicationsAsync ───────────────────────────────────────────────

    [Test]
    public async Task GetMyPublicationsAsync_NoLinkedAuthor_ReturnsEmpty()
    {
        var result = await _sut.GetMyPublicationsAsync();
        result.ShouldBeEmpty();
    }

    // ─── GetAreaPublicationsAsync ─────────────────────────────────────────────

    [Test]
    public async Task GetAreaPublicationsAsync_UserWithNoArea_ReturnsEmpty()
    {
        _db.Users.Add(new User { Id = "user-1", UserName = "test", Email = "t@t.com", UserLastName1 = "T" });
        await _db.SaveChangesAsync();

        var result = await _sut.GetAreaPublicationsAsync();
        result.ShouldBeEmpty();
    }

    // ─── GetPublicationTypesAsync ─────────────────────────────────────────────

    [Test]
    public async Task GetPublicationTypesAsync_ReturnsAllTypes()
    {
        var result = await _sut.GetPublicationTypesAsync();
        result.ShouldNotBeEmpty();
    }

    // ─── GetMyRedPublicationsAsync ────────────────────────────────────────────

    [Test]
    public async Task GetMyRedPublicationsAsync_AsNonJefe_NoCoordinatedReds_ReturnsEmpty()
    {
        var result = await _sut.GetMyRedPublicationsAsync();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GetMyRedPublicationsAsync_AsJefeDeRedes_ReturnsAllWithRed()
    {
        _currentUser.Setup(u => u.Roles).Returns(new List<string> { "Jefe_de_Redes" });

        var pub = new Publication
        {
            Id = Guid.NewGuid().ToString(), Title = "Pub con Red",
            NormalizedTitle = "pub con red", PublishedDate = "2024",
            PublicationData = "{}",
            RedId = Guid.NewGuid().ToString(),
            AuthorPublications = new List<AuthorPublication>()
        };
        _db.Publications.Add(pub);
        await _db.SaveChangesAsync();

        var result = await _sut.GetMyRedPublicationsAsync();
        result.Count.ShouldBe(1);
    }

    // ─── FindDuplicatesAsync ─────────────────────────────────────────────────

    [Test]
    public async Task FindDuplicatesAsync_NullInputs_ReturnsEmpty()
    {
        var result = await _sut.FindDuplicatesAsync(null, null, null);
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task FindDuplicatesAsync_ByTitle_FindsMatch()
    {
        var pub = new Publication
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Machine Learning en Cuba",
            NormalizedTitle = "machine learning en cuba",
            PublishedDate = "2024",
            PublicationData = "{}",
            AuthorPublications = new List<AuthorPublication>()
        };
        _db.Publications.Add(pub);
        await _db.SaveChangesAsync();

        var result = await _sut.FindDuplicatesAsync("Machine Learning en Cuba", null, null);
        result.ShouldHaveSingleItem();
        result[0].MatchType.ShouldBe("title");
    }

    [Test]
    public async Task FindDuplicatesAsync_ByDoi_FindsMatch()
    {
        var doi = "10.1000/xyz123";
        var pub = new Publication
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Some Paper",
            NormalizedTitle = "some paper",
            UrlDoi = doi,
            NormalizedUrlDoi = doi,
            PublishedDate = "2024",
            PublicationData = "{}",
            AuthorPublications = new List<AuthorPublication>()
        };
        _db.Publications.Add(pub);
        await _db.SaveChangesAsync();

        var result = await _sut.FindDuplicatesAsync(null, doi, null);
        result.ShouldHaveSingleItem();
        result[0].MatchType.ShouldBe("doi");
    }

    [Test]
    public async Task FindDuplicatesAsync_ExcludeId_ExcludesPublication()
    {
        var id = Guid.NewGuid().ToString();
        var pub = new Publication
        {
            Id = id,
            Title = "Same Title",
            NormalizedTitle = "same title",
            PublishedDate = "2024",
            PublicationData = "{}",
            AuthorPublications = new List<AuthorPublication>()
        };
        _db.Publications.Add(pub);
        await _db.SaveChangesAsync();

        var result = await _sut.FindDuplicatesAsync("Same Title", null, null, id);
        result.ShouldBeEmpty();
    }

    // ─── SearchCrossRefCandidatesAsync ────────────────────────────────────────

    [Test]
    public async Task SearchCrossRefCandidatesAsync_NullInputs_ReturnsEmpty()
    {
        var result = await _sut.SearchCrossRefCandidatesAsync(null, null);
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task SearchCrossRefCandidatesAsync_ByTitle_CallsCrossRefSearch()
    {
        _crossRefClient.Setup(c => c.SearchWorksByTitleAsync("test", 10, default))
            .ReturnsAsync(new List<PublicationCrossRefDto>());

        var result = await _sut.SearchCrossRefCandidatesAsync(null, "test");
        _crossRefClient.Verify(c => c.SearchWorksByTitleAsync("test", 10, default), Times.Once);
    }

    // ─── SearchOpenAireCandidatesAsync ────────────────────────────────────────

    [Test]
    public async Task SearchOpenAireCandidatesAsync_NullInputs_ReturnsEmpty()
    {
        var result = await _sut.SearchOpenAireCandidatesAsync(null, null);
        result.ShouldBeEmpty();
    }

    // ─── DeleteAsync ──────────────────────────────────────────────────────────

    [Test]
    public async Task DeleteAsync_UserNotAuthor_Fails()
    {
        var result = await _sut.DeleteAsync("non-existing-id");
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("no tienes permiso") || e.Contains("no encontrado"));
    }

    // ─── GetByIdAsync ─────────────────────────────────────────────────────────

    [Test]
    public async Task GetByIdAsync_NoLinkedAuthor_ReturnsNull()
    {
        var result = await _sut.GetByIdAsync("any-id");
        result.ShouldBeNull();
    }

    // ─── AddCurrentUserAsCoauthorAsync ───────────────────────────────────────

    [Test]
    public async Task AddCurrentUserAsCoauthorAsync_AuthorResolutionReturnsNull_Fails()
    {
        _authorResolution.Setup(a => a.GetOrCreateForUserAsync("user-1", default))
            .ReturnsAsync((Author?)null);

        var result = await _sut.AddCurrentUserAsCoauthorAsync("pub-id");
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("no encontrado"));
    }

    [Test]
    public async Task AddCurrentUserAsCoauthorAsync_PublicationNotFound_Fails()
    {
        var author = new Author { Id = "a1", LastName = "X", Name = "X", SearchKey = "x", LastNameKey = "x" };
        _authorResolution.Setup(a => a.GetOrCreateForUserAsync("user-1", default))
            .ReturnsAsync(author);

        var result = await _sut.AddCurrentUserAsCoauthorAsync("nonexistent-pub");
        result.Succeeded.ShouldBeFalse();
    }

    // ─── CreateAsync Validations ──────────────────────────────────────────────

    [Test]
    public async Task CreateAsync_InvalidPublicationType_Fails()
    {
        var request = new CreatePublicationRequest
        {
            Title = "Test",
            PublishedDate = "2024",
            PublicationType = (PublicationType)999,
            AdditionalAuthorIds = new List<string>(),
            AdditionalAuthorNames = new List<string>(),
            AdditionalUserIds = new List<string>()
        };
        var (result, _) = await _sut.CreateAsync(request);
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Tipo") || e.Contains("válido"));
    }

    [Test]
    public async Task CreateAsync_InvalidDate_Fails()
    {
        var request = new CreatePublicationRequest
        {
            Title = "Test",
            PublishedDate = "not-a-date",
            PublicationType = PublicationType.Libro,
            Index = 1,
            AdditionalAuthorIds = new List<string>(),
            AdditionalAuthorNames = new List<string>(),
            AdditionalUserIds = new List<string>()
        };
        var (result, _) = await _sut.CreateAsync(request);
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("fecha"));
    }

    [Test]
    public async Task CreateAsync_Diario_MissingGroup_Fails()
    {
        var request = new CreatePublicationRequest
        {
            Title = "Test",
            PublishedDate = "2024",
            PublicationType = PublicationType.Diario,
            Group = null,
            AdditionalAuthorIds = new List<string>(),
            AdditionalAuthorNames = new List<string>(),
            AdditionalUserIds = new List<string>()
        };
        var (result, _) = await _sut.CreateAsync(request);
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("grupo"));
    }

    [Test]
    public async Task CreateAsync_Libro_MissingIndex_Fails()
    {
        var request = new CreatePublicationRequest
        {
            Title = "Test",
            PublishedDate = "2024",
            PublicationType = PublicationType.Libro,
            Index = null,
            AdditionalAuthorIds = new List<string>(),
            AdditionalAuthorNames = new List<string>(),
            AdditionalUserIds = new List<string>()
        };
        var (result, _) = await _sut.CreateAsync(request);
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("indexaci"));
    }

    [Test]
    public async Task CreateAsync_ArticuloDivulgacion_NoIndexNeeded_AuthorResolutionFails()
    {
        _authorResolution.Setup(a => a.GetOrCreateForUserAsync("user-1", default))
            .ReturnsAsync((Author?)null);

        var request = new CreatePublicationRequest
        {
            Title = "Test",
            PublishedDate = "2024",
            PublicationType = PublicationType.Artículo_de_Divulgación,
            AdditionalAuthorIds = new List<string>(),
            AdditionalAuthorNames = new List<string>(),
            AdditionalUserIds = new List<string>()
        };
        var (result, _) = await _sut.CreateAsync(request);
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("no encontrado"));
    }

    // ─── GetPublicByIdAsync ───────────────────────────────────────────────────

    [Test]
    public async Task GetPublicByIdAsync_NotFound_ReturnsNull()
    {
        var result = await _sut.GetPublicByIdAsync("nonexistent-id");
        result.ShouldBeNull();
    }

    [Test]
    public async Task GetPublicByIdAsync_Found_ReturnsDtoWithTitle()
    {
        var pubId = Guid.NewGuid().ToString();
        _db.Publications.Add(new Publication
        {
            Id = pubId,
            Title = "Pub Pública",
            NormalizedTitle = "pub publica",
            PublishedDate = "2023",
            PublicationData = "{}",
            AuthorPublications = new List<AuthorPublication>()
        });
        await _db.SaveChangesAsync();

        var result = await _sut.GetPublicByIdAsync(pubId);
        result.ShouldNotBeNull();
        result!.Title.ShouldBe("Pub Pública");
    }

    // ─── GetAllPublicationsAsync (with data) ──────────────────────────────────

    [Test]
    public async Task GetAllPublicationsAsync_WithData_ReturnsOrdered()
    {
        _db.Publications.AddRange(
            new Publication { Id = "p1", Title = "Zebra", NormalizedTitle = "zebra", PublishedDate = "2022", PublicationData = "{}", AuthorPublications = new List<AuthorPublication>() },
            new Publication { Id = "p2", Title = "Alpha", NormalizedTitle = "alpha", PublishedDate = "2022", PublicationData = "{}", AuthorPublications = new List<AuthorPublication>() }
        );
        await _db.SaveChangesAsync();

        var result = await _sut.GetAllPublicationsAsync();
        result.Count.ShouldBe(2);
        result[0].Title.ShouldBe("Alpha");
    }
}
