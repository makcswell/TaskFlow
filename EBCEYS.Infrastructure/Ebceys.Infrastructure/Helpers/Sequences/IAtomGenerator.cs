using System.Numerics;
using JetBrains.Annotations;

namespace Ebceys.Infrastructure.Helpers.Sequences;

/// <summary>
///     Thread-safe atomic generator that produces incrementing <see cref="long" /> values
///     using <see cref="Interlocked.Increment(ref long)" />.
/// </summary>
/// <param name="baseValue">
///     The optional base value for the initial random starting value. If <c>null</c>, uses
///     <see cref="Environment.TickCount" />.
/// </param>
[PublicAPI]
public class AtomicLongGenerator(long? baseValue = null) : IAtomGenerator<long>
{
    private long _currentId = baseValue ?? new Random(Environment.TickCount).NextInt64();

    /// <inheritdoc />
    public long Next()
    {
        return Interlocked.Increment(ref _currentId);
    }
}

/// <summary>
///     Thread-safe atomic generator that produces incrementing <see cref="int" /> values
///     using <see cref="Interlocked.Increment(ref int)" />.
/// </summary>
/// <param name="baseValue">
///     The optional base value for the initial random starting value. If <c>null</c>, uses
///     <see cref="Environment.TickCount" />.
/// </param>
[PublicAPI]
public class AtomicIntGenerator(int? baseValue = null) : IAtomGenerator<int>
{
    private int _currentId = baseValue ?? new Random(Environment.TickCount).Next();

    /// <inheritdoc />
    public int Next()
    {
        return Interlocked.Increment(ref _currentId);
    }
}

/// <summary>
///     Interface for a thread-safe atomic number generator that produces monotonically incrementing values.
/// </summary>
/// <typeparam name="T">The numeric type to generate (must implement <see cref="INumber{T}" />).</typeparam>
[PublicAPI]
public interface IAtomGenerator<out T> where T : struct, INumber<T>
{
    /// <summary>
    ///     Gets the next incremented value in a thread-safe manner.
    /// </summary>
    /// <returns>The next value of type <typeparamref name="T" />.</returns>
    T Next();
}