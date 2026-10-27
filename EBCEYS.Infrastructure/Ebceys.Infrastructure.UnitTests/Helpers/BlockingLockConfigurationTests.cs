using AwesomeAssertions;
using Ebceys.Infrastructure.Helpers.Multithreading;

namespace Ebceys.Infrastructure.UnitTests.Helpers;

public class BlockingLockConfigurationTests
{
    // ── Constructor ──────────────────────────────────────────────────────────

    [TestCase((ushort)1)]
    [TestCase((ushort)4)]
    [TestCase(ushort.MaxValue)]
    public void When_Constructor_WithValidParallelism_Result_SuccessfullyCreated(ushort parallelism)
    {
        // Act
        var config = new BlockingLockConfiguration(parallelism);

        // Assert
        config.MaxDegreeOfParallelism.Should().Be(parallelism);
        config.ResultsCacheCapacity.Should().BeNull();
    }

    [Test]
    public void When_Constructor_WithZeroParallelism_Result_ArgumentOutOfRangeException()
    {
        // Act
        var act = () => new BlockingLockConfiguration(0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestCase((ushort)1)]
    [TestCase((ushort)10)]
    [TestCase(ushort.MaxValue)]
    public void When_Constructor_WithValidCapacity_Result_SuccessfullyCreated(ushort capacity)
    {
        // Act
        var config = new BlockingLockConfiguration(4, capacity);

        // Assert
        config.MaxDegreeOfParallelism.Should().Be(4);
        config.ResultsCacheCapacity.Should().Be(capacity);
    }

    [Test]
    public void When_Constructor_WithZeroCapacity_Result_ArgumentOutOfRangeException()
    {
        // Act
        var act = () => new BlockingLockConfiguration(4, 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}