using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Ebceys.Infrastructure.DatabaseRegistration.Conversions;

/// <summary>
///     Value converter for Entity Framework Core that ensures <see cref="DateTimeOffset" /> values
///     are always stored and retrieved in UTC format.
/// </summary>
[PublicAPI]
public class DateTimeOffsetConverter()
    : ValueConverter<DateTimeOffset, DateTimeOffset>(d => d.ToUniversalTime(), d => d.ToUniversalTime());