using AwesomeAssertions;
using Ebceys.Infrastructure.AuthorizationTestApplication.Client;
using Ebceys.Infrastructure.Helpers;
using Ebceys.Infrastructure.Helpers.Sequences;
using Ebceys.Infrastructure.HttpClient.TokenManager;
using Ebceys.Infrastructure.Interfaces;
using Ebceys.Infrastructure.TestApplication.BoundedContext.Requests;
using Ebceys.Infrastructure.TestApplication.Commands;
using Ebceys.Infrastructure.TestApplication.DaL;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Ebceys.Infrastructure.Tests.DiTests;

public class DiCTests
{
    private IServiceProvider _context;

    [SetUp]
    public void SetUp()
    {
        _context = AppTestContext.AppContext.Factory.Services;
    }

    [Test]
    public void When_AppIsRunning_With_TestApp_Result_HttpContextAccessorExists()
    {
        var accessor = _context.GetService<IHttpContextAccessor>();

        accessor.Should().NotBeNull();
    }

    [Test]
    public void When_AppIsRunning_With_TestApp_Result_RequestValidatorExists()
    {
        var validator1 = _context.GetService<IValidator<ChangeNameRequest>>();
        var validator2 = _context.GetService<IValidator<SomeBodyRequest>>();

        validator1.Should().NotBeNull();
        validator2.Should().NotBeNull();
    }

    [Test]
    public void When_AppIsRunning_With_TestApp_Result_ScopedCommandsExists()
    {
        var command = GetServiceIfExists<ICommand<AddEntityCommandContext, AddEntityCommandResult>>();
        using var scope = _context.CreateScope();
        var scopedCommand = scope.ServiceProvider
            .GetRequiredService<ICommand<AddEntityCommandContext, AddEntityCommandResult>>();

        command.Should().NotBeNull();
        scopedCommand.Should().NotBeNull();
    }

    [Test]
    public void When_AppIsRunning_With_TestApp_Result_ServiceClientExists()
    {
        var client = _context.GetService<IAuthAppClient>();

        client.Should().NotBeNull();
    }

    [Test]
    public void When_AppIsRunning_With_TestApp_Result_AuthTokenResolverExists()
    {
        var manager = _context.GetService<IClientTokenManager<IAuthAppClient>>();

        manager.Should().NotBeNull();
    }

    [Test]
    public void When_AppIsRunning_With_TestApp_Result_DataModelContextFactoryExists()
    {
        var dataModelContext = _context.GetService<IDbContextFactory<DataModelContext>>();

        dataModelContext.Should().NotBeNull();
    }

    [Test]
    public void When_AppIsRunning_With_TestApp_Result_DataModelContextExists()
    {
        var dataModelContext = GetServiceIfExists<DataModelContext>();

        using var scope = _context.CreateScope();
        var context = scope.ServiceProvider.GetService<DataModelContext>();

        dataModelContext.Should().NotBeNull();
        context.Should().NotBeNull();
    }

    [Test]
    public void When_AppIsRunning_With_TestApp_Result_CommandExecutorExists()
    {
        var singletonExecutorExists = _context.GetService<ICommandExecutor>();
        var scopedExecutorDoesntExists = GetServiceIfExists<IScopedCommandExecutor>();

        using var scope = _context.CreateScope();
        var scopedExecutorExists = scope.ServiceProvider.GetService<IScopedCommandExecutor>();

        singletonExecutorExists.Should().NotBeNull();
        scopedExecutorExists.Should().NotBeNull();
        scopedExecutorDoesntExists.Should().NotBeNull();
    }

    [Test]
    public void When_AppIsRunning_With_TestApp_Result_AtomicGeneratorExists()
    {
        _context.GetRequiredService<IAtomGenerator<int>>().Should().NotBeNull();
        _context.GetRequiredService<IAtomGenerator<long>>().Should().NotBeNull();
    }

    private T? GetServiceIfExists<T>()
        where T : class
    {
        try
        {
            return _context.GetRequiredService<T>();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }
}