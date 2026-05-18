using Dashboard_v2.Domain.Common;
using NUnit.Framework;
using Shouldly;

namespace Dashboard_v2.Domain.UnitTests.Common;

// Minimal concrete entity for testing BaseEntity.
file sealed class TestEntity : BaseEntity { }

[TestFixture]
public class BaseEntityTests
{
    [Test]
    public void AddDomainEvent_EventIsTracked()
    {
        var entity = new TestEntity();
        var evt = new Mock_BaseEvent();

        entity.AddDomainEvent(evt);

        entity.DomainEvents.Count.ShouldBe(1);
        entity.DomainEvents.ShouldContain(evt);
    }

    [Test]
    public void RemoveDomainEvent_EventIsRemoved()
    {
        var entity = new TestEntity();
        var evt = new Mock_BaseEvent();
        entity.AddDomainEvent(evt);

        entity.RemoveDomainEvent(evt);

        entity.DomainEvents.ShouldBeEmpty();
    }

    [Test]
    public void ClearDomainEvents_AllEventsAreRemoved()
    {
        var entity = new TestEntity();
        entity.AddDomainEvent(new Mock_BaseEvent());
        entity.AddDomainEvent(new Mock_BaseEvent());

        entity.ClearDomainEvents();

        entity.DomainEvents.ShouldBeEmpty();
    }

    [Test]
    public void DomainEvents_InitiallyEmpty()
    {
        var entity = new TestEntity();
        entity.DomainEvents.ShouldBeEmpty();
    }
}

// Minimal concrete BaseEvent so we don't pull in MediatR just for this.
file sealed class Mock_BaseEvent : BaseEvent { }
