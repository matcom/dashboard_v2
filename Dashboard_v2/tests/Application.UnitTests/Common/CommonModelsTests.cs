using Dashboard_v2.Application.Common.Exceptions;
using Dashboard_v2.Application.Common.Models;
using Dashboard_v2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shouldly;

namespace Dashboard_v2.Application.UnitTests.Common;

[TestFixture]
public class PaginatedListTests
{
    private ApplicationDbContext _db = null!;

    [SetUp]
    public void SetUp()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(opts);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    [Test]
    public void Constructor_SetsPropertiesCorrectly()
    {
        var items = new List<int> { 1, 2, 3 };
        var list = new PaginatedList<int>(items, count: 30, pageNumber: 2, pageSize: 10);

        list.Items.Count.ShouldBe(3);
        list.PageNumber.ShouldBe(2);
        list.TotalCount.ShouldBe(30);
        list.TotalPages.ShouldBe(3);
    }

    [Test]
    public void HasPreviousPage_FirstPage_ReturnsFalse()
    {
        var list = new PaginatedList<int>(new List<int>(), count: 10, pageNumber: 1, pageSize: 10);
        list.HasPreviousPage.ShouldBeFalse();
    }

    [Test]
    public void HasPreviousPage_SecondPage_ReturnsTrue()
    {
        var list = new PaginatedList<int>(new List<int>(), count: 20, pageNumber: 2, pageSize: 10);
        list.HasPreviousPage.ShouldBeTrue();
    }

    [Test]
    public void HasNextPage_LastPage_ReturnsFalse()
    {
        var list = new PaginatedList<int>(new List<int>(), count: 10, pageNumber: 1, pageSize: 10);
        list.HasNextPage.ShouldBeFalse();
    }

    [Test]
    public void HasNextPage_NotLastPage_ReturnsTrue()
    {
        var list = new PaginatedList<int>(new List<int>(), count: 20, pageNumber: 1, pageSize: 10);
        list.HasNextPage.ShouldBeTrue();
    }

    [Test]
    public void TotalPages_CeilsDivision()
    {
        var list = new PaginatedList<int>(new List<int>(), count: 21, pageNumber: 1, pageSize: 10);
        list.TotalPages.ShouldBe(3);
    }

    [Test]
    public async Task CreateAsync_ReturnsPaginatedSlice()
    {
        for (int i = 1; i <= 15; i++)
            _db.Areas.Add(new Domain.Entities.Area { Id = $"a{i}", Nombre = $"Area{i:D2}" });
        await _db.SaveChangesAsync();

        var query = _db.Areas.OrderBy(a => a.Nombre).Select(a => a.Id);
        var page = await PaginatedList<string>.CreateAsync(query, pageNumber: 2, pageSize: 5);

        page.Items.Count.ShouldBe(5);
        page.PageNumber.ShouldBe(2);
        page.TotalCount.ShouldBe(15);
        page.TotalPages.ShouldBe(3);
        page.HasPreviousPage.ShouldBeTrue();
        page.HasNextPage.ShouldBeTrue();
    }
}

[TestFixture]
public class ForbiddenAccessExceptionTests
{
    [Test]
    public void DefaultConstructor_CreatesException()
    {
        var ex = new ForbiddenAccessException();
        ex.ShouldNotBeNull();
        ex.ShouldBeOfType<ForbiddenAccessException>();
    }

    [Test]
    public void IsException()
    {
        var ex = new ForbiddenAccessException();
        (ex is Exception).ShouldBeTrue();
    }
}

[TestFixture]
public class ResultTests
{
    [Test]
    public void Success_HasSucceededTrue_AndNoErrors()
    {
        var result = Result.Success();
        result.Succeeded.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public void Failure_HasSucceededFalse_AndErrors()
    {
        var result = Result.Failure(["Error 1", "Error 2"]);
        result.Succeeded.ShouldBeFalse();
        result.Errors.Length.ShouldBe(2);
        result.Errors.ShouldContain("Error 1");
    }
}
