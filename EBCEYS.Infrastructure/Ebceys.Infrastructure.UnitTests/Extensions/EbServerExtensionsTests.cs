using System.Diagnostics;
using AwesomeAssertions;
using Ebceys.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ebceys.Infrastructure.UnitTests.Extensions;

public class EbServerExtensionsTests
{
    // ── ProblemDetails.CreateFromResponse ────────────────────────────────────

    [TestCase(200)]
    [TestCase(400)]
    [TestCase(404)]
    [TestCase(500)]
    public void When_CreateFromResponse_With_KnownStatusCode_Result_CorrectProblemDetails(int statusCode)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/test/path";
        httpContext.Response.StatusCode = statusCode;

        var result = ProblemDetails.CreateFromResponse(httpContext.Response);

        result.Status.Should().Be(statusCode);
        result.Instance.Should().Be("/test/path");
        result.Title.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void When_CreateFromResponse_With_UnknownStatusCode_Result_StatusCodeAsString()
    {
        const int unknownCode = 599;
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/some/path";
        httpContext.Response.StatusCode = unknownCode;

        var result = ProblemDetails.CreateFromResponse(httpContext.Response);

        result.Status.Should().Be(unknownCode);
        result.Title.Should().Be(unknownCode.ToString());
    }

    [Test]
    public void When_CreateFromResponse_With_RootPath_Result_InstanceIsRoot()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/";
        httpContext.Response.StatusCode = 200;

        var result = ProblemDetails.CreateFromResponse(httpContext.Response);

        result.Instance.Should().Be("/");
    }

    // ── Task.WaitUntilAsync ───────────────────────────────────────────────────

    [Test]
    public async Task When_WaitUntilAsync_With_ImmediatelyTrueCondition_Result_CompletesInstantly()
    {
        var act = async () => await Task.WaitUntilAsync(_ => true, TimeSpan.FromSeconds(5));

        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task When_WaitUntilAsync_With_ConditionBecomesTrueAfterDelay_Result_CompletesBeforeTimeout()
    {
        var flag = false;
        _ = Task.Run(async () =>
        {
            await Task.Delay(100);
            flag = true;
        });

        await Task.WaitUntilAsync(_ => flag, TimeSpan.FromSeconds(5));

        flag.Should().BeTrue();
    }

    [Test]
    public async Task When_WaitUntilAsync_With_NeverTrueCondition_Result_CompletesAfterTimeout()
    {
        var sw = Stopwatch.StartNew();

        var act = () => Task.WaitUntilAsync(_ => false, TimeSpan.FromMilliseconds(2000));
        await act.Should().ThrowAsync<OperationCanceledException>();

        sw.Stop();

        sw.Elapsed.Should().BeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(1500));
    }

    [Test]
    public async Task When_WaitUntilAsync_With_CancelledToken_Result_CompletesEarly()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        var sw = Stopwatch.StartNew();
        var act = () => Task.WaitUntilAsync(_ => false, TimeSpan.FromSeconds(30), cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
    }
}