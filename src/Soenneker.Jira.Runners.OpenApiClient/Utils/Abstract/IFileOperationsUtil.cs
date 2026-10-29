using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Jira.Runners.OpenApiClient.Utils.Abstract;

/// <summary>Regenerates the API client from Jira's published OpenAPI specification.</summary>
public interface IFileOperationsUtil
{
    /// <summary>
    /// Downloads the specification, regenerates the client, and verifies a Release build.
    /// Set Jira:LocalRepositoryPath to regenerate a local client repository without pushing by default.
    /// Jira:PushChanges explicitly controls publication; Jira:ClientGenerationUrl overrides the specification source.
    /// Throws when generation or compilation fails.
    /// </summary>
    ValueTask Process(CancellationToken cancellationToken = default);
}
