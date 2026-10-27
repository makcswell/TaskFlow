using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Ebceys.Infrastructure.Helpers.Json;
using JetBrains.Annotations;

namespace Ebceys.Infrastructure.Extensions;

/// <summary>
///     The <see cref="object" /> extensions.
/// </summary>
[PublicAPI]
public static class EbObjectExtensions
{
    /// <summary>
    ///     Casts the <paramref name="value" /> to <see cref="uint" /> value.
    /// </summary>
    /// <param name="value">The value to cast.</param>
    /// <returns>The value of <see cref="uint" />.</returns>
    [Pure]
    public static uint ToUInt(this int value)
    {
        return (uint)value;
    }

    /// <summary>
    ///     Casts the <paramref name="value" /> to <see cref="ulong" /> value.
    /// </summary>
    /// <param name="value">The value to cast.</param>
    /// <returns>The value of <see cref="ulong" />.</returns>
    [Pure]
    public static ulong ToULong(this long value)
    {
        return (ulong)value;
    }

    /// <param name="obj">The object.</param>
    extension(object obj)
    {
        /// <summary>
        ///     Serializes the <paramref name="obj" /> to diagnostic JSON string.
        /// </summary>
        /// <returns>The serialized JSON string.</returns>
        [Pure]
        public string ToDiagnosticJson()
        {
            var json = JsonSerializer.Serialize(obj, DefaultJsonSerializerOptions.DiagnosticJsonOptions);
            return json;
        }

        /// <summary>
        ///     Serializes the <paramref name="obj" /> to default JSON string.
        /// </summary>
        /// <returns>The serialized JSON string.</returns>
        [Pure]
        public string ToJson()
        {
            var json = JsonSerializer.Serialize(obj, DefaultJsonSerializerOptions.DefaultJsonOptions);
            return json;
        }
    }

    extension(string? val)
    {
        /// <summary>
        ///     Indicates that <paramref name="val" /> is null or empty.
        /// </summary>
        /// <returns>true if string is null or empty.</returns>
        [MemberNotNullWhen(false)]
        [Pure]
        public bool IsNullOrEmpty()
        {
            return string.IsNullOrEmpty(val);
        }

        /// <summary>
        ///     Indicates that <paramref name="val" /> is null or white space.
        /// </summary>
        /// <returns>true if string is null or white space.</returns>
        [MemberNotNullWhen(false)]
        [Pure]
        public bool IsNullOrWhiteSpace()
        {
            return string.IsNullOrWhiteSpace(val);
        }
    }

    extension(IEnumerable<string> strings)
    {
        /// <summary>
        ///     Joins the string with specified <paramref name="separator" />.
        /// </summary>
        /// <param name="separator">The separator.</param>
        /// <returns>The joined strings.</returns>
        [Pure]
        public string Join(string separator)
        {
            return string.Join(separator, strings);
        }
    }

    /// <param name="enumerable">The enumerable.</param>
    /// <typeparam name="T">The item in <paramref name="enumerable" /> type.</typeparam>
    extension<T>(IEnumerable<T> enumerable)
    {
        /// <summary>
        ///     Foreaches the <paramref name="enumerable" /> and executes the specified <paramref name="action" /> for each item.
        /// </summary>
        /// <param name="action">
        ///     The action that will be applied for each <typeparamref name="T" /> if
        ///     <paramref name="enumerable" />.
        /// </param>
        public void Foreach(Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }
        }

        /// <summary>
        ///     Foreaches the <paramref name="enumerable" /> and executes the specified <paramref name="action" /> for each item
        ///     with yield return.
        /// </summary>
        /// <param name="action">
        ///     The action that will be applied for each <typeparamref name="T" /> if
        ///     <paramref name="enumerable" />.
        /// </param>
        /// <returns>The <paramref name="enumerable" /> with applied <paramref name="action" /> for each element.</returns>
        [Pure]
        public IEnumerable<T> ForeachLazy(Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
                yield return item;
            }
        }

        /// <summary>
        ///     Foreaches the <paramref name="enumerable" /> and executes the specified asynchronous <paramref name="action" /> for
        ///     each item.
        /// </summary>
        /// <param name="action">The function that will be applied for each element in <paramref name="enumerable" />.</param>
        public async Task ForeachAsync(Func<T, Task> action)
        {
            foreach (var item in enumerable)
            {
                await action(item);
            }
        }

        /// <summary>
        ///     Foreaches the <paramref name="enumerable" /> and executes the specified asynchronous <paramref name="action" /> for
        ///     each item with yield return.
        /// </summary>
        /// <param name="action">The function that will be applied for each element in <paramref name="enumerable" />.</param>
        /// <returns>The <paramref name="enumerable" /> with applied <paramref name="action" /> for each element.</returns>
        /// \
        [Pure]
        public async IAsyncEnumerable<T> ForeachLazyAsync(Func<T, Task> action)
        {
            foreach (var item in enumerable)
            {
                await action(item);
                yield return item;
            }
        }
    }

    /// <param name="enumerable">The enumerable.</param>
    /// <typeparam name="T">The type of elements in <paramref name="enumerable" />.</typeparam>
    extension<T>(IEnumerable<T>? enumerable)
    {
        /// <summary>
        ///     Indicates that <paramref name="enumerable" /> is null or empty.
        /// </summary>
        /// <returns>true if <paramref name="enumerable" /> null or empty; otherwise false.</returns>
        [Pure]
        [MemberNotNullWhen(false)]
        public bool IsNullOrEmpty()
        {
            return enumerable switch
            {
                null => true,
                T[] array => array.Length == 0,
                ICollection<T> collection => collection.Count == 0,
                _ => enumerable.Any()
            };
        }
    }
}