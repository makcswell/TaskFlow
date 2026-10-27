using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Ebceys.Infrastructure.Helpers.Jwt;
using Ebceys.Infrastructure.Options;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Ebceys.Tests.Infrastructure.Helpers;

/// <summary>
///     Comprehensive random data generator for use in tests. Provides methods for generating random
///     primitives (int, long, byte, double, decimal, bool), strings, hex strings, arrays, emails, domains,
///     dates, timespans, URLs, enums, JWTs, and more. Supports reproducible sequences via seed parameter.
/// </summary>
/// <param name="seed">
///     The seed for the random generator. Use <c>0</c> for a random seed.
/// </param>
[PublicAPI]
public partial class EbRandomizer(int seed = 0)
{
    private const int MaxLengthForStringGeneration = 256;

    private static readonly string DefaultChars =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789" + "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToLower();

    private readonly Random _random = new(seed != 0 ? seed : Random.Shared.Next());

    /// <summary>
    ///     The chars for string generation.
    /// </summary>
    public string Chars { get; set; } = DefaultChars;

    private string HexChars { get; } = "ABCDEF1234567890";

    /// <summary>
    ///     The random jwt generator instance.
    /// </summary>
    public IJwtGenerator RandomJwtGeneratorInstance => new RandomJwtGenerator(JwtOptions());

    /// <summary>
    ///     Creates the new instance of <see cref="EbRandomizer" /> with random seed.
    /// </summary>
    /// <returns>The new instance of <see cref="EbRandomizer" />.</returns>
    public static EbRandomizer Create()
    {
        return new EbRandomizer();
    }

    /// <summary>
    ///     Gets the random string with specified length.
    /// </summary>
    /// <param name="length">The string length. If 0 the random length will be.</param>
    /// <returns>The new instance of <see cref="string" />.</returns>
    public string String(uint length = 0)
    {
        return String(length, Chars);
    }

    /// <summary>
    ///     Gets the random string with specified length and alphabet.
    /// </summary>
    /// <param name="length">The length.</param>
    /// <param name="chars">The alphabet.</param>
    /// <returns>The new random string.</returns>
    private string String(uint length, string chars)
    {
        length = length == 0 ? UInt(0U, byte.MaxValue) : length;
        return new string(
            Enumerable.Repeat(chars, (int)length)
                .Select(s => s[Int(0, s.Length)]).ToArray()
        );
    }

    /// <summary>
    ///     Gets the random hex string with specified length.
    /// </summary>
    /// <param name="length">The string length. If 0 the random length will be.</param>
    /// <returns>The new instance of hex <see cref="string" />.</returns>
    public string HexString(uint length = 0)
    {
        var stringLength = length == 0 ? UInt(0U, byte.MaxValue) : length;
        if (stringLength % 2 == 1)
        {
            stringLength++;
        }

        return String(stringLength, HexChars);
    }

    /// <summary>
    ///     Gets the random int value.
    /// </summary>
    /// <returns>The random int value.</returns>
    public int Int()
    {
        return Int(int.MinValue, int.MaxValue);
    }

    /// <summary>
    ///     Gets the random int value within the range [<paramref name="minValue" />, <see cref="int.MaxValue" />).
    /// </summary>
    /// <param name="minValue">The inclusive lower bound.</param>
    /// <returns>The random int value.</returns>
    public int Int(int minValue)
    {
        return Int(minValue, int.MaxValue);
    }

    /// <summary>
    ///     Gets the random int value within the range [<paramref name="minValue" />, <paramref name="maxValue" />).
    /// </summary>
    /// <param name="minValue">The inclusive lower bound.</param>
    /// <param name="maxValue">The exclusive upper bound.</param>
    /// <returns>The random int value.</returns>
    public int Int(int minValue, int maxValue)
    {
        return _random.Next(minValue, maxValue);
    }

    /// <summary>
    ///     Gets the random uint value within the range [0, <see cref="uint.MaxValue" />].
    /// </summary>
    /// <returns>The random uint value.</returns>
    public uint UInt()
    {
        return UInt(0, uint.MaxValue);
    }

    /// <summary>
    ///     Gets the random uint value within the range [<paramref name="minValue" />, <see cref="uint.MaxValue" />].
    /// </summary>
    /// <param name="minValue">The inclusive lower bound.</param>
    /// <returns>The random uint value.</returns>
    public uint UInt(uint minValue)
    {
        return UInt(minValue, uint.MaxValue);
    }

    /// <summary>
    ///     Gets the random uint value within the range [<paramref name="minValue" />, <paramref name="maxValue" />].
    /// </summary>
    /// <param name="minValue">The inclusive lower bound.</param>
    /// <param name="maxValue">The inclusive upper bound.</param>
    /// <returns>The random uint value.</returns>
    public uint UInt(uint minValue, uint maxValue)
    {
        return (uint)
            (_random.Next((int)minValue, (int)maxValue / 2) +
             _random.Next((int)minValue, (int)maxValue / 2));
    }

    /// <summary>
    ///     Gets the random long value.
    /// </summary>
    /// <returns>The random long value.</returns>
    public long Long()
    {
        return _random.Next();
    }

    /// <summary>
    ///     Gets the random long value within the range [<paramref name="minValue" />, <see cref="long.MaxValue" />).
    /// </summary>
    /// <param name="minValue">The inclusive lower bound.</param>
    /// <returns>The random long value.</returns>
    public long Long(long minValue)
    {
        return Long(minValue, long.MaxValue);
    }

    /// <summary>
    ///     Gets the random long value within the range [<paramref name="minValue" />, <paramref name="maxValue" />).
    /// </summary>
    /// <param name="minValue">The inclusive lower bound.</param>
    /// <param name="maxValue">The exclusive upper bound.</param>
    /// <returns>The random long value.</returns>
    public long Long(long minValue, long maxValue)
    {
        return _random.NextInt64(minValue, maxValue);
    }

    /// <summary>
    ///     Gets the random byte value.
    /// </summary>
    /// <returns>The random byte value.</returns>
    public byte Byte()
    {
        return Byte(byte.MinValue, byte.MaxValue);
    }

    /// <summary>
    ///     Gets a random byte value.
    /// </summary>
    /// <param name="minValue">The inclusive lower bound (unused, kept for API compatibility).</param>
    /// <returns>The random byte value.</returns>
    public byte Byte(long minValue)
    {
        return Byte(minValue, byte.MaxValue);
    }

    /// <summary>
    ///     Gets a random byte value.
    /// </summary>
    /// <param name="minValue">The inclusive lower bound (unused, kept for API compatibility).</param>
    /// <param name="maxValue">The inclusive upper bound (unused, kept for API compatibility).</param>
    /// <returns>The random byte value.</returns>
    public byte Byte(long minValue, long maxValue)
    {
        var bytes = new byte[1];
        _random.NextBytes(bytes);
        return bytes[0];
    }

    /// <summary>
    ///     Gets the random double value.
    /// </summary>
    /// <returns>The random double value.</returns>
    public double Double()
    {
        return Double(double.MinValue / 2, double.MaxValue / 2);
    }

    /// <summary>
    ///     Gets the random double value within the range [<paramref name="minValue" />, <see cref="double.MaxValue" />).
    /// </summary>
    /// <param name="minValue">The inclusive lower bound.</param>
    /// <returns>The random double value.</returns>
    public double Double(double minValue)
    {
        return Double(minValue, double.MaxValue);
    }

    /// <summary>
    ///     Gets the random double value within the range [<paramref name="minValue" />, <paramref name="maxValue" />).
    /// </summary>
    /// <param name="minValue">The inclusive lower bound.</param>
    /// <param name="maxValue">The exclusive upper bound.</param>
    /// <returns>The random double value.</returns>
    public double Double(double minValue, double maxValue)
    {
        return _random.NextDouble() * (maxValue - minValue) + minValue;
    }

    /// <summary>
    ///     Gets the random decimal value.
    /// </summary>
    /// <returns>The random decimal value.</returns>
    public decimal Decimal()
    {
        return Decimal(decimal.MinValue, decimal.MaxValue);
    }

    /// <summary>
    ///     Gets the random decimal value within the range [<paramref name="minValue" />, <see cref="decimal.MaxValue" />).
    /// </summary>
    /// <param name="minValue">The inclusive lower bound.</param>
    /// <returns>The random decimal value.</returns>
    public decimal Decimal(decimal minValue)
    {
        return Decimal(minValue, decimal.MaxValue);
    }

    /// <summary>
    ///     Gets the random decimal value within the range [<paramref name="minValue" />, <paramref name="maxValue" />).
    /// </summary>
    /// <param name="minValue">The inclusive lower bound.</param>
    /// <param name="maxValue">The exclusive upper bound.</param>
    /// <returns>The random decimal value.</returns>
    public decimal Decimal(decimal minValue, decimal maxValue)
    {
        return (decimal)_random.NextSingle() * (maxValue - minValue) + minValue;
    }

    /// <summary>
    ///     Gets the random array of specified type.
    /// </summary>
    /// <param name="length">The array length.</param>
    /// <param name="conf">
    ///     The func generates each element of array.<br />
    ///     The first arg is index of element in array.
    /// </param>
    /// <typeparam name="T"></typeparam>
    /// <returns>The new array of <typeparamref name="T" />.</returns>
    public T[] Array<T>(Func<int, EbRandomizer, T> conf, uint length = 0)
    {
        if (length == 0)
        {
            length = UInt(0, 256);
        }

        return Enumerable.Range(0, (int)length)
            .Select(ind => conf(ind, this)).ToArray();
    }

    /// <summary>
    ///     Gets the random array of strings.
    /// </summary>
    /// <param name="length">The array length.</param>
    /// <param name="stringMaxLength">The max length of string.</param>
    /// <returns>The new array of strings.</returns>
    public string[] StringArray(uint length = 0, uint stringMaxLength = 0)
    {
        return Array<string>(
            (_, rand) => rand.String(UInt(0, stringMaxLength)),
            length);
    }

    /// <summary>
    ///     Gets the random array of longs.
    /// </summary>
    /// <param name="length">The array length.</param>
    /// <returns>The new array of longs.</returns>
    public long[] LongArray(uint length = 0)
    {
        return Array((_, rand) => rand.Long(), length);
    }

    /// <summary>
    ///     Gets the random array of ints.
    /// </summary>
    /// <param name="length">The array length.</param>
    /// <returns>The new array of ints.</returns>
    public int[] IntArray(uint length = 0)
    {
        return Array((_, rand) => rand.Int(), length);
    }

    /// <summary>
    ///     Gets the random array of uints.
    /// </summary>
    /// <param name="length">The array length.</param>
    /// <returns>The new array of uints.</returns>
    public uint[] UIntArray(uint length = 0)
    {
        return Array((_, rand) => rand.UInt(), length);
    }

    /// <summary>
    ///     Gets the random array of doubles.
    /// </summary>
    /// <param name="length">The array length.</param>
    /// <returns>The new array of doubles.</returns>
    public double[] DoubleArray(uint length = 0)
    {
        return Array((_, rand) => rand.Double(), length);
    }

    /// <summary>
    ///     Gets the random array of bytes.
    /// </summary>
    /// <param name="length">The array length.</param>
    /// <returns>The new array of bytes.</returns>
    public byte[] ByteArray(uint length = 0)
    {
        return Array((_, rand) => rand.Byte(), length);
    }

    /// <summary>
    ///     Gets the random array of decimals.
    /// </summary>
    /// <param name="length">The array length.</param>
    /// <returns>The new array of decimals.</returns>
    public decimal[] DecimalArray(uint length = 0)
    {
        return Array((_, rand) => rand.Decimal(), length);
    }

    /// <summary>
    ///     Gets the random bool.
    /// </summary>
    /// <param name="weight">The weight.</param>
    /// <remarks>Weight 0.7 meens that 70% of bool values will be true.</remarks>
    /// <returns>The random bool.</returns>
    public bool Bool(float weight = 0.5f)
    {
        return weight >= Double(0, 1);
    }

    /// <summary>
    ///     Generates a random email address with a random domain.
    /// </summary>
    /// <param name="length">The prefix length.</param>
    /// <param name="domainLength">The domain prefix length.</param>
    /// <param name="postfix">The email postfix. Default is <c>com</c>.</param>
    /// <returns>The new random email string.</returns>
    public string Email(uint length = 16, uint domainLength = 8, string postfix = "com")
    {
        return Email(length, Domain(domainLength, postfix));
    }

    /// <summary>
    ///     The random email with specified domain.
    /// </summary>
    /// <param name="length">The prefix length.</param>
    /// <param name="domain">The domain.</param>
    /// <returns>The new string with random email.</returns>
    /// <exception cref="InvalidOperationException">If domain contains spec symbols.</exception>
    public string Email(uint length, string domain)
    {
        if (!LatAlphabetAndNumsRegex().IsMatch(domain))
        {
            throw new InvalidOperationException("The domain should not contain spec symbols!");
        }

        return $"{String(length, DefaultChars)}@{domain}".ToLower();
    }

    /// <summary>
    ///     The random domain.
    /// </summary>
    /// <param name="length">The domain prefix length.</param>
    /// <param name="postfix">The postfix. Default is <c>com</c>.</param>
    /// <returns>The new string with domain.</returns>
    /// <example>vitaliy.com</example>
    public string Domain(uint length = 8, string postfix = "com")
    {
        if (!LatAlphabetAndNumsRegex().IsMatch(postfix))
        {
            throw new InvalidOperationException("The domain should not contain spec symbols!");
        }

        return $"{String(length, DefaultChars)}.{postfix}".ToLower();
    }

    /// <summary>
    ///     Gets the random datetime.
    /// </summary>
    /// <returns>The random datetime.</returns>
    public DateTime DateTime()
    {
        return DateTime(System.DateTime.UnixEpoch);
    }

    /// <summary>
    ///     Gets the random datetime.
    /// </summary>
    /// <returns>The random datetime.</returns>
    public DateTime DateTime(DateTime minValue)
    {
        return DateTime(minValue, System.DateTime.MaxValue);
    }

    /// <summary>
    ///     Gets the random datetime.
    /// </summary>
    /// <returns>The random datetime.</returns>
    public DateTime DateTime(DateTime minValue, DateTime maxValue)
    {
        var year = Int(0, maxValue.Year + 1);
        var month = Int(0, maxValue.Month + 1);
        var day = Int(0, maxValue.Day + 1);
        var hour = Int(0, maxValue.Hour + 1);
        var minute = Int(0, maxValue.Minute);
        var second = Int(0, maxValue.Second);
        return minValue
            .AddYears(year)
            .AddMonths(month)
            .AddDays(day)
            .AddHours(hour)
            .AddMinutes(minute)
            .AddSeconds(second);
    }

    /// <summary>
    ///     Gets the random DateTimeOffset.
    /// </summary>
    /// <returns>The random DateTimeOffset.</returns>
    public DateTimeOffset DateTimeOffset()
    {
        return DateTimeOffset(System.DateTime.UnixEpoch);
    }

    /// <summary>
    ///     Gets the random DateTimeOffset.
    /// </summary>
    /// <returns>The random DateTimeOffset.</returns>
    public DateTimeOffset DateTimeOffset(DateTimeOffset minValue)
    {
        return DateTimeOffset(minValue, System.DateTime.MaxValue);
    }

    /// <summary>
    ///     Gets the random DateTimeOffset.
    /// </summary>
    /// <returns>The random DateTimeOffset.</returns>
    public DateTimeOffset DateTimeOffset(DateTimeOffset minValue, DateTimeOffset maxValue)
    {
        var year = Int(0, maxValue.Year);
        var month = Int(0, maxValue.Month);
        var day = Int(0, maxValue.Day);
        var hour = Int(0, maxValue.Hour);
        var minute = Int(0, maxValue.Minute);
        var second = Int(0, maxValue.Second);
        return minValue
            .AddYears(year)
            .AddMonths(month)
            .AddDays(day)
            .AddHours(hour)
            .AddMinutes(minute)
            .AddSeconds(second)
            .ToOffset(System.TimeSpan.FromHours(Int(-12, 12)));
    }

    /// <summary>
    ///     Gets a random element from the specified collection.
    /// </summary>
    /// <param name="elements">The source collection.</param>
    /// <typeparam name="T">The element type.</typeparam>
    /// <returns>A random element of type <typeparamref name="T" />.</returns>
    public T RandomElement<T>(IEnumerable<T> elements)
    {
        return RandomElements(elements, 1).First();
    }

    /// <summary>
    ///     Gets random elements from the specified collection.
    /// </summary>
    /// <param name="elements">The source collection.</param>
    /// <param name="count">The number of elements to select.</param>
    /// <typeparam name="T">The element type.</typeparam>
    /// <returns>The new instance of <typeparamref name="T" /> collection.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Throw if <paramref name="count" /> is greater then
    ///     <paramref name="elements" /> length.
    /// </exception>
    public IEnumerable<T> RandomElements<T>(IEnumerable<T> elements, uint count)
    {
        var array = elements.ToArray();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, (uint)array.Length);

        _random.Shuffle(array);
        return array.Take((int)count);
    }

    /// <summary>
    ///     Gets a random enum value.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <returns>The <typeparamref name="TEnum" /> exemplar.</returns>
    public TEnum Enum<TEnum>()
        where TEnum : struct, Enum
    {
        var values = System.Enum.GetValues<TEnum>();
        return RandomElement(values);
    }

    /// <summary>
    ///     Gets random enum values.
    /// </summary>
    /// <param name="count">The number of random enums to get.</param>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <returns>A collection of random <typeparamref name="TEnum" /> values.</returns>
    public IEnumerable<TEnum> Enums<TEnum>(uint count)
        where TEnum : struct, Enum
    {
        var values = System.Enum.GetValues<TEnum>();
        return RandomElements(values, count);
    }

    /// <summary>
    ///     Generates the random timespan.
    /// </summary>
    /// <returns>The new instance of <see cref="System.TimeSpan" />.</returns>
    public TimeSpan TimeSpan(TimeSpan minValue, TimeSpan maxValue)
    {
        return new TimeSpan(Long(minValue.Ticks, maxValue.Ticks));
    }

    /// <summary>
    ///     Generates the random timespan.
    /// </summary>
    /// <returns>The new instance of <see cref="System.TimeSpan" />.</returns>
    public TimeSpan TimeSpan(TimeSpan minValue)
    {
        return TimeSpan(minValue, System.TimeSpan.MaxValue);
    }

    /// <summary>
    ///     Generates the random timespan.
    /// </summary>
    /// <returns>The new instance of <see cref="System.TimeSpan" />.</returns>
    public TimeSpan TimeSpan()
    {
        return TimeSpan(System.TimeSpan.MinValue, System.TimeSpan.MaxValue);
    }

    /// <summary>
    ///     Generates the random JWT with specified <paramref name="opts" />.
    /// </summary>
    /// <returns>The random jwt with prefix <c>Bearer {token}</c>.</returns>
    public string Jwt(JwtOptions opts)
    {
        return Jwt(opts, StringArray().Select(x => x.ToClaim(String())));
    }

    /// <summary>
    ///     Generates the random JWT with specified <paramref name="opts" />.
    /// </summary>
    /// <returns>The random jwt with prefix <c>Bearer {token}</c>.</returns>
    public string Jwt(JwtOptions opts, IEnumerable<Claim> claims)
    {
        var tokenGenerator = new RandomJwtGenerator(opts);
        return Jwt(tokenGenerator, claims);
    }

    /// <summary>
    ///     Generates a JWT token using the specified generator and claims.
    /// </summary>
    /// <param name="generator">The JWT generator.</param>
    /// <param name="claims">The claims to include in the token.</param>
    /// <typeparam name="TJwtGenerator">The JWT generator implementation type.</typeparam>
    /// <returns>The generated JWT string with prefix <c>Bearer {token}</c>.</returns>
    public string Jwt<TJwtGenerator>(TJwtGenerator generator, IEnumerable<Claim> claims)
        where TJwtGenerator : IJwtGenerator
    {
        return generator.GenerateKey(claims);
    }

    /// <summary>
    ///     Generates the random JWT.
    /// </summary>
    /// <returns>The random jwt with prefix <c>Bearer {token}</c>.</returns>
    public string Jwt(params Claim[] claims)
    {
        var opts = JwtOptions();
        return Jwt(opts, claims);
    }

    /// <summary>
    ///     Generates the random JWT.
    /// </summary>
    /// <returns>The random jwt with prefix <c>Bearer {token}</c>.</returns>
    public string Jwt()
    {
        return Jwt(JwtOptions());
    }

    /// <summary>
    ///     Generates the random <see cref="JwtOptions" />.
    /// </summary>
    /// <returns>The new instance of <see cref="JwtOptions" />.</returns>
    public JwtOptions JwtOptions()
    {
        return RandomJwtGenerator.GenerateRandomOptions(this);
    }

    [GeneratedRegex("[a-z,A-Z,0-9]")]
    private static partial Regex LatAlphabetAndNumsRegex();

    /// <summary>
    ///     Generates the random uri.
    /// </summary>
    /// <param name="scheme">The scheme.</param>
    /// <returns>The new <see cref="Url" /> instance.</returns>
    public Uri Url(string scheme = "http")
    {
        var builder = new UriBuilder
        {
            Scheme = scheme,
            Host = String(),
            Path = Bool() ? String() : string.Empty,
            Query = Bool() ? String() : string.Empty,
            Fragment = Bool() ? String() : string.Empty,
            Port = Bool() ? Int(0, 65535) : 0
        };
        return builder.Uri;
    }

    /// <summary>
    ///     Internal JWT generator for tests that creates signed tokens using random <see cref="JwtOptions" />.
    /// </summary>
    /// <param name="options">The JWT options for token generation.</param>
    private class RandomJwtGenerator(JwtOptions options) : IJwtGenerator
    {
        /// <summary>
        ///     The auth schema.
        /// </summary>
        private const string AuthSchema = JwtBearerDefaults.AuthenticationScheme;

        /// <inheritdoc />
        public string GenerateKey(params IEnumerable<Claim> claims)
        {
            DateTime? expires = null;
            if (options.TokenTimeToLive.HasValue && options.TokenTimeToLive.Value > System.TimeSpan.Zero)
            {
                expires = System.DateTime.UtcNow.Add(options.TokenTimeToLive.Value);
            }

            var key = new SymmetricSecurityKey(Convert.FromBase64String(options.Base64Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var jwt = new JwtSecurityToken
            (
                options.Issuer,
                options.Audience,
                claims,
                null,
                expires,
                credentials
            );
            return $"{AuthSchema} {new JwtSecurityTokenHandler().WriteToken(jwt)}";
        }

        /// <summary>
        ///     Generates random <see cref="JwtOptions" /> with random issuer, audience, key, and optional TTL.
        /// </summary>
        /// <param name="randomizer">The randomizer instance for generating values.</param>
        /// <returns>The new instance of <see cref="JwtOptions" /> with random values.</returns>
        public static JwtOptions GenerateRandomOptions(EbRandomizer randomizer)
        {
            return new JwtOptions
            {
                Audience = randomizer.Domain(32),
                Issuer = randomizer.Domain(32),
                Base64Key = randomizer.HexString(128),
                TokenTimeToLive = randomizer.Int(0, 2) % 2 == 0
                    ? null
                    : randomizer.TimeSpan(System.TimeSpan.FromMinutes(15), System.TimeSpan.FromHours(24))
            };
        }
    }
}