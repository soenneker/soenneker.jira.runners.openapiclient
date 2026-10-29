using System;
using System.IO;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Soenneker.Jira.Runners.OpenApiClient.Utils;

namespace Soenneker.Jira.Runners.OpenApiClient.Tests;

public sealed class JiraSpecificationNormalizerTests
{
    [Test]
    public async Task PreservesOperationsAndRenamesPathParameters()
    {
        string folder = Path.Combine(Path.GetTempPath(), "jira-normalizer-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            string source = Path.Combine(folder, "source.json");
            string target = Path.Combine(folder, "normalized.json");
            await File.WriteAllTextAsync(source, """
                {"paths":{
                  "/rest/api/3/field/{fieldId}":{"put":{"operationId":"updateField"}},
                  "/rest/api/3/field/{id}":{"delete":{"operationId":"deleteField","parameters":[{"in":"path","name":"id"},{"in":"query","name":"id"}]}}
                }}
                """);
            await JiraSpecificationNormalizer.Normalize(source, target);
            JsonNode document = JsonNode.Parse(await File.ReadAllTextAsync(target))!;
            JsonNode path = document["paths"]!["/rest/api/3/field/{fieldId}"]!;
            if (document["paths"]!.AsObject().Count != 1 || path["put"]!["operationId"]!.ToString() != "updateField" ||
                path["delete"]!["operationId"]!.ToString() != "deleteField" ||
                path["delete"]!["parameters"]![0]!["name"]!.ToString() != "fieldId" ||
                path["delete"]!["parameters"]![1]!["name"]!.ToString() != "id")
                throw new Exception("Path normalization dropped or changed an operation or parameter.");
        }
        finally { Directory.Delete(folder, true); }
    }

    [Test]
    public async Task RejectsConflictingOperations()
    {
        string folder = Path.Combine(Path.GetTempPath(), "jira-normalizer-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            string source = Path.Combine(folder, "source.json");
            await File.WriteAllTextAsync(source, """
                {"paths":{
                  "/rest/api/3/field/{fieldId}":{"delete":{"operationId":"first"}},
                  "/rest/api/3/field/{id}":{"delete":{"operationId":"second"}}
                }}
                """);
            try { await JiraSpecificationNormalizer.Normalize(source, Path.Combine(folder, "output.json")); }
            catch (InvalidOperationException) { return; }
            throw new Exception("Conflicting operations were silently merged.");
        }
        finally { Directory.Delete(folder, true); }
    }
}
