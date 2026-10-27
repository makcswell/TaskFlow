using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ebceys.Infrastructure.Helpers.Sequences;

/// <summary>
///     Extension methods for registering <see cref="IAtomGenerator{T}" /> implementations
///     (thread-safe atomic long and int generators) in the DI container.
/// </summary>
[PublicAPI]
public static class AtomicGeneratorsExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Adds the <see cref="IAtomGenerator{T}" /> to <paramref name="services" />.
        /// </summary>
        /// <param name="seed">The seed.</param>
        /// <returns>
        ///     <paramref name="services" />
        /// </returns>
        public IServiceCollection AddAtomicGenerators(int? seed = null)
        {
            services.TryAddSingleton<IAtomGenerator<long>>(_ => new AtomicLongGenerator(seed));
            services.TryAddSingleton<IAtomGenerator<int>>(_ => new AtomicIntGenerator(seed));
            return services;
        }
    }
}