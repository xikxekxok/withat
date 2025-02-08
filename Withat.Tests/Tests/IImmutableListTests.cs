using NUnit.Framework;
using System;
using System.Collections.Immutable;
using Withat;
namespace Withat.Tests.Tests;

[TestFixture]
public class IImmutableListTests
{
    [Test]
    public void With_PositiveScenario()
    {
        // Arrange
        IImmutableList<int> list = ImmutableList.Create(1, 2, 3);
        int index = 1;
        Func<int, int> valueFunc = x => x * 2;

        // Act
        var result = list.With(index, valueFunc);

        // Assert
        Assert.That(result[0], Is.EqualTo(1));
        Assert.That(result[1], Is.EqualTo(4)); // 2 * 2 = 4
        Assert.That(result[2], Is.EqualTo(3));
    }

    [Test]
    public void With_NegativeScenario_IndexOutOfRange()
    {
        // Arrange
        IImmutableList<int> testedList = ImmutableList.Create(1, 2, 3);
        int index = 5;
        Func<int, int> valueFunc = x => x * 2;

        // Act & Assert
        var exception = Assert.Throws<IndexOutOfRangeException>(() => testedList.With(index, valueFunc));
        Assert.That(exception.Message, Does.Contain("The index 5 is greater than the length of 'testedList' (testedList.length is 3)!"));
    }

    [Test]
    public void WithFirst_PositiveScenario()
    {
        // Arrange
        IImmutableList<int> list = ImmutableList.Create(1, 2, 3);
        Func<int, bool> filter = x => x == 2;
        Func<int, int> valueFunc = x => x * 10;

        // Act
        var result = list.WithFirst(filter, valueFunc);

        // Assert
        Assert.That(result[0], Is.EqualTo(1));
        Assert.That(result[1], Is.EqualTo(20)); // 2 * 10 = 20
        Assert.That(result[2], Is.EqualTo(3));
    }

    [Test]
    public void WithFirst_NegativeScenario_NoMatchingItem()
    {
        // Arrange
        IImmutableList<int> testedList = ImmutableList.Create(1, 2, 3);
        Func<int, int> valueFunc = x => x * 10;

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => testedList.WithFirst(x => x == 5, valueFunc));
        Assert.That(exception.Message, Does.Contain("No item for filter 'x => x == 5' found in 'testedList'!"));
    }

    [Test]
    public void WithFirstOrDefault_PositiveScenario()
    {
        // Arrange
        IImmutableList<int> list = ImmutableList.Create(1, 2, 3);
        Func<int, bool> filter = x => x == 2;
        Func<int, int> valueFunc = x => x * 10;

        // Act
        var result = list.WithFirstOrDefault(filter, valueFunc);

        // Assert
        Assert.That(result[0], Is.EqualTo(1));
        Assert.That(result[1], Is.EqualTo(20)); // 2 * 10 = 20
        Assert.That(result[2], Is.EqualTo(3));
    }

    [Test]
    public void WithFirstOrDefault_NegativeScenario_NoMatchingItem()
    {
        // Arrange
        IImmutableList<int> list = ImmutableList.Create(1, 2, 3);
        Func<int, bool> filter = x => x == 5;
        Func<int, int> valueFunc = x => x * 10;

        // Act
        var result = list.WithFirstOrDefault(filter, valueFunc);

        // Assert
        Assert.That(result, Is.EqualTo(list)); // No changes expected
    }

    [Test]
    public void WithAll_PositiveScenario()
    {
        // Arrange
        IImmutableList<int> list = ImmutableList.Create(1, 2, 3);
        Func<int, bool> filter = x => x > 1;
        Func<int, int> valueFunc = x => x * 10;

        // Act
        var result = list.WithAll(filter, valueFunc);

        // Assert
        Assert.That(result[0], Is.EqualTo(1));
        Assert.That(result[1], Is.EqualTo(20)); // 2 * 10 = 20
        Assert.That(result[2], Is.EqualTo(30)); // 3 * 10 = 30
    }

    [Test]
    public void WithAll_NegativeScenario_NoMatchingItems()
    {
        // Arrange
        IImmutableList<int> list = ImmutableList.Create(1, 2, 3);
        Func<int, bool> filter = x => x > 5;
        Func<int, int> valueFunc = x => x * 10;

        // Act
        var result = list.WithAll(filter, valueFunc);

        // Assert
        Assert.That(result, Is.EqualTo(list)); // No changes expected
    }
}