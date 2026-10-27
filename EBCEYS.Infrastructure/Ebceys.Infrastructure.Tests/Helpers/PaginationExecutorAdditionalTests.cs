using AwesomeAssertions;
using Ebceys.Infrastructure.Helpers;
using Ebceys.Infrastructure.Helpers.Sequences;
using Ebceys.Infrastructure.TestApplication.DaL;
using Ebceys.Tests.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Ebceys.Infrastructure.Tests.Helpers;

public class PaginationExecutorAdditionalTests
{
    private static readonly EbRandomizer Randomizer = new();
    private IDbContextFactory<DataModelContext> _contextFactory;
    private AtomicIntGenerator _seqGen;

    [SetUp]
    public void SetUp()
    {
        _contextFactory = AppTestContext.AppContext.Factory.Services
            .GetRequiredService<IDbContextFactory<DataModelContext>>();
        _seqGen = new AtomicIntGenerator();
    }

    [TearDown]
    public void TearDown()
    {
        using var dbContext = _contextFactory.CreateDbContext();
        dbContext.TestTable.ExecuteDelete();
    }

    // ── Empty table ───────────────────────────────────────────────────────────

    [Test]
    public async Task When_PaginationExecuteAsync_WithEmptyTable_Result_EmptyCollection()
    {
        var result = await PaginationExecutor.PaginationExecuteAsync<TestTableDbo>(
            async (pages, token) =>
                await _contextFactory.CreateDbContext().TestTable
                    .OrderBy(x => x.Id).Skip(pages.Skip).Take(pages.BatchSize).ToArrayAsync(token),
            100).ToArrayAsync();

        result.Should().BeEmpty();
    }

    [Test]
    public void When_PaginationExecute_WithEmptyTable_Result_EmptyCollection()
    {
        var result = PaginationExecutor.PaginationExecute<TestTableDbo>(
            pages => _contextFactory.CreateDbContext().TestTable
                .OrderBy(x => x.Id).Skip(pages.Skip).Take(pages.BatchSize).ToArray(),
            100).ToArray();

        result.Should().BeEmpty();
    }

    // ── Fewer elements than batchSize ─────────────────────────────────────────

    [TestCase(5, 100)]
    [TestCase(1, 50)]
    public async Task When_PaginationExecuteAsync_WithFewerElementsThanBatchSize_Result_AllElementsReturnedInOneBatch(
        int numOfElements, int batchSize)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync();
        var data = GenerateData(numOfElements);
        await dbContext.TestTable.AddRangeAsync(data);
        await dbContext.SaveChangesAsync();

        var iterationCount = 0;
        var result = await PaginationExecutor.PaginationExecuteAsync<TestTableDbo>(
            async (pages, token) =>
            {
                iterationCount++;
                return await _contextFactory.CreateDbContext().TestTable
                    .OrderBy(x => x.Id).Skip(pages.Skip).Take(pages.BatchSize).ToArrayAsync(token);
            },
            batchSize).ToArrayAsync();

        result.Should().HaveCount(numOfElements);
        iterationCount.Should().Be(1);
    }

    [TestCase(5, 100)]
    [TestCase(1, 50)]
    public void When_PaginationExecute_WithFewerElementsThanBatchSize_Result_AllElementsReturnedInOneBatch(
        int numOfElements, int batchSize)
    {
        using var dbContext = _contextFactory.CreateDbContext();
        var data = GenerateData(numOfElements);
        dbContext.TestTable.AddRange(data);
        dbContext.SaveChanges();

        var iterationCount = 0;
        var result = PaginationExecutor.PaginationExecute<TestTableDbo>(
            pages =>
            {
                iterationCount++;
                return _contextFactory.CreateDbContext().TestTable
                    .OrderBy(x => x.Id).Skip(pages.Skip).Take(pages.BatchSize).ToArray();
            },
            batchSize).ToArray();

        result.Should().HaveCount(numOfElements);
        iterationCount.Should().Be(1);
    }

    // ── Exact multiple of batchSize ───────────────────────────────────────────

    [TestCase(100, 10)]
    [TestCase(50, 25)]
    public async Task When_PaginationExecuteAsync_WithExactMultipleOfBatchSize_Result_AllElementsReturned(
        int numOfElements, int batchSize)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync();
        var data = GenerateData(numOfElements);
        await dbContext.TestTable.AddRangeAsync(data);
        await dbContext.SaveChangesAsync();

        var result = await PaginationExecutor.PaginationExecuteAsync<TestTableDbo>(
            async (pages, token) =>
                await _contextFactory.CreateDbContext().TestTable
                    .OrderBy(x => x.Id).Skip(pages.Skip).Take(pages.BatchSize).ToArrayAsync(token),
            batchSize, numOfElements + 1).ToArrayAsync();

        result.Should().HaveCount(numOfElements);
        result.Select(x => x.Id).Should().OnlyHaveUniqueItems();
    }

    // ── Cancellation mid-pagination ───────────────────────────────────────────

    [Test]
    public async Task When_PaginationExecuteAsync_WithCancellationMidway_Result_OperationCancelledException()
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync();
        var data = GenerateData(500);
        await dbContext.TestTable.AddRangeAsync(data);
        await dbContext.SaveChangesAsync();

        var cts = new CancellationTokenSource();
        var token = cts.Token;
        var callCount = 0;

        var act = async () => await PaginationExecutor.PaginationExecuteAsync<TestTableDbo>(
            async (pages, ct) =>
            {
                callCount++;
                if (callCount >= 2)
                {
                    await cts.CancelAsync();
                    ct.ThrowIfCancellationRequested();
                }

                return await _contextFactory.CreateDbContext().TestTable
                    .OrderBy(x => x.Id).Skip(pages.Skip).Take(pages.BatchSize).ToArrayAsync(ct);
            },
            100,
            token: token).ToArrayAsync(CancellationToken.None);

        await act.Should().ThrowAsync<OperationCanceledException>();
        cts.Dispose();
    }

    // ── Single-element batchSize (stress) ────────────────────────────────────

    [TestCase(10)]
    public async Task When_PaginationExecuteAsync_WithBatchSizeOne_Result_AllElementsReturned(int numOfElements)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync();
        var data = GenerateData(numOfElements);
        await dbContext.TestTable.AddRangeAsync(data);
        await dbContext.SaveChangesAsync();

        var result = await PaginationExecutor.PaginationExecuteAsync<TestTableDbo>(
            async (pages, token) =>
                await _contextFactory.CreateDbContext().TestTable
                    .OrderBy(x => x.Id).Skip(pages.Skip).Take(pages.BatchSize).ToArrayAsync(token),
            1,
            numOfElements + 1).ToArrayAsync();

        result.Should().HaveCount(numOfElements);
    }

    // ── Ordering preserved ────────────────────────────────────────────────────

    [Test]
    public async Task When_PaginationExecuteAsync_WithOrderedQuery_Result_OrderIsPreserved()
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync();
        var data = GenerateData(30);
        await dbContext.TestTable.AddRangeAsync(data);
        await dbContext.SaveChangesAsync();

        var result = await PaginationExecutor.PaginationExecuteAsync<TestTableDbo>(
            async (pages, token) =>
                await _contextFactory.CreateDbContext().TestTable
                    .OrderBy(x => x.Id).Skip(pages.Skip).Take(pages.BatchSize).ToArrayAsync(token),
            10).ToArrayAsync();

        result.Should().HaveCount(30);
        result.Select(x => x.Id).Should().BeInAscendingOrder();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private TestTableDbo[] GenerateData(int count)
    {
        return Enumerable.Range(0, count).Select(_ => new TestTableDbo
        {
            Id = _seqGen.Next(),
            Name = Randomizer.String(8)
        }).ToArray();
    }
}