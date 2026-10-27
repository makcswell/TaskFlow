using AwesomeAssertions;
using Ebceys.Infrastructure.Exceptions;
using Ebceys.Infrastructure.TestApplication.Client.Implementations;
using Ebceys.Infrastructure.Tests.AppInitializer;
using Ebceys.Tests.Infrastructure.Helpers;

namespace Ebceys.Infrastructure.Tests.ClientTests;

public class AuthClientTests
{
    private static readonly EbRandomizer Randomizer = new();
    private ITestApplicationClient _client;
    private AppTestClientContext _context;

    [SetUp]
    public void Setup()
    {
        _context = AppTestContext.AppContext;
        _client = _context.ServiceClient;
    }

    [Test]
    public async Task When_GetToken_With_AppClient_Result_Ok()
    {
        var result = await _client.TestClient.GenerateTokenAsync(CancellationToken.None);

        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        Console.WriteLine(result.Token);
    }

    [Test]
    public async Task When_ValidateToken_With_GotToken_Result_Ok()
    {
        var token = await _client.TestClient.GenerateTokenAsync(CancellationToken.None);

        var act = () => _client.TestClient.ValidateTokenAsync(token.Token, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task When_ValidateToken_With_RandomToken_Result_Forbidden()
    {
        var act = () => _client.TestClient.ValidateTokenAsync(Randomizer.Jwt(), CancellationToken.None);

        (await act.Should().ThrowAsync<ApiException>())
            .And.ProblemDetails.Status.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Test]
    public async Task When_ValidateAuth_With_TokenAndWithout_Result_FirstUnauthorizedSecondOk()
    {
        var act = () => _client.TestClient.ValidateAuthAsync(Randomizer.Jwt(), CancellationToken.None);

        (await act.Should().ThrowAsync<ApiException>())
            .And.ProblemDetails.Status.Should().Be(StatusCodes.Status401Unauthorized);

        var token = await _client.TestClient.GenerateTokenAsync(CancellationToken.None);

        var act2 = () => _client.TestClient.ValidateAuthAsync(token.Token, CancellationToken.None);

        await act2.Should().NotThrowAsync();
    }
}