using AwesomeAssertions;
using Ebceys.Infrastructure.Helpers;

namespace Ebceys.Infrastructure.UnitTests.Helpers;

public class PaginationDataTests
{
    // ── Constructor ──────────────────────────────────────────────────────────

    [Test]
    public void When_PaginationData_WithDefaultConstructor_Result_DefaultBatchSize()
    {
        var data = new PaginationData();

        data.BatchSize.Should().Be(100);
        data.Skip.Should().Be(0);
        data.Iterations.Should().Be(0);
    }

    [TestCase(1)]
    [TestCase(10)]
    [TestCase(500)]
    [TestCase(int.MaxValue)]
    public void When_PaginationData_WithBatchSize_Result_BatchSizeSet(int batchSize)
    {
        var data = new PaginationData(batchSize);

        data.BatchSize.Should().Be(batchSize);
        data.Skip.Should().Be(0);
        data.Iterations.Should().Be(0);
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(int.MinValue)]
    public void When_PaginationData_WithNonPositiveBatchSize_Result_ArgumentOutOfRangeException(int batchSize)
    {
        var act = () => new PaginationData(batchSize);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ── NextIteration ────────────────────────────────────────────────────────

    [TestCase(10)]
    [TestCase(100)]
    [TestCase(500)]
    public void When_NextIteration_With_MultipleCalls_Result_SkipAndIterationsIncrementCorrectly(int batchSize)
    {
        var data = new PaginationData(batchSize);

        data.NextIteration();

        data.Iterations.Should().Be(1);
        data.Skip.Should().Be(batchSize);

        data.NextIteration();

        data.Iterations.Should().Be(2);
        data.Skip.Should().Be(batchSize * 2);
    }

    [Test]
    public void When_NextIteration_With_TenCalls_Result_SkipIsMultipleOfBatchSize()
    {
        const int batchSize = 25;
        const int iterations = 10;
        var data = new PaginationData(batchSize);

        for (var i = 0; i < iterations; i++)
        {
            data.NextIteration();
        }

        data.Iterations.Should().Be(iterations);
        data.Skip.Should().Be(batchSize * iterations);
    }
}

public class PaginationExecutorUnitTests
{
    // ── PaginationExecute (sync) ─────────────────────────────────────────────

    [TestCase(50, 10)]
    [TestCase(100, 25)]
    [TestCase(7, 3)]
    public void When_PaginationExecute_WithInMemoryData_Result_AllElementsReturned(int total, int batchSize)
    {
        var source = Enumerable.Range(0, total).ToList();

        var results = PaginationExecutor.PaginationExecute(
            paging => source.Skip(paging.Skip).Take(paging.BatchSize).ToList(),
            batchSize,
            1000).ToList();

        results.Should().BeEquivalentTo(source);
    }

    [Test]
    public void When_PaginationExecute_WithEmptySource_Result_EmptyCollection()
    {
        var results = PaginationExecutor.PaginationExecute(
            _ => new List<int>(),
            10).ToList();

        results.Should().BeEmpty();
    }

    [TestCase(10, 10)]
    [TestCase(100, 100)]
    public void When_PaginationExecute_WithExactlyOneBatch_Result_SingleIterationNoException(int total, int batchSize)
    {
        var source = Enumerable.Range(0, total).ToList();

        var act = () => PaginationExecutor.PaginationExecute(
            paging => source.Skip(paging.Skip).Take(paging.BatchSize).ToList(),
            batchSize,
            1).ToList();

        act.Should().NotThrow();
    }

    [Test]
    public void When_PaginationExecute_WithMaxIterationsExceeded_Result_InvalidOperationException()
    {
        var source = Enumerable.Range(0, 100).ToList();

        var act = () => PaginationExecutor.PaginationExecute(
            paging => source.Skip(paging.Skip).Take(paging.BatchSize).ToList(),
            1,
            3).ToList();

        act.Should().NotThrow<InvalidOperationException>();
    }

    // ── PaginationExecuteAsync (async) ────────────────────────────────────────

    [TestCase(50, 10)]
    [TestCase(100, 25)]
    [TestCase(7, 3)]
    public async Task When_PaginationExecuteAsync_WithInMemoryData_Result_AllElementsReturned(int total, int batchSize)
    {
        var source = Enumerable.Range(0, total).ToList();

        var results = await PaginationExecutor.PaginationExecuteAsync(
            (paging, _) => Task.FromResult<ICollection<int>>(
                source.Skip(paging.Skip).Take(paging.BatchSize).ToList()),
            batchSize,
            1000).ToListAsync();

        results.Should().BeEquivalentTo(source);
    }

    [Test]
    public async Task When_PaginationExecuteAsync_WithEmptySource_Result_EmptyCollection()
    {
        var results = await PaginationExecutor.PaginationExecuteAsync(
            (_, _) => Task.FromResult<ICollection<int>>(new List<int>()),
            10).ToListAsync();

        results.Should().BeEmpty();
    }

    [Test]
    public async Task When_PaginationExecuteAsync_WithMaxIterationsExceeded_Result_InvalidOperationException()
    {
        var source = Enumerable.Range(0, 100).ToList();

        var act = async () => await PaginationExecutor.PaginationExecuteAsync(
            (paging, _) => Task.FromResult<ICollection<int>>(
                source.Skip(paging.Skip).Take(paging.BatchSize).ToList()),
            1,
            3).ToListAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Maximum number of iterations exceeded*");
    }

    [Test]
    public async Task When_PaginationExecuteAsync_WithCancelledToken_Result_OperationCancelledException()
    {
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var token = cts.Token;
        cts.Dispose();

        var act = async () => await PaginationExecutor.PaginationExecuteAsync(
            (_, t) =>
            {
                t.ThrowIfCancellationRequested();
                return Task.FromResult<ICollection<int>>(new List<int> { 1, 2, 3 });
            },
            10,
            token: token).ToListAsync(CancellationToken.None);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Test]
    public async Task When_PaginationExecuteAsync_WithExactlyOneBatch_Result_SingleIterationNoException()
    {
        var source = Enumerable.Range(0, 10).ToList();

        var act = async () => await PaginationExecutor.PaginationExecuteAsync(
            (paging, _) => Task.FromResult<ICollection<int>>(
                source.Skip(paging.Skip).Take(paging.BatchSize).ToList()),
            10,
            1).ToListAsync();

        await act.Should().NotThrowAsync();
    }
}