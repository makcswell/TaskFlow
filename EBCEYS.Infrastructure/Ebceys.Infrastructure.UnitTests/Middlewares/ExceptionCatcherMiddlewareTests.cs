using AwesomeAssertions;
using Ebceys.Infrastructure.Exceptions;
using Ebceys.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ebceys.Infrastructure.UnitTests.Middlewares;

public class ExceptionCatcherMiddlewareTests
{
    // ── Passes through when no exception ────────────────────────────────────

    [Test]
    public async Task When_Invoke_WithNoException_Result_NextIsCalled()
    {
        var nextCalled = false;
        var middleware = new ExceptionCatcherMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.Invoke(new DefaultHttpContext());

        nextCalled.Should().BeTrue();
    }

    // ── Wraps arbitrary exception into ApiException ──────────────────────────

    [Test]
    public async Task When_Invoke_WithRegularException_Result_ApiExceptionWithCode500()
    {
        var inner = new InvalidOperationException("something went wrong");
        var middleware = new ExceptionCatcherMiddleware(_ => throw inner);

        var act = async () => await middleware.Invoke(new DefaultHttpContext());

        var thrown = (await act.Should().ThrowAsync<ApiException>()).And;
        thrown.ProblemDetails.Status.Should().Be(StatusCodes.Status500InternalServerError);
        thrown.ProblemDetails.Detail.Should().Contain(inner.Message);
    }

    [Test]
    public async Task When_Invoke_WithArgumentException_Result_ApiExceptionWithCode500()
    {
        var middleware = new ExceptionCatcherMiddleware(_ => throw new ArgumentException("bad arg"));

        var act = async () => await middleware.Invoke(new DefaultHttpContext());

        (await act.Should().ThrowAsync<ApiException>())
            .And.ProblemDetails.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    // ── Does NOT catch ApiException — lets it propagate ──────────────────────

    [Test]
    public async Task When_Invoke_WithApiException_Result_ApiExceptionPropagatesUnchanged()
    {
        var original = new ApiException(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Not Found",
            Detail = "Resource missing"
        }, "Not Found");

        var middleware = new ExceptionCatcherMiddleware(_ => throw original);

        var act = async () => await middleware.Invoke(new DefaultHttpContext());

        var thrown = (await act.Should().ThrowAsync<ApiException>()).And;
        thrown.ProblemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        thrown.Should().BeSameAs(original);
    }

    [Test]
    public async Task When_Invoke_WithApiExceptionCode403_Result_SameApiExceptionPropagates()
    {
        var original = new ApiException(new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden"
        }, "Forbidden");

        var middleware = new ExceptionCatcherMiddleware(_ => throw original);

        var act = async () => await middleware.Invoke(new DefaultHttpContext());

        (await act.Should().ThrowAsync<ApiException>())
            .And.ProblemDetails.Status.Should().Be(StatusCodes.Status403Forbidden);
    }

    // ── Exception message preserved ─────────────────────────────────────────

    [Test]
    public async Task When_Invoke_WithExceptionMessage_Result_MessagePreservedInApiException()
    {
        const string message = "detailed error description";
        var middleware = new ExceptionCatcherMiddleware(_ => throw new Exception(message));

        var act = async () => await middleware.Invoke(new DefaultHttpContext());

        var thrown = (await act.Should().ThrowAsync<ApiException>()).And;
        thrown.ProblemDetails.Detail.Should().Contain(message);
    }
}