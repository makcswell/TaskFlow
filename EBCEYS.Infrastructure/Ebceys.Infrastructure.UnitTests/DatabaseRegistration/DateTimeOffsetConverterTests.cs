using AwesomeAssertions;
using Ebceys.Infrastructure.DatabaseRegistration.Conversions;
using Ebceys.Tests.Infrastructure.Helpers;

namespace Ebceys.Infrastructure.UnitTests.DatabaseRegistration;

public class DateTimeOffsetConverterTests
{
    private static readonly EbRandomizer Randomizer = new();

    private readonly DateTimeOffsetConverter _converter = new();

    private Func<DateTimeOffset, DateTimeOffset> ConvertToProvider =>
        _converter.ConvertToProviderExpression.Compile();

    private Func<DateTimeOffset, DateTimeOffset> ConvertFromProvider =>
        _converter.ConvertFromProviderExpression.Compile();

    [Test]
    public void When_ConvertToProvider_With_UtcValue_Result_SameValue()
    {
        var utcValue = Randomizer.DateTimeOffset(DateTimeOffset.UnixEpoch,
            DateTimeOffset.UnixEpoch.AddYears(1000));
        utcValue = new DateTimeOffset(utcValue.DateTime, TimeSpan.Zero);

        var result = ConvertToProvider(utcValue);

        result.Offset.Should().Be(TimeSpan.Zero);
        result.UtcTicks.Should().Be(utcValue.UtcTicks);
    }

    [Test]
    public void When_ConvertToProvider_With_OffsetValue_Result_ConvertedToUtc()
    {
        var localOffset = TimeSpan.FromHours(3);
        var localTime = new DateTimeOffset(2025, 6, 15, 12, 0, 0, localOffset);

        var result = ConvertToProvider(localTime);

        result.Offset.Should().Be(TimeSpan.Zero);
        result.UtcTicks.Should().Be(localTime.UtcTicks);
    }

    [Test]
    public void When_ConvertFromProvider_With_UtcValue_Result_UtcValue()
    {
        var utcValue = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var result = ConvertFromProvider(utcValue);

        result.Offset.Should().Be(TimeSpan.Zero);
        result.UtcTicks.Should().Be(utcValue.UtcTicks);
    }

    [Test]
    public void When_RoundTrip_With_ArbitraryOffset_Result_UtcTicksPreserved()
    {
        var original = new DateTimeOffset(2024, 3, 10, 15, 30, 0, TimeSpan.FromHours(5));

        var stored = ConvertToProvider(original);
        var restored = ConvertFromProvider(stored);

        restored.UtcTicks.Should().Be(original.UtcTicks);
        restored.Offset.Should().Be(TimeSpan.Zero);
    }

    [TestCaseSource(nameof(GetDateTimeOffsets))]
    public void When_ConvertToProvider_With_VariousOffsets_Result_AlwaysUtc(DateTimeOffset value)
    {
        var result = ConvertToProvider(value);

        result.Offset.Should().Be(TimeSpan.Zero);
        result.UtcTicks.Should().Be(value.UtcTicks);
    }

    private static IEnumerable<TestCaseData<DateTimeOffset>> GetDateTimeOffsets()
    {
        yield return new TestCaseData<DateTimeOffset>(new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero));
        yield return new TestCaseData<DateTimeOffset>(new DateTimeOffset(2000, 1, 1, 12, 0, 0, TimeSpan.FromHours(-5)));
        yield return new TestCaseData<DateTimeOffset>(
            new DateTimeOffset(2000, 1, 1, 23, 59, 59, TimeSpan.FromHours(14)));
        yield return new TestCaseData<DateTimeOffset>(DateTimeOffset.UnixEpoch);
    }
}