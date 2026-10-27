using Ebceys.Infrastructure.Helpers.Jwt;
using Ebceys.Infrastructure.Interfaces;
using FluentValidation;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ebceys.Infrastructure.Extensions;

/// <summary>
///     The <see cref="IServiceCollection" /> extensions.
/// </summary>
[PublicAPI]
public static class EbIServiceCollectionExtensions
{
    /// <param name="services">The services.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Adds the <typeparamref name="TValidator" /> as <see cref="IValidator{TBody}" /> to transient services.
        /// </summary>
        /// <typeparam name="TValidator">The abstract validator.</typeparam>
        /// <typeparam name="TBody">The body.</typeparam>
        /// <returns>The <paramref name="services" /> instance.</returns>
        public IServiceCollection AddValidator<TValidator, TBody>()
            where TValidator : AbstractValidator<TBody>
        {
            services.AddTransient<IValidator<TBody>, TValidator>();
            return services;
        }

        /// <summary>
        ///     Adds command to <paramref name="services" /> as scoped service.
        /// </summary>
        /// <typeparam name="TCommand">The command implementation.</typeparam>
        /// <typeparam name="TContext">The command context.</typeparam>
        /// <typeparam name="TResult">The command result.</typeparam>
        /// <returns>The <paramref name="services" /> instance.</returns>
        public IServiceCollection AddScopedCommand<TCommand, TContext, TResult>()
            where TCommand : class, ICommand<TContext, TResult>
        {
            services.AddScoped<ICommand<TContext, TResult>, TCommand>();
            return services;
        }

        /// <summary>
        ///     Adds command to <paramref name="services" /> as singleton service.
        /// </summary>
        /// <typeparam name="TCommand">The command implementation.</typeparam>
        /// <typeparam name="TContext">The command context.</typeparam>
        /// <typeparam name="TResult">The command result.</typeparam>
        /// <returns>The <paramref name="services" /> instance.</returns>
        public IServiceCollection AddSingletonCommand<TCommand, TContext, TResult>()
            where TCommand : class, ICommand<TContext, TResult>
        {
            services.AddSingleton<ICommand<TContext, TResult>, TCommand>();
            return services;
        }

        /// <summary>
        ///     Adds command to <paramref name="services" /> as transient service.
        /// </summary>
        /// <typeparam name="TCommand">The command implementation.</typeparam>
        /// <typeparam name="TContext">The command context.</typeparam>
        /// <typeparam name="TResult">The command result.</typeparam>
        /// <returns>The <paramref name="services" /> instance.</returns>
        public IServiceCollection AddTransientCommand<TCommand, TContext, TResult>()
            where TCommand : class, ICommand<TContext, TResult>
        {
            services.AddTransient<ICommand<TContext, TResult>, TCommand>();
            return services;
        }

        /// <summary>
        ///     Adds the <see cref="JwtValidator" /> to <paramref name="services" />.
        /// </summary>
        /// <returns>The <paramref name="services" />.</returns>
        public IServiceCollection AddJwtValidator()
        {
            services.TryAddTransient<IJwtValidator, JwtValidator>();
            return services;
        }
    }
}