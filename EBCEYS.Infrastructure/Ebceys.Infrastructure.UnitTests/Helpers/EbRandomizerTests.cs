using System.Net.Mail;
using System.Security.Claims;
using AwesomeAssertions;
using Ebceys.Infrastructure.Helpers.Jwt;
using Ebceys.Infrastructure.Options;
using Ebceys.Tests.Infrastructure.Helpers;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Ebceys.Infrastructure.UnitTests.Helpers;

public class EbRandomizerTests
{
    private const int Seed = 128439018;
    private EbRandomizer _randomizer;

    [SetUp]
    public void Setup()
    {
        _randomizer = new EbRandomizer(Seed);
    }

    [TestCase(10U)]
    public void When_RandomString_With_SpecifiedNumOfAttempts_Result_ExpectedLength(uint num)
    {
        var prevString = string.Empty;
        for (var i = 1U; i <= num; i++)
        {
            var str = _randomizer.String(i);
            str.Length.Should().Be((int)i);
            str.Should().NotBe(prevString);
            prevString = str;
        }
    }

    [TestCase(int.MinValue, int.MaxValue)]
    [TestCase(0, int.MaxValue)]
    [TestCase(1231, 181685)]
    public void When_RandomInt_With_SpecifiedLimits_Result_ValueInRange(int min, int max)
    {
        var val = _randomizer.Int(min, max);
        val.Should()
            .BeGreaterThanOrEqualTo(min)
            .And
            .BeLessThanOrEqualTo(max);
    }

    [TestCase(double.MinValue / 2, double.MaxValue / 2)]
    [TestCase(0.0, double.MaxValue)]
    [TestCase(1231.0, 181685.0)]
    public void When_RandomDouble_With_SpecifiedLimits_Result_ValueInRange(double min, double max)
    {
        var val = _randomizer.Double(min, max);
        val.Should()
            .BeGreaterThanOrEqualTo(min)
            .And
            .BeLessThanOrEqualTo(max);
    }

    [TestCase(long.MinValue, long.MaxValue)]
    [TestCase(0, long.MaxValue)]
    [TestCase(1231, 181685)]
    public void When_RandomLong_With_SpecifiedLimits_Result_ValueInRange(long min, long max)
    {
        var val = _randomizer.Long(min, max);
        val.Should()
            .BeGreaterThanOrEqualTo(min)
            .And
            .BeLessThanOrEqualTo(max);
    }

    [TestCase(uint.MinValue, uint.MaxValue)]
    [TestCase(0U, uint.MaxValue)]
    [TestCase(1231U, 181685U)]
    public void When_RandomUInt_With_SpecifiedLimits_Result_ValueInRange(uint min, uint max)
    {
        var val = _randomizer.UInt(min, max);
        val.Should()
            .BeGreaterThanOrEqualTo(min)
            .And
            .BeLessThanOrEqualTo(max);
    }

    [TestCaseSource(nameof(GetTestDates))]
    public void When_RandomDateTime_With_SpecifiedLimits_Result_ValueInRange(DateTime min, DateTime max)
    {
        var val = _randomizer.DateTime(min, max);
        val.Should()
            .BeOnOrAfter(min)
            .And
            .BeOnOrBefore(max);
    }

    private static IEnumerable<TestCaseData<DateTime, DateTime>> GetTestDates()
    {
        yield return new TestCaseData<DateTime, DateTime>(DateTime.UnixEpoch, DateTime.MaxValue);
        yield return new TestCaseData<DateTime, DateTime>(new DateTime(1999, 11, 23),
            new DateTime(2452, 6, 5, 10, 42, 12));
    }

    [TestCaseSource(nameof(GetTestOffsetDates))]
    public void When_RandomDateTimeOffset_With_SpecifiedLimits_Result_ValueInRange(DateTimeOffset min,
        DateTimeOffset max)
    {
        var val = _randomizer.DateTimeOffset(min, max);
        val.Should()
            .BeOnOrAfter(min)
            .And
            .BeOnOrBefore(max);
    }

    private static IEnumerable<TestCaseData<DateTimeOffset, DateTimeOffset>> GetTestOffsetDates()
    {
        yield return
            new TestCaseData<DateTimeOffset, DateTimeOffset>(DateTimeOffset.UnixEpoch, DateTimeOffset.MaxValue);
        yield return new TestCaseData<DateTimeOffset, DateTimeOffset>(new DateTime(1999, 11, 23),
            new DateTime(2452, 6, 5, 10, 42, 12));
    }

    [TestCase(50U)]
    public void When_RandomStringArray_With_SpecifiedNumOfAttempts_Result_ExpectedLength(uint num)
    {
        for (var i = 1U; i <= num; i++)
        {
            var array = _randomizer.StringArray(i);
            array.Length.Should().Be((int)i);
        }
    }

    [TestCase(10U, 64U)]
    public void When_RandomStringArray_WithSpecifiedLimits_Result_ValueInRange(uint arrayLength, uint maxStringLength)
    {
        var array = _randomizer.StringArray(arrayLength, maxStringLength);

        array.Length.Should().Be((int)arrayLength);
        array.Aggregate(true, (b, s) => b & (s.Length <= maxStringLength)).Should().BeTrue();
    }

    [TestCase(10U)]
    public void When_RandomIntArray_WithSpecifiedLimits_Result_ValueInRange(uint arrayLength)
    {
        var array = _randomizer.Array<int?>((_, rand) => rand.Int(), arrayLength);

        array.Length.Should().Be((int)arrayLength);
        array.Aggregate(true, (b, i) => b & (i != null)).Should().BeTrue();
    }

    [TestCase(10U, 5U)]
    [TestCase(10U, 11U)]
    public void When_RandomElementsOfCollection_With_SpecifiedCollectionLimits_Result_SpecifiedNumOfElementsWereTaken(
        uint length, uint toTake)
    {
        var randomArray = _randomizer.IntArray(length);

        var act = () => _randomizer.RandomElements(randomArray, toTake);
        if (toTake > length)
        {
            act.Should().Throw<ArgumentOutOfRangeException>();
            return;
        }

        var elements = act().ToArray();
        randomArray.Should().Contain(elements);
        elements.Should().HaveCount((int)toTake);
    }

    [TestCase(10U)]
    [TestCase(100U)]
    public void When_RandomElementOfCollection_With_SpecifiedCollectionLimits_Result_ElementFromCollection(uint length)
    {
        var randomArray = _randomizer.IntArray(length);

        var element = _randomizer.RandomElement(randomArray);

        randomArray.Should().Contain(element);
    }

    [TestCase(10)]
    [TestCase(100)]
    public void When_RandomEnumValue_With_SpecifiedNumOfIterations_Result_ValueIsInEnum(int numOfIterations)
    {
        for (var i = 0; i < numOfIterations; i++)
        {
            var val = _randomizer.Enum<TestEnum>();

            Enum.IsDefined(val).Should().BeTrue();
        }
    }

    [TestCase(5U)]
    public void When_RandomEnumValue_With_Result_ValueIsInEnum(uint numOfElementsToTake)
    {
        var enums = Enum.GetValues<TestEnum>();

        var val = _randomizer.Enums<TestEnum>(numOfElementsToTake);

        enums.Should().Contain(val);
    }

    [TestCaseSource(nameof(GetTestTimeSpans))]
    public void When_RandomTimeSpans_With_SpecifiedMinAndMaxVals_Result_ValidGenerates(
        TimeSpan min, TimeSpan max)
    {
        for (var i = 0; i < 100; i++)
        {
            var random = _randomizer.TimeSpan(min, max);

            random.Should().BeGreaterThanOrEqualTo(min).And.BeLessThanOrEqualTo(max);

            var random2 = _randomizer.TimeSpan(min);
            random2.Should().BeGreaterThanOrEqualTo(min).And.BeLessThanOrEqualTo(TimeSpan.MaxValue);

            var random3 = _randomizer.TimeSpan();
            random3.Should().BeGreaterThanOrEqualTo(TimeSpan.MinValue).And.BeLessThanOrEqualTo(TimeSpan.MaxValue);
        }
    }

    [TestCaseSource(nameof(GetJwts))]
    public void When_RandomJwts_With_SpecifiedClaimsAndOpts_Result_ValidGenerates(
        JwtOptions opts, Claim[] claims)
    {
        var tokenValidator = new JwtValidator(Substitute.For<ILogger<JwtValidator>>());

        var jwt = _randomizer.Jwt(opts, claims);

        var validationResult = tokenValidator.TryValidate(jwt, out var jwtValidationResults);
        validationResult.Should().BeTrue();
        jwtValidationResults!.Issuer.Should().BeEquivalentTo(opts.Issuer);
        jwtValidationResults.Claims.Should().Contain(c => claims.Any(x => x.Value == c.Value && x.Type == c.Type));
        jwtValidationResults.Audiences.Should().Contain(x => x == opts.Audience);
    }

    [TestCase(10000, 0.5f)]
    [TestCase(10000, 0.1f)]
    [TestCase(10000, 0.9f)]
    public void When_RandomBool_With_SpecifiedNumOfAttemptsAndWeight_Result_AproximatlyEqualToMat(int numOfAttempts,
        float weight)
    {
        var bools = Enumerable.Range(0, numOfAttempts).Select(_ => _randomizer.Bool(weight)).ToArray();
        var numOfTrue = bools.Where(x => x).ToArray().Length;

        var weightCalc = Math.Round(Convert.ToDouble(numOfTrue) / Convert.ToDouble(numOfAttempts), 1);

        Math.Abs(weightCalc - weight).Should().BeLessThanOrEqualTo(0.1f);
    }

    [TestCase(1000)]
    public void When_RandomEmail_With_SpecifiedNumOfAttempts_Result_ValidEmails(int numOfAttempts)
    {
        var emails = Enumerable.Range(0, numOfAttempts).Select(_ => _randomizer.Email()).ToArray();

        emails.Should().AllSatisfy(email => IsEmailValid(email).Should().BeTrue());
    }

    [TestCase(10U, 4U, "com")]
    [TestCase(16U, 8U, "net")]
    public void When_RandomEmail_With_GenerationRules_Result_ValidEmails(uint length, uint domainLength, string postfix)
    {
        var email = _randomizer.Email(length, domainLength, postfix);
        var mail = new MailAddress(email);
        mail.Host.Should().HaveLength((int)domainLength + postfix.Length + 1)
            .And.EndWith($".{postfix}");
        mail.User.Should().HaveLength((int)length);
    }

    [Test]
    public void When_RandomEmail_With_NotValidDomain_Result_InvalidOperationException()
    {
        var alphabet = "@#!$%^&*()_+-=<>";
        var rand = new EbRandomizer
        {
            Chars = alphabet
        };
        var domain = rand.String();
        var actDomain = () => rand.Domain(10, domain);
        var actEmail = () => rand.Email(16, 8, domain);
        actDomain.Should().Throw<InvalidOperationException>();
        actEmail.Should().Throw<InvalidOperationException>();
    }

    [TestCase(10000)]
    public void When_RandomHexString_With_SpecifiedNumOfAttempts_Result_ValidHexStrings(int numOfAttempts)
    {
        var hexes = () =>
            Enumerable.Range(0, numOfAttempts).Select(_ => _randomizer.HexString()).Select(Convert.FromHexString)
                .ToArray();

        hexes.Should().NotThrow();
    }

    [TestCase(10000)]
    public void With_RandomUrl_With_SpecifiedNumOfAttempts_Result_ValidAbsoluteUrlStrings(int numOfAttempts)
    {
        var urls = Enumerable.Range(0, numOfAttempts)
            .Select(_ => _randomizer.Url().ToString())
            .ToArray();

        urls.Should().AllSatisfy(url => Uri.TryCreate(url, UriKind.Absolute, out _).Should().BeTrue());
    }

    private static bool IsEmailValid(string address)
    {
        try
        {
            _ = new MailAddress(address);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static IEnumerable<TestCaseData<TimeSpan, TimeSpan>> GetTestTimeSpans()
    {
        yield return new TestCaseData<TimeSpan, TimeSpan>(TimeSpan.Zero, TimeSpan.MaxValue);
        yield return new TestCaseData<TimeSpan, TimeSpan>(TimeSpan.FromHours(1), TimeSpan.FromHours(2));
    }

    private static IEnumerable<TestCaseData<JwtOptions, Claim[]>> GetJwts()
    {
        var random = EbRandomizer.Create();
        yield return new TestCaseData<JwtOptions, Claim[]>(random.JwtOptions(),
            [random.String().ToClaim(random.String())]);
        yield return new TestCaseData<JwtOptions, Claim[]>(random.JwtOptions(),
            random.StringArray(32, 10).Select(x => x.ToClaim(random.String())).ToArray());
    }
}

public enum TestEnum
{
    One = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,
    Nine = 9
}