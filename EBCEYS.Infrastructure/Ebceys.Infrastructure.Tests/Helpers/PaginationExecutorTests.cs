using AwesomeAssertions;
using Ebceys.Infrastructure.Helpers;
using Ebceys.Infrastructure.Helpers.Sequences;
using Ebceys.Infrastructure.TestApplication.DaL;
using Ebceys.Tests.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Ebceys.Infrastructure.Tests.Helpers;

public class PaginationExecutorTests
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
        ClearDb();
    }

    private void ClearDb()
    {
        var dbContext = _contextFactory.CreateDbContext();
        dbContext.TestTable.ExecuteDelete();
    }

    [TestCase(10000, 100)]
    public async Task
        When_GetElementsWithPaginationAsync_With_SpecifiedNumOfElementsAndBatchSize_Result_GotDataCorrectly(
            int numOfElements, int batchSize)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync();

        var data = Enumerable.Range(0, numOfElements).Select(_ => new TestTableDbo
        {
            Id = _seqGen.Next(),
            Name = Randomizer.String(8)
        }).ToArray();
        await dbContext.TestTable.AddRangeAsync(data);
        await dbContext.SaveChangesAsync();

        var res = await PaginationExecutor.PaginationExecuteAsync<TestTableDbo>(
            async (pages, token) =>
            {
                return await dbContext.TestTable.OrderBy(x => x.Id).Skip(pages.Skip).Take(pages.BatchSize)
                    .ToArrayAsync(token);
            }, batchSize, 10000, CancellationToken.None).ToArrayAsync();

        data.Should().BeEquivalentTo(res);
    }

    [TestCase(10000, 1)]
    public async Task
        When_GetElementsWithPaginationAsyncReachMaxIterations_With_SpecifiedNumOfElementsAndBatchSize_Result_GotDataCorrectly(
            int numOfElements, int batchSize)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync();

        var data = Enumerable.Range(0, numOfElements).Select(_ => new TestTableDbo
        {
            Id = _seqGen.Next(),
            Name = Randomizer.String(8)
        }).ToArray();
        await dbContext.TestTable.AddRangeAsync(data);
        await dbContext.SaveChangesAsync();

        var act = async () =>
        {
            return await PaginationExecutor.PaginationExecuteAsync<TestTableDbo>(
                async (pages, token) =>
                {
                    return await dbContext.TestTable.OrderBy(x => x.Id).Skip(pages.Skip).Take(pages.BatchSize)
                        .ToArrayAsync(token);
                }, batchSize).ToArrayAsync();
        };

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [TestCase(10000, 100)]
    public async Task When_GetElementsWithPagination_With_SpecifiedNumOfElementsAndBatchSize_Result_GotDataCorrectly(
        int numOfElements, int batchSize)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync();

        var data = Enumerable.Range(0, numOfElements).Select(_ => new TestTableDbo
        {
            Id = _seqGen.Next(),
            Name = Randomizer.String(8)
        }).ToArray();
        await dbContext.TestTable.AddRangeAsync(data);
        await dbContext.SaveChangesAsync();

        var res = PaginationExecutor
            .PaginationExecute<TestTableDbo>(
                pages =>
                {
                    return dbContext.TestTable.OrderBy(x => x.Id).Skip(pages.Skip).Take(pages.BatchSize).ToArray();
                }, batchSize, 10000).ToArray();

        data.Should().BeEquivalentTo(res);
    }

    [TestCase(10000, 1)]
    public async Task
        When_GetElementsWithPaginationReachMaxIterations_With_SpecifiedNumOfElementsAndBatchSize_Result_GotDataCorrectly(
            int numOfElements, int batchSize)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync();

        var data = Enumerable.Range(0, numOfElements).Select(_ => new TestTableDbo
        {
            Id = _seqGen.Next(),
            Name = Randomizer.String(8)
        }).ToArray();
        await dbContext.TestTable.AddRangeAsync(data);
        await dbContext.SaveChangesAsync();

        var act = () =>
        {
            return PaginationExecutor
                .PaginationExecute<TestTableDbo>(
                    pages =>
                    {
                        return dbContext.TestTable.OrderBy(x => x.Id).Skip(pages.Skip).Take(pages.BatchSize)
                            .ToArray();
                    }, batchSize, 1).ToArray();
        };

        act.Should().NotThrow<InvalidOperationException>();
    }
}