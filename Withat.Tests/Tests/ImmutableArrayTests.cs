using System.Collections.Immutable;
using Withat;

namespace Withat.Tests.Tests;

[TestFixture]
public class ImmutableArrayTests
{
    [Test]
    public void With_PositiveScenario()
    {
        // Arrange
        var array = ImmutableArray.Create(1, 2, 3);
        int index = 1;
        Func<int, int> valueFunc = x => x * 2;

        // Act
        var result = array.With(index, valueFunc);

        // Assert
        Assert.That(result[0], Is.EqualTo(1));
        Assert.That(result[1], Is.EqualTo(4)); // 2 * 2 = 4
        Assert.That(result[2], Is.EqualTo(3));
    }

    [Test]
    public void With_NegativeScenario_IndexOutOfRange()
    {
        // Arrange
        var testedArray = ImmutableArray.Create(1, 2, 3);
        int index = 4;
        Func<int, int> valueFunc = x => x * 2;

        // Act & Assert
        var exception = Assert.Throws<IndexOutOfRangeException>(() => testedArray.With(index, valueFunc));
        Assert.That(exception.Message, Does.Contain("The index 4 is greater than the length of 'testedArray' (testedArray.length is 3)!"));
    }

    [Test]
    public void WithFirst_PositiveScenario()
    {
        // Arrange
        var array = ImmutableArray.Create(1, 2, 3);
        Func<int, bool> filter = x => x == 2;
        Func<int, int> valueFunc = x => x * 10;

        // Act
        var result = array.WithFirst(filter, valueFunc);

        // Assert
        Assert.That(result[0], Is.EqualTo(1));
        Assert.That(result[1], Is.EqualTo(20)); // 2 * 10 = 20
        Assert.That(result[2], Is.EqualTo(3));
    }

    [Test]
    public void WithFirst_NegativeScenario_NoMatchingItem()
    {
        // Arrange
        var testedArray = ImmutableArray.Create(1, 2, 3);
        Func<int, int> valueFunc = x => x * 10;

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => testedArray.WithFirst(x => x == 5, valueFunc));
        Assert.That(exception.Message, Does.Contain("No item for filter 'x => x == 5' found in 'testedArray'!"));
    }

    [Test]
    public void WithFirstOrDefault_PositiveScenario()
    {
        // Arrange
        var array = ImmutableArray.Create(1, 2, 3);
        Func<int, bool> filter = x => x == 2;
        Func<int, int> valueFunc = x => x * 10;

        // Act
        var result = array.WithFirstOrDefault(filter, valueFunc);

        // Assert
        Assert.That(result[0], Is.EqualTo(1));
        Assert.That(result[1], Is.EqualTo(20)); // 2 * 10 = 20
        Assert.That(result[2], Is.EqualTo(3));
    }

    [Test]
    public void WithFirstOrDefault_NegativeScenario_NoMatchingItem()
    {
        // Arrange
        var array = ImmutableArray.Create(1, 2, 3);
        Func<int, bool> filter = x => x == 5;
        Func<int, int> valueFunc = x => x * 10;

        // Act
        var result = array.WithFirstOrDefault(filter, valueFunc);

        // Assert
        Assert.That(result, Is.EqualTo(array)); // No changes expected
    }

    [Test]
    public void WithAll_PositiveScenario()
    {
        // Arrange
        var array = ImmutableArray.Create(1, 2, 3);
        Func<int, bool> filter = x => x > 1;
        Func<int, int> valueFunc = x => x * 10;

        // Act
        var result = array.WithAll(filter, valueFunc);

        // Assert
        Assert.That(result[0], Is.EqualTo(1));
        Assert.That(result[1], Is.EqualTo(20)); // 2 * 10 = 20
        Assert.That(result[2], Is.EqualTo(30)); // 3 * 10 = 30
    }

    [Test]
    public void WithAll_NegativeScenario_NoMatchingItems()
    {
        // Arrange
        var array = ImmutableArray.Create(1, 2, 3);
        Func<int, bool> filter = x => x > 5;
        Func<int, int> valueFunc = x => x * 10;

        // Act
        var result = array.WithAll(filter, valueFunc);

        // Assert
        Assert.That(result, Is.EqualTo(array)); // No changes expected
    }
}