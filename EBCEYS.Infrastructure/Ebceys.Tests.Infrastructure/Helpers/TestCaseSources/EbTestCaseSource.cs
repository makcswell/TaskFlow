using System.Collections;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Ebceys.Tests.Infrastructure.Helpers.TestCaseSources;

/// <summary>
///     Abstract base class for NUnit parameterized test case sources. Provides a pattern for defining
///     valid and invalid test data generators with a shared base data setup.
///     Used with <see cref="EbTestDataValidExecutor{TEnumerator,TData}" /> and
///     <see cref="EbTestDataInvalidExecutor{TEnumerator,TData}" />.
/// </summary>
/// <typeparam name="TData">The type of test data.</typeparam>
[PublicAPI]
public abstract class EbTestCaseSource<TData>
{
    /// <summary>
    ///     Setups the valid base data that serves as the starting point for valid test case transformations.
    /// </summary>
    /// <returns>A new instance of <typeparamref name="TData" /> representing valid base data.</returns>
    public abstract TData SetupValidBase();

    /// <summary>
    ///     Setups the invalid base data that serves as the starting point for invalid test case transformations.
    /// </summary>
    /// <returns>A new instance of <typeparamref name="TData" /> representing invalid base data, or <c>null</c>.</returns>
    public abstract TData? SetupInvalidBase();

    /// <summary>
    ///     Gets the collection of test case transformations that should produce valid data.
    /// </summary>
    /// <returns>An enumerable of tuples containing the data transformation function and a test case description.</returns>
    public abstract IEnumerable<(Func<TData?, TData?>, string)> GetValidCases();

    /// <summary>
    ///     Gets the collection of test case transformations that should produce invalid data.
    /// </summary>
    /// <returns>An enumerable of tuples containing the data transformation function and a test case description.</returns>
    public abstract IEnumerable<(Func<TData?, TData?>, string)> GetInvalidCases();
}

/// <summary>
///     NUnit test case enumerator that generates valid test cases from <see cref="EbTestCaseSource{TData}" />.
///     Yields the valid base data plus all cases returned by <see cref="EbTestCaseSource{TData}.GetValidCases" />.
/// </summary>
/// <typeparam name="TData">The type of test data.</typeparam>
/// <typeparam name="TEnumerator">The <see cref="EbTestCaseSource{TData}" /> implementation providing valid cases.</typeparam>
[PublicAPI]
public sealed class EbTestDataValidExecutor<TEnumerator, TData> : TestCaseEnumerator<TData>
    where TEnumerator : EbTestCaseSource<TData>
{
    private readonly TEnumerator _testCaseSource;

    /// <summary>
    ///     Initiates the new instance of <see cref="EbTestDataValidExecutor{TEnumerator,TData}" />.
    /// </summary>
    public EbTestDataValidExecutor()
    {
        _testCaseSource = Activator.CreateInstance<TEnumerator>();
    }

    /// <inheritdoc />
    protected override (IEnumerable<(Func<TData?, TData?>, string)> dataSource, TData? baseData) GetData()
    {
        return (GetCases(), _testCaseSource.SetupValidBase());
    }

    private IEnumerable<(Func<TData?, TData?>, string)> GetCases()
    {
        yield return (_ => _testCaseSource.SetupValidBase(), "Valid_Base");
        foreach (var validCase in _testCaseSource.GetValidCases())
        {
            yield return validCase;
        }
    }
}

/// <summary>
///     NUnit test case enumerator that generates invalid test cases from <see cref="EbTestCaseSource{TData}" />.
///     Yields all cases returned by <see cref="EbTestCaseSource{TData}.GetInvalidCases" /> using the invalid base data.
/// </summary>
/// <typeparam name="TData">The type of test data.</typeparam>
/// <typeparam name="TEnumerator">The <see cref="EbTestCaseSource{TData}" /> implementation providing invalid cases.</typeparam>
[PublicAPI]
public sealed class EbTestDataInvalidExecutor<TEnumerator, TData> : TestCaseEnumerator<TData>
    where TEnumerator : EbTestCaseSource<TData>
{
    private readonly TEnumerator _testCaseSource;

    /// <summary>
    ///     Initiates the new instance of <see cref="EbTestDataInvalidExecutor{TEnumerator,TData}" />.
    /// </summary>
    public EbTestDataInvalidExecutor()
    {
        _testCaseSource = Activator.CreateInstance<TEnumerator>();
    }

    /// <inheritdoc />
    protected override (IEnumerable<(Func<TData?, TData?>, string)> dataSource, TData? baseData) GetData()
    {
        return new ValueTuple<IEnumerable<(Func<TData?, TData?>, string)>, TData?>(_testCaseSource.GetInvalidCases(),
            _testCaseSource.SetupInvalidBase());
    }
}

/// <summary>
///     Abstract base class for NUnit <see cref="TestCaseData" /> enumerators that converts
///     test case transformation functions into <see cref="TestCaseData" /> instances.
/// </summary>
/// <typeparam name="TData">The type of test data.</typeparam>
[PublicAPI]
public abstract class TestCaseEnumerator<TData> : IEnumerable<TestCaseData>
{
    /// <summary>
    ///     Initiates the new instance of <see cref="TestCaseEnumerator{TData}" />.
    /// </summary>
    protected TestCaseEnumerator()
    {
        Cases = Enumerate();
    }

    private IEnumerable<TestCaseData> Cases { get; }

    /// <inheritdoc />
    public IEnumerator<TestCaseData> GetEnumerator()
    {
        return Cases.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    ///     Gets the test case data source and the base data for constructing test cases.
    /// </summary>
    /// <returns>A tuple containing the test case transformation functions with descriptions, and the base data instance.</returns>
    protected abstract (IEnumerable<(Func<TData?, TData?>, string)> dataSource, TData? baseData) GetData();

    private IEnumerable<TestCaseData> Enumerate()
    {
        var dataSource = GetData();
        foreach (var testCase in dataSource.dataSource)
        {
            var casted = EbTestCaseData<TData>.CreateFrom(testCase);
            yield return new TestCaseData(casted.PreparedData(dataSource.baseData))
                .SetArgDisplayNames(casted.TestDescription);
        }
    }
}

/// <summary>
///     The test case data.
/// </summary>
/// <param name="PreparedData">The func to prepare data to test case.</param>
/// <param name="TestDescription">The test descriptions.</param>
/// <typeparam name="TData">The data to test.</typeparam>
internal record EbTestCaseData<TData>(Func<TData?, TData?> PreparedData, string TestDescription)
{
    /// <summary>
    ///     Creates the new instance of <see cref="EbTestCaseData{TData}" /> by <paramref name="data" />.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <returns>The new instance of <see cref="EbTestCaseData{TData}" />.</returns>
    public static EbTestCaseData<TData> CreateFrom((Func<TData?, TData?>, string) data)
    {
        return new EbTestCaseData<TData>(data.Item1, data.Item2);
    }
}