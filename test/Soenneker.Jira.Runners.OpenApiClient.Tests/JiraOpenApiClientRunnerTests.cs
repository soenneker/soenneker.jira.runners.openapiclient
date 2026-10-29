using Soenneker.Tests.HostedUnit;

namespace Soenneker.Jira.Runners.OpenApiClient.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class JiraOpenApiClientRunnerTests : HostedUnitTest
{
    public JiraOpenApiClientRunnerTests(Host host) : base(host)
    {
    }

    [Test]
    public void Default()
    {

    }
}
