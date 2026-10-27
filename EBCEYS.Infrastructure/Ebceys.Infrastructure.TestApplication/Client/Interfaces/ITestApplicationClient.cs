using Ebceys.Infrastructure.TestApplication.Client.Interfaces;

namespace Ebceys.Infrastructure.TestApplication.Client.Implementations;

public interface ITestApplicationClient
{
    ITestClient TestClient { get; }
}