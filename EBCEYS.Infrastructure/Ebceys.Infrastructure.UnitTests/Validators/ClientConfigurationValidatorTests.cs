using AwesomeAssertions;
using Ebceys.Infrastructure.HttpClient.ClientRegistration;
using Ebceys.Tests.Infrastructure.Helpers;
using Ebceys.Tests.Infrastructure.Helpers.TestCaseSources;
using FluentValidation;
using JetBrains.Annotations;

namespace Ebceys.Infrastructure.UnitTests.Validators;

public class ClientConfigurationValidatorTests
{
    private static readonly EbRandomizer Randomizer = new();
    private ClientConfigurationValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new ClientConfigurationValidator();
    }

    [TestCaseSource(typeof(EbTestDataValidExecutor<ClientConfigurationValidatorCaseSource, ClientConfiguration>))]
    public void When_Validate_With_ValidCase_Result_NotThrows(ClientConfiguration cfg)
    {
        var act = () => _validator.ValidateAndThrow(cfg);

        act.Should().NotThrow<ValidationException>();
    }

    [TestCaseSource(typeof(EbTestDataInvalidExecutor<ClientConfigurationValidatorCaseSource, ClientConfiguration>))]
    public void When_Validate_With_InvalidCase_Result_NotThrows(ClientConfiguration cfg)
    {
        var act = () => _validator.ValidateAndThrow(cfg);

        act.Should().Throw<ValidationException>();
    }

    [UsedImplicitly]
    private class ClientConfigurationValidatorCaseSource : EbTestCaseSource<ClientConfiguration>
    {
        public override ClientConfiguration SetupValidBase()
        {
            return new ClientConfiguration
            {
                ServiceUrl = Randomizer.Url().ToString()
            };
        }

        public override ClientConfiguration? SetupInvalidBase()
        {
            return new ClientConfiguration
            {
                ServiceUrl = string.Empty
            };
        }

        public override IEnumerable<(Func<ClientConfiguration?, ClientConfiguration?>, string)> GetValidCases()
        {
            for (var i = 0; i < 10; i++)
            {
                yield return (_ => new ClientConfiguration { ServiceUrl = Randomizer.Url().ToString() },
                    "Random ServiceUrl");
            }
        }

        public override IEnumerable<(Func<ClientConfiguration?, ClientConfiguration?>, string)> GetInvalidCases()
        {
            yield return (conf => conf, "Empty ServiceUrl");
            yield return (_ => new ClientConfiguration { ServiceUrl = Randomizer.String() }, "Random string");
            yield return (_ => new ClientConfiguration { ServiceUrl = Randomizer.HexString() }, "Random hex string");
            yield return (_ => new ClientConfiguration { ServiceUrl = Randomizer.Domain() }, "Random domain");
            yield return (_ => new ClientConfiguration { ServiceUrl = Randomizer.Email() }, "Random email");
        }
    }
}