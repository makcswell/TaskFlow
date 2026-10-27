using AwesomeAssertions;
using Ebceys.Infrastructure.DatabaseRegistration;
using Ebceys.Tests.Infrastructure.Helpers;
using Ebceys.Tests.Infrastructure.Helpers.TestCaseSources;
using FluentValidation;
using JetBrains.Annotations;

namespace Ebceys.Infrastructure.UnitTests.Validators;

public class DbContextRegistrationOptionsValidatorTests
{
    private static readonly EbRandomizer Randomizer = new();
    private DbContextRegistrationOptionsValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new DbContextRegistrationOptionsValidator();
    }

    [TestCaseSource(
        typeof(EbTestDataValidExecutor<DbContextRegistrationOptionsValidatorCaseSource, DbContextRegistrationOptions>))]
    public void When_Validate_With_ValidCase_Result_NotThrows(DbContextRegistrationOptions cfg)
    {
        var act = () => _validator.ValidateAndThrow(cfg);

        act.Should().NotThrow<ValidationException>();
    }

    [TestCaseSource(
        typeof(EbTestDataInvalidExecutor<DbContextRegistrationOptionsValidatorCaseSource,
            DbContextRegistrationOptions>))]
    public void When_Validate_With_InvalidCase_Result_NotThrows(DbContextRegistrationOptions cfg)
    {
        var act = () => _validator.ValidateAndThrow(cfg);

        act.Should().Throw<ValidationException>();
    }

    [UsedImplicitly]
    private class DbContextRegistrationOptionsValidatorCaseSource : EbTestCaseSource<DbContextRegistrationOptions>
    {
        public override DbContextRegistrationOptions SetupValidBase()
        {
            return new DbContextRegistrationOptions
            {
                ConnectionString = "SomeConnectionString",
                MigrateDb = true
            };
        }

        public override DbContextRegistrationOptions? SetupInvalidBase()
        {
            return new DbContextRegistrationOptions();
        }

        public override IEnumerable<(Func<DbContextRegistrationOptions?, DbContextRegistrationOptions?>, string)>
            GetValidCases()
        {
            yield return (cfg => cfg! with { Retries = 1 }, "Retries");
            yield return (cfg => cfg! with { Timeout = TimeSpan.FromSeconds(1) }, "Timeout");
            yield return (cfg => cfg! with { MigrateDb = false }, "MigrateDb");
            yield return (cfg => cfg! with { Retries = 1, Timeout = TimeSpan.FromSeconds(1) }, "Retries and Timeout");
        }

        public override IEnumerable<(Func<DbContextRegistrationOptions?, DbContextRegistrationOptions?>, string)>
            GetInvalidCases()
        {
            yield return (cfg => cfg! with { ConnectionString = string.Empty }, "ConnectionString");
            yield return (
                cfg => cfg! with { ConnectionString = Randomizer.String(), Timeout = TimeSpan.FromSeconds(-1) },
                "Timeout -1s");
            yield return (
                cfg => cfg! with { ConnectionString = Randomizer.String(), Timeout = TimeSpan.FromSeconds(0) },
                "Timeout 0s");
        }
    }
}