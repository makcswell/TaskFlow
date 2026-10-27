using AwesomeAssertions;
using Ebceys.Infrastructure.Exceptions;
using Ebceys.Infrastructure.TestApplication.BoundedContext.Requests;
using Ebceys.Infrastructure.TestApplication.Client.Implementations;
using Ebceys.Infrastructure.TestApplication.DaL;
using Ebceys.Infrastructure.Tests.AppInitializer;
using Ebceys.Tests.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Ebceys.Infrastructure.Tests.ClientTests;

internal class TestAppClientAdditionalTests
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

    [TearDown]
    public async Task TearDown()
    {
        await using var dbContext = await _context.Factory.Services
            .GetRequiredService<IDbContextFactory<DataModelContext>>()
            .CreateDbContextAsync();

        await dbContext.TestTable.Where(_ => true).ExecuteDeleteAsync();
        await dbContext.SaveChangesAsync();
    }

    // ── GetJson ───────────────────────────────────────────────────────────────

    [Test]
    public async Task When_GetJson_With_Result_OkWithBody()
    {
        var result = await _client.TestClient.GetJsonAsync(CancellationToken.None);

        result.Should().NotBeNull();
        result.Message.Should().NotBeNullOrEmpty();
    }

    // ── PostBody boundary validation ──────────────────────────────────────────

    [TestCase(1)]
    [TestCase(10)]
    public async Task When_PostBody_With_BoundaryValidLength_Result_Ok(int length)
    {
        var name = Randomizer.String((uint)length);
        var response = await _client.TestClient.PostBodyAsync(new SomeBodyRequest(name), CancellationToken.None);

        response.Should().NotBeNull();
        response.Message.Should().Be(name);
    }

    [TestCase(11)]
    [TestCase(50)]
    public async Task When_PostBody_With_TooLongMessage_Result_ApiExceptionAndCode400(int length)
    {
        var name = Randomizer.String((uint)length);
        var act = () => _client.TestClient.PostBodyAsync(new SomeBodyRequest(name), CancellationToken.None);

        (await act.Should().ThrowAsync<ApiException>())
            .And.ProblemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
    }

    // ── PostCommand duplicates ────────────────────────────────────────────────

    [Test]
    public async Task When_PostCommand_With_DuplicateName_Result_ApiException()
    {
        var name = Randomizer.String(10);
        await _client.TestClient.PostCommandAsync(name, CancellationToken.None);

        var act = () => _client.TestClient.PostCommandAsync(name, CancellationToken.None);

        await act.Should().ThrowAsync<ApiException>();
    }

    // ── PutCommand edge-cases ─────────────────────────────────────────────────

    [Test]
    public async Task When_PutCommand_With_NonExistingEntity_Result_ApiExceptionAndCode404()
    {
        var act = () => _client.TestClient.PutCommandAsync(
            Randomizer.String(10),
            new ChangeNameRequest(Randomizer.String(10)),
            CancellationToken.None);

        (await act.Should().ThrowAsync<ApiException>())
            .And.ProblemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
    }

    [Test]
    public async Task When_PutCommand_With_InvalidNewName_Result_ApiExceptionAndCode400()
    {
        var name = Randomizer.String(8);
        await _client.TestClient.PostCommandAsync(name, CancellationToken.None);

        var act = () => _client.TestClient.PutCommandAsync(
            name,
            new ChangeNameRequest(Randomizer.String(51)),
            CancellationToken.None);

        (await act.Should().ThrowAsync<ApiException>())
            .And.ProblemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Test]
    public async Task When_PutCommand_With_EmptyNewName_Result_ApiExceptionAndCode400()
    {
        var name = Randomizer.String(8);
        await _client.TestClient.PostCommandAsync(name, CancellationToken.None);

        var act = () => _client.TestClient.PutCommandAsync(
            name,
            new ChangeNameRequest(string.Empty),
            CancellationToken.None);

        (await act.Should().ThrowAsync<ApiException>())
            .And.ProblemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
    }

    // ── DeleteCommand idempotency ─────────────────────────────────────────────

    [Test]
    public async Task When_DeleteCommand_With_AlreadyDeletedEntity_Result_ApiExceptionAndCode404()
    {
        var name = Randomizer.String(10);
        await _client.TestClient.PostCommandAsync(name, CancellationToken.None);
        await _client.TestClient.DeleteCommandAsync(name, CancellationToken.None);

        var act = () => _client.TestClient.DeleteCommandAsync(name, CancellationToken.None);

        (await act.Should().ThrowAsync<ApiException>())
            .And.ProblemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
    }

    // ── Multiple entities lifecycle ───────────────────────────────────────────

    [TestCase(3)]
    [TestCase(5)]
    public async Task When_PostMultipleCommands_With_UniqueNames_Result_AllPersisted(int count)
    {
        var names = Enumerable.Range(0, count)
            .Select(_ => Randomizer.String(8))
            .Distinct()
            .ToArray();

        foreach (var name in names)
        {
            await _client.TestClient.PostCommandAsync(name, CancellationToken.None);
        }

        var result = await _client.TestClient.GetCommandAsync(CancellationToken.None);

        result.Entities.Should().HaveCount(names.Length);
        result.Entities.Select(e => e.Name).Should().Contain(names);
    }

    [Test]
    public async Task When_PostThenDelete_With_MultipleEntities_Result_OnlyRemainingEntityExists()
    {
        var nameToKeep = Randomizer.String(8);
        var nameToDelete = Randomizer.String(8);

        await _client.TestClient.PostCommandAsync(nameToKeep, CancellationToken.None);
        await _client.TestClient.PostCommandAsync(nameToDelete, CancellationToken.None);

        await _client.TestClient.DeleteCommandAsync(nameToDelete, CancellationToken.None);

        var result = await _client.TestClient.GetCommandAsync(CancellationToken.None);

        result.Entities.Should().HaveCount(1);
        result.Entities.Should().Contain(e => e.Name == nameToKeep);
        result.Entities.Should().NotContain(e => e.Name == nameToDelete);
    }

    // ── Response body shape ───────────────────────────────────────────────────

    [Test]
    public async Task When_PostCommand_With_Name_Result_ResponseContainsCorrectIdAndName()
    {
        var name = Randomizer.String(8);
        var response = await _client.TestClient.PostCommandAsync(name, CancellationToken.None);

        response.Should().NotBeNull();
        response.Entities.Should().HaveCount(1);

        var entity = response.Entities.Single();
        entity.Name.Should().Be(name);
        entity.Id.Should().BeGreaterThan(0);
    }
}