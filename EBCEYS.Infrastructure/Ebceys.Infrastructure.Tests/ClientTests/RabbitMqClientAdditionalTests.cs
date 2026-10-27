using AwesomeAssertions;
using Ebceys.Infrastructure.AuthorizationTestApplication.BoundedContext;
using Ebceys.Infrastructure.AuthorizationTestApplication.Client;
using Ebceys.Tests.Infrastructure.Helpers;

namespace Ebceys.Infrastructure.Tests.ClientTests;

public class RabbitMqClientAdditionalTests
{
    private static readonly EbRandomizer Randomizer = EbRandomizer.Create();
    private IAppRabbitClient _client;

    [SetUp]
    public void Setup()
    {
        _client = AppTestContext.AppContext.Factory.Services.GetRequiredService<IAppRabbitClient>();
    }

    // ── GetOk response value ──────────────────────────────────────────────────

    [Test]
    public async Task When_GetOk_With_Result_ResponseEqualsOk()
    {
        var response = await _client.GetOkAsync();

        response.Should().Be("Ok");
    }

    // ── GenerateToken with various usernames ──────────────────────────────────

    [TestCase(1)]
    [TestCase(16)]
    [TestCase(64)]
    public async Task When_GenerateToken_With_VariousUserNameLengths_Result_ValidTokenReturned(int length)
    {
        var request = new GenerateTokenRequest(Randomizer.String((uint)length));

        var response = await _client.GenerateTokenAsync(request);

        response.Should().NotBeNull();
        response!.Token.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task When_GenerateToken_With_SameUser_MultipleTimes_Result_TokensAreNonEmpty()
    {
        var request = new GenerateTokenRequest(Randomizer.String(16));

        var token1 = (await _client.GenerateTokenAsync(request))?.Token;
        var token2 = (await _client.GenerateTokenAsync(request))?.Token;

        token1.Should().NotBeNullOrEmpty();
        token2.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task When_GenerateToken_WithRandomUser_Result_TokenIsWellFormedJwt()
    {
        var request = new GenerateTokenRequest(Randomizer.String(16));

        var response = await _client.GenerateTokenAsync(request);

        response.Should().NotBeNull();
        var parts = response!.Token.Split('.');
        parts.Should().HaveCount(3, "a JWT must have exactly 3 parts");
    }
}