using System.Collections.Concurrent;
using AwesomeAssertions;
using Ebceys.Infrastructure.Extensions;
using Ebceys.Infrastructure.Helpers.Sequences;

namespace Ebceys.Infrastructure.UnitTests.Helpers;

public class AtomicLongGeneratorTests
{
    private AtomicIntGenerator _intGen;
    private AtomicLongGenerator _longGen;

    [SetUp]
    public void Setup()
    {
        _longGen = new AtomicLongGenerator();
        _intGen = new AtomicIntGenerator();
    }

    [TestCase(1000, 1000)]
    [TestCase(1000, 10000)]
    [TestCase(1000, 10000)]
    [TestCase(1000, 1000)]
    public async Task When_LongGenManyParallelIncrements_With_SpecifiedNumOfThreadsAndOperations_Result_NoDuplicates(
        int numOfThreads, int numOfOperations)
    {
        var bag = new ConcurrentBag<long>();
        var parallelLoopResult = Parallel.For(0, numOfThreads, _ =>
        {
            for (var i = 0; i < numOfOperations; i++)
            {
                bag.Add(_longGen.Next());
            }
        });

        await Task.WaitUntilAsync(_ => parallelLoopResult.IsCompleted, TimeSpan.FromMinutes(3));

        bag.Should().OnlyHaveUniqueItems();
    }

    [TestCase(1000, 1000)]
    [TestCase(1000, 10000)]
    [TestCase(1000, 10000)]
    [TestCase(1000, 1000)]
    public async Task When_intGenManyParallelIncrements_With_SpecifiedNumOfThreadsAndOperations_Result_NoDuplicates(
        int numOfThreads, int numOfOperations)
    {
        var bag = new ConcurrentBag<int>();
        var parallelLoopResult = Parallel.For(0, numOfThreads, _ =>
        {
            for (var i = 0; i < numOfOperations; i++)
            {
                bag.Add(_intGen.Next());
            }
        });

        await Task.WaitUntilAsync(_ => parallelLoopResult.IsCompleted, TimeSpan.FromMinutes(3));

        bag.Should().OnlyHaveUniqueItems();
    }
}