using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Ebceys.Tests.Infrastructure.IntegrationTests;

/// <summary>
///     Extension methods for <see cref="IWebHostBuilder" /> providing convenient configuration helpers
///     for integration tests, such as adding in-memory configuration values.
/// </summary>
[PublicAPI]
public static class TestsExtensions
{
    extension(IWebHostBuilder builder)
    {
        /// <summary>
        ///     Adds the in memory config.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void AddInMemoryConfig(string key, string value)
        {
            builder.ConfigureAppConfiguration(b => b.AddInMemoryCollection([
                new KeyValuePair<string, string?>(key, value)
            ]));
        }

        /// <summary>
        ///     Adds the in memory collection.
        /// </summary>
        /// <param name="config"></param>
        public void AddInMemoryCollection(Dictionary<string, string?> config)
        {
            builder.ConfigureAppConfiguration(b => b.AddInMemoryCollection(config));
        }
    }
}