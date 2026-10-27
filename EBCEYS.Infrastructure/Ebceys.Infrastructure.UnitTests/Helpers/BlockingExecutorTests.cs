using AwesomeAssertions;
using Ebceys.Infrastructure.Helpers.Multithreading;

namespace Ebceys.Infrastructure.UnitTests.Helpers;

public class BlockingLockExecutorTests
{
    [Test]
    public async Task When_EnqueJobAsync_WithSingleJob_Result_ResultProcessed()
    {
        // Arrange & Act
        ValueTask<int[]> results;
        using (var executor = new BlockingLockExecutor<int, int>(x => x * 2))
        {
            results = executor.GetResultsAsync().ToArrayAsync();
            await executor.EnqueJobAsync(5);
        }

        // Assert
        (await results).Should().Equal(10);
    }

    [Test]
    public async Task When_EnqueJobAsync_WithMultipleJobs_Result_AllResultsProcessed()
    {
        // Arrange & Act
        ValueTask<int[]> results;
        using (var executor = new BlockingLockExecutor<int, int>(x => x * 2))
        {
            results = executor.GetResultsAsync().ToArrayAsync();
            await executor.EnqueJobAsync(1);
            await executor.EnqueJobAsync(2);
            await executor.EnqueJobAsync(3);
        }

        // Assert
        (await results).Should().BeEquivalentTo([2, 4, 6]);
    }

    [Test]
    public async Task When_EnqueJobAsync_WithParallelism_Result_JobsProcessedConcurrently()
    {
        // Arrange & Act
        ValueTask<int[]> results;
        using (var executor = new BlockingLockExecutor<int, int>(x =>
               {
                   Thread.Sleep(10); // Simulate work
                   return x * 2;
               }, new BlockingLockConfiguration(2)))
        {
            results = executor.GetResultsAsync().ToArrayAsync();
            var task1 = executor.EnqueJobAsync(1);
            var task2 = executor.EnqueJobAsync(2);
            await Task.WhenAll(task1, task2);
        }

        // Assert
        (await results).Should().HaveCount(2);
        (await results).Should().Contain(2).And.Contain(4);
    }

    [Test]
    public async Task When_EnqueJobAsync_WithCancellationToken_Result_TaskCancelled()
    {
        // Arrange & Act
        using var executor = new BlockingLockExecutor<int, int>(x =>
        {
            Thread.Sleep(200);
            return x * 2;
        });
        using var cts = new CancellationTokenSource(100);
        var act = () => executor.EnqueJobAsync(5, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}

public class BlockingLockAsyncExecutorTests
{
    [Test]
    public async Task When_GetResultsAsync_WithSingleJob_Result_ResultProcessed()
    {
        // Arrange & Act
        ValueTask<int[]> results;
        using (var executor = new BlockingLockAsyncExecutor<int, int>(async x =>
               {
                   await Task.Delay(10);
                   return x * 2;
               }))
        {
            results = executor.GetResultsAsync().ToArrayAsync();
            await executor.EnqueJobAsync(5);
        }

        // Assert
        (await results).Should().Equal(10);
    }

    [Test]
    public async Task When_GetResultsAsync_WithMultipleJobs_Result_AllResultsProcessed()
    {
        // Arrange & Act
        ValueTask<int[]> results;
        using (var executor = new BlockingLockAsyncExecutor<int, int>(async x =>
               {
                   await Task.Delay(10);
                   return x * 2;
               }))
        {
            results = executor.GetResultsAsync().ToArrayAsync();
            await executor.EnqueJobAsync(1);
            await executor.EnqueJobAsync(2);
            await executor.EnqueJobAsync(3);
        }

        // Assert
        (await results).Should().BeEquivalentTo([2, 4, 6]);
    }

    [Test]
    public async Task When_EnqueJobAsync_WithExceptionInFunc_Result_ExceptionPropagated()
    {
        // Arrange & Act
        using var executor = new BlockingLockAsyncExecutor<int, int>(_ => throw new InvalidOperationException("Test"));
        var act = () => executor.EnqueJobAsync(1);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}

public class BlockingLockProcessorTests
{
    [Test]
    public async Task When_EnqueJobAsync_WithAction_Result_ActionExecuted()
    {
        // Arrange & Act
        var processedItems = new List<int>();
        using (var processor = new BlockingLockProcessor<int>(processedItems.Add))
        {
            await processor.EnqueJobAsync(5);
            await processor.EnqueJobAsync(10);
        }

        // Assert
        processedItems.Should().Equal(5, 10);
    }

    [Test]
    public void When_GetResultsAsync_WithProcessor_Result_NotSupportedException()
    {
        // Arrange
        using var processor = new BlockingLockProcessor<int>(_ => { });

        // Act
        Action act = () => processor.GetResultsAsync();

        // Assert
        act.Should().Throw<NotSupportedException>();
    }
}

public class BlockingLockAsyncProcessorTests
{
    [Test]
    public async Task When_EnqueJobAsync_WithAsyncAction_Result_ActionExecuted()
    {
        // Arrange & Act
        var processedItems = new List<int>();
        var processingCompleted = new TaskCompletionSource();
        var processedCount = 0;

        using (var processor = new BlockingLockAsyncProcessor<int>(async x =>
               {
                   await Task.Delay(1);
                   lock (processedItems)
                   {
                       processedItems.Add(x);
                       processedCount++;
                       if (processedCount == 2)
                       {
                           processingCompleted.SetResult();
                       }
                   }
               }))
        {
            await processor.EnqueJobAsync(5);
            await processor.EnqueJobAsync(10);
        }

        // Wait for items to be processed (with timeout)
        await Task.WhenAny(processingCompleted.Task, Task.Delay(5000));

        // Assert
        processedItems.Should().Equal(5, 10);
    }

    [Test]
    public void When_GetResultsAsync_WithAsyncProcessor_Result_NotSupportedException()
    {
        // Arrange
        using var processor = new BlockingLockAsyncProcessor<int>(_ => Task.CompletedTask);

        // Act
        Action act = () => processor.GetResultsAsync();

        // Assert
        act.Should().Throw<NotSupportedException>();
    }
}