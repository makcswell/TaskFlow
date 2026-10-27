using AwesomeAssertions;
using Ebceys.Infrastructure.Options;
using EBCEYS.RabbitMQ.Configuration;
using Ebceys.Tests.Infrastructure.Helpers;
using Ebceys.Tests.Infrastructure.Helpers.TestCaseSources;
using FluentValidation;
using JetBrains.Annotations;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

namespace Ebceys.Infrastructure.UnitTests.Validators;

public class SimpleRabbitMqConfigurationValidatorTests
{
    private static readonly EbRandomizer Randomizer = EbRandomizer.Create();

    [TestCaseSource(
        typeof(EbTestDataValidExecutor<SimpleRabbitMqConfigurationTestCaseSource, SimpleRabbitMqConfiguration>))]
    public void When_Validate_With_SpecifiedValidCases_Result_NoException(SimpleRabbitMqConfiguration? configuration)
    {
        var act = configuration.Validate;
        act.Should().NotThrow();
    }

    [TestCaseSource(
        typeof(EbTestDataInvalidExecutor<SimpleRabbitMqConfigurationTestCaseSource, SimpleRabbitMqConfiguration>))]
    public void When_Validate_With_SpecifiedInvalidCases_Result_ValidationException(
        SimpleRabbitMqConfiguration? configuration)
    {
        var act = configuration.Validate;
        act.Should().Throw<ValidationException>();
    }

    [UsedImplicitly]
    private class SimpleRabbitMqConfigurationTestCaseSource : EbTestCaseSource<SimpleRabbitMqConfiguration>
    {
        public override SimpleRabbitMqConfiguration SetupValidBase()
        {
            return new SimpleRabbitMqConfiguration
            {
                ConnectionString = "amqp://guest:guest@localhost:5672",
                ExName = Randomizer.String(16),
                ExType = Randomizer.Enum<ExchangeTypes>(),
                QueueName = Randomizer.String(16),
                TimeoutCallback = Randomizer.TimeSpan(TimeSpan.FromMinutes(5)),
                QueueNameCallback = Randomizer.String(16),
                ExNameCallback = Randomizer.String(16),
                ExTypeCallback = Randomizer.Enum<ExchangeTypes>(),
                RoutingKey = Randomizer.String(16)
            };
        }

        public override SimpleRabbitMqConfiguration SetupInvalidBase()
        {
            return SetupValidBase();
        }

        public override IEnumerable<(Func<SimpleRabbitMqConfiguration?, SimpleRabbitMqConfiguration?>, string)>
            GetValidCases()
        {
            yield return (
                x => x with
                {
                    TimeoutCallback = null, QueueNameCallback = null, ExNameCallback = null,
                    ExTypeCallback = default, RoutingKey = null
                }, "Without_Callback");
            yield return (x => x with
            {
                TimeoutCallback = null, QueueNameCallback = null, ExNameCallback = null,
                ExTypeCallback = default, RoutingKey = Randomizer.String(32)
            }, "Without_Callback_But_With_RoutingKey");
            yield return (x => x with
            {
                RoutingKey = null
            }, "With_CallBack_But_Without_RoutingKey");
        }

        public override IEnumerable<(Func<SimpleRabbitMqConfiguration?, SimpleRabbitMqConfiguration?>, string)>
            GetInvalidCases()
        {
            yield return (
                _ => null,
                "With_Null");

            yield return (
                x => x with { ConnectionString = null },
                "With_NULLConnectionString");

            yield return (
                x => x with { ConnectionString = Randomizer.String(64) },
                "With_NotUriConnectionString");

            yield return (
                x => x with { ExName = null },
                "With_NULLExName");

            yield return (
                x => x with { ExName = string.Empty },
                "With_EmptyExName");

            yield return (
                x => x with { ExType = (ExchangeTypes)54 },
                "With_InvalidExType");

            yield return (
                x => x with { QueueName = null },
                "With_NULLQueueName");

            yield return (
                x => x with { QueueName = string.Empty },
                "With_EmptyQueueName");

            yield return (
                x => x with { QueueNameCallback = null },
                "With_TimeoutSetButQueueNameCallbackIsNull");

            yield return (
                x => x with { QueueNameCallback = string.Empty },
                "With_TimeoutSetButQueueNameCallbackIsEmpty");

            yield return (
                x => x with { ExNameCallback = null },
                "With_TimeoutSetButExNameCallbackIsNull");

            yield return (
                x => x with { ExNameCallback = string.Empty },
                "With_TimeoutSetButExNameCallbackIsEmpty");

            yield return (
                x => x with { ExTypeCallback = (ExchangeTypes)54 },
                "With_TimeoutSetButExTypeCallbackIsNotInEnum");

            yield return (
                x => x with { TimeoutCallback = TimeSpan.Zero },
                "With_ZeroTimeout");
        }
    }
}