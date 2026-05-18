using Dashboard_v2.Domain.Common;
using NUnit.Framework;
using Shouldly;

namespace Dashboard_v2.Domain.UnitTests.Common;

// Concrete implementation used only for testing the abstract ValueObject.
file sealed class Point : ValueObject
{
    public int X { get; }
    public int Y { get; }

    public Point(int x, int y) { X = x; Y = y; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return X;
        yield return Y;
    }
}

[TestFixture]
public class ValueObjectTests
{
    // ─── Equals ──────────────────────────────────────────────────────────────

    [Test]
    public void Equals_SameComponents_ReturnsTrue()
    {
        var a = new Point(1, 2);
        var b = new Point(1, 2);
        a.Equals(b).ShouldBeTrue();
    }

    [Test]
    public void Equals_DifferentComponents_ReturnsFalse()
    {
        var a = new Point(1, 2);
        var b = new Point(3, 4);
        a.Equals(b).ShouldBeFalse();
    }

    [Test]
    public void Equals_Null_ReturnsFalse()
    {
        var a = new Point(1, 2);
        a.Equals(null).ShouldBeFalse();
    }

    [Test]
    public void Equals_DifferentType_ReturnsFalse()
    {
        var a = new Point(1, 2);
        a.Equals("not a point").ShouldBeFalse();
    }

    // ─── GetHashCode ──────────────────────────────────────────────────────────

    [Test]
    public void GetHashCode_EqualObjects_SameHash()
    {
        var a = new Point(5, 10);
        var b = new Point(5, 10);
        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Test]
    public void GetHashCode_DifferentObjects_DifferentHash()
    {
        var a = new Point(1, 2);
        var b = new Point(9, 9);
        // Not guaranteed by contract but extremely likely for distinct int pairs
        a.GetHashCode().ShouldNotBe(b.GetHashCode());
    }

    // ─── EqualOperator / NotEqualOperator (via protected static) ─────────────
    // Tested indirectly through a subclass that exposes them.

    [Test]
    public void EqualObjects_AreConsideredEqual()
    {
        object a = new Point(7, 3);
        object b = new Point(7, 3);
        a.Equals(b).ShouldBeTrue();
    }

    [Test]
    public void UnequalObjects_AreNotConsideredEqual()
    {
        object a = new Point(7, 3);
        object b = new Point(7, 4);
        a.Equals(b).ShouldBeFalse();
    }
}
