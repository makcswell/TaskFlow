using Ebceys.Infrastructure.Interfaces;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Ebceys.Infrastructure.Extensions;

/// <summary>
///     The <see cref="IServiceProvider" /> extensions.
/// </summary>
[PublicAPI]
public static class EbIServiceProviderExtensions
{
    /// <param name="serviceProvider">The service provider.</param>
    extension(IServiceProvider serviceProvider)
    {
        /// <summary>
        ///     Gets the command from service provider.
        /// </summary>
        /// <typeparam name="TContext">The command context.</typeparam>
        /// <typeparam name="TResult">The command result.</typeparam>
        /// <returns>The instance of <see cref="ICommand{TContext,TResult}" /> if exists; otherwise <c>null</c>.</returns>
        public ICommand<TContext, TResult>? GetCommand<TContext, TResult>()
        {
            return serviceProvider.GetService<ICommand<TContext, TResult>>();
        }

        /// <summary>
        ///     Gets the command from service provider.
        /// </summary>
        /// <typeparam name="TContext">The command context.</typeparam>
        /// <typeparam name="TResult">The command result.</typeparam>
        /// <returns>The instance of <see cref="ICommand{TContext,TResult}" />.</returns>
        public ICommand<TContext, TResult> GetRequiredCommand<TContext, TResult>()
        {
            return serviceProvider.GetRequiredService<ICommand<TContext, TResult>>();
        }
    }
}