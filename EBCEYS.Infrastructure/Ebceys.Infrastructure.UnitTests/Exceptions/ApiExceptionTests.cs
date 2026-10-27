using AwesomeAssertions;
using Ebceys.Infrastructure.Exceptions;
using Ebceys.Tests.Infrastructure.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ebceys.Infrastructure.UnitTests.Exceptions;

public class ApiExceptionTests
{
    private static readonly EbRandomizer Randomizer = new();

    [Test]
    public void When_NotFound_With_RandomMessage_Result_CorrectFields()
    {
        var act = () => ApiExceptionHelper.ThrowNotFound<object>(Randomizer.String(10));

        var assertion = act.Should().Throw<ApiException>().And;
        assertion.ProblemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        assertion.ProblemDetails.Title.Should().NotBeEmpty();
        assertion.ProblemDetails.Instance.Should().NotBeEmpty();
        assertion.ProblemDetails.Detail.Should().NotBeEmpty();
        Console.WriteLine(assertion.ProblemDetails.Instance);
    }

    [Test]
    public void When_NotFound_With_RandomException_Result_CorrectFields()
    {
        var act = () => ApiExceptionHelper.ThrowNotFound<object, Exception>(new Exception(Randomizer.String(10)));

        var assertion = act.Should().Throw<ApiException>().And;
        assertion.ProblemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        assertion.ProblemDetails.Title.Should().NotBeEmpty();
        assertion.ProblemDetails.Instance.Should().NotBeEmpty();
        assertion.ProblemDetails.Detail.Should().NotBeEmpty();
    }

    [Test]
    public void When_Conflict_With_RandomMessage_Result_CorrectFields()
    {
        var act = () => ApiExceptionHelper.ThrowConflict<object>(Randomizer.String(10));

        var assertion = act.Should().Throw<ApiException>().And;
        assertion.ProblemDetails.Status.Should().Be(StatusCodes.Status409Conflict);
        assertion.ProblemDetails.Title.Should().NotBeEmpty();
        assertion.ProblemDetails.Instance.Should().NotBeEmpty();
        assertion.ProblemDetails.Detail.Should().NotBeEmpty();
        Console.WriteLine(assertion.ProblemDetails.Instance);
    }

    [Test]
    public void When_Conflict_With_RandomException_Result_CorrectFields()
    {
        var act = () => ApiExceptionHelper.ThrowConflict<object, Exception>(new Exception(Randomizer.String(10)));

        var assertion = act.Should().Throw<ApiException>().And;
        assertion.ProblemDetails.Status.Should().Be(StatusCodes.Status409Conflict);
        assertion.ProblemDetails.Title.Should().NotBeEmpty();
        assertion.ProblemDetails.Instance.Should().NotBeEmpty();
        assertion.ProblemDetails.Detail.Should().NotBeEmpty();
    }

    [Test]
    public void When_Validation_With_RandomMessage_Result_CorrectFields()
    {
        var act = () => ApiExceptionHelper.ThrowValidation<object>(new ValidationProblemDetails(
            new Dictionary<string, string[]>
            {
                { Randomizer.String(10), Randomizer.StringArray(10, 10) }
            })
        {
            Status = StatusCodes.Status400BadRequest,
            Title = Randomizer.String(10),
            Detail = Randomizer.String(10)
        });

        var assertion = act.Should().Throw<ApiException>().And;
        assertion.ProblemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
        assertion.ProblemDetails.Title.Should().NotBeEmpty();
        assertion.ProblemDetails.Instance.Should().NotBeEmpty();
        assertion.ProblemDetails.Detail.Should().NotBeEmpty();
    }

    [Test]
    public void When_Exception_With_RandomException_Result_CorrectFields()
    {
        var act = () => ApiExceptionHelper.ThrowException<object, Exception>(new Exception(Randomizer.String(10)));

        var assertion = act.Should().Throw<ApiException>().And;
        assertion.ProblemDetails.Status.Should().Be(StatusCodes.Status500InternalServerError);
        assertion.ProblemDetails.Title.Should().NotBeEmpty();
        assertion.ProblemDetails.Instance.Should().NotBeEmpty();
        assertion.ProblemDetails.Detail.Should().NotBeEmpty();
    }

    [Test]
    public void When_Exception_With_RandomExceptionAndNoReturningType_Result_CorrectFields()
    {
        var act = () => ApiExceptionHelper.ThrowException(new Exception(Randomizer.String(10)));

        var assertion = act.Should().Throw<ApiException>().And;
        assertion.ProblemDetails.Status.Should().Be(StatusCodes.Status500InternalServerError);
        assertion.ProblemDetails.Title.Should().NotBeEmpty();
        assertion.ProblemDetails.Instance.Should().NotBeEmpty();
        assertion.ProblemDetails.Detail.Should().NotBeEmpty();
    }

    [Test]
    public void When_ApiException_With_EmptyProblemDetails_Result_CorrectFields()
    {
        var act = () =>
            ApiExceptionHelper.ThrowApiException<object>(new ProblemDetails(),
                StatusCodes.Status500InternalServerError);

        var assertion = act.Should().Throw<ApiException>().And;
        assertion.ProblemDetails.Status.Should().BeNull();
        assertion.ProblemDetails.Instance.Should().NotBeEmpty();
    }

    [Test]
    public void When_ApiException_With_NullProblemDetails_Result_CorrectFields()
    {
        var act = () => ApiExceptionHelper.ThrowApiException<object>(null, StatusCodes.Status500InternalServerError);

        var assertion = act.Should().Throw<ApiException>().And;
        assertion.ProblemDetails.Status.Should().Be(StatusCodes.Status500InternalServerError);
        assertion.ProblemDetails.Title.Should().NotBeEmpty();
        assertion.ProblemDetails.Instance.Should().NotBeEmpty();
        assertion.ProblemDetails.Detail.Should().NotBeEmpty();
    }

    [Test]
    public void When_ApiException_With_RandomProblemDetails_Result_CorrectFields()
    {
        var statusCode = Randomizer.Int(400, 550);
        var act = () => ApiExceptionHelper.ThrowApiException<object>(new ProblemDetails
        {
            Detail = Randomizer.String(10),
            Status = statusCode,
            Title = Randomizer.String(10)
        });

        var assertion = act.Should().Throw<ApiException>().And;
        assertion.ProblemDetails.Status.Should().Be(statusCode);
        assertion.ProblemDetails.Title.Should().NotBeEmpty();
        assertion.ProblemDetails.Instance.Should().NotBeEmpty();
        assertion.ProblemDetails.Detail.Should().NotBeEmpty();
    }
}