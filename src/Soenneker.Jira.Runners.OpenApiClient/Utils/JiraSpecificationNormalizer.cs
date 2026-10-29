using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Jira.Runners.OpenApiClient.Utils;

/// <summary>Combines equivalent Jira path templates without dropping their operations.</summary>
public static class JiraSpecificationNormalizer
{
    public static async Task Normalize(string source, string destination, CancellationToken cancellationToken = default)
    {
        JsonNode document = JsonNode.Parse(await File.ReadAllTextAsync(source, cancellationToken))
            ?? throw new InvalidOperationException("The Jira specification is empty.");
        JsonObject paths = document["paths"]?.AsObject()
            ?? throw new InvalidOperationException("The Jira specification has no paths.");

        Merge(paths, "/rest/api/3/field/{id}", "/rest/api/3/field/{fieldId}", "id", "fieldId");
        Merge(paths, "/rest/api/3/issuesecurityschemes/{schemeId}", "/rest/api/3/issuesecurityschemes/{id}", "schemeId", "id");
        Merge(paths, "/rest/api/3/notificationscheme/{notificationSchemeId}", "/rest/api/3/notificationscheme/{id}", "notificationSchemeId", "id");

        await File.WriteAllTextAsync(destination, document.ToJsonString(new JsonSerializerOptions { WriteIndented = true }), cancellationToken);
    }

    private static void Merge(JsonObject paths, string source, string destination, string oldName, string newName)
    {
        if (paths[source] is not JsonObject sourceItem || paths[destination] is not JsonObject destinationItem)
            return;

        foreach (var member in sourceItem)
        {
            if (destinationItem.ContainsKey(member.Key))
                throw new InvalidOperationException($"Conflicting Jira path member {member.Key}: {source} and {destination}.");

            JsonNode? copy = member.Value?.DeepClone();
            RenamePathParameter(copy, oldName, newName);
            destinationItem.Add(member.Key, copy);
        }

        paths.Remove(source);
    }

    private static void RenamePathParameter(JsonNode? node, string oldName, string newName)
    {
        if (node is JsonObject obj)
        {
            if (obj["in"]?.ToString() == "path" && obj["name"]?.ToString() == oldName)
                obj["name"] = newName;

            foreach (var property in obj)
                RenamePathParameter(property.Value, oldName, newName);
        }
        else if (node is JsonArray array)
        {
            foreach (JsonNode? child in array)
                RenamePathParameter(child, oldName, newName);
        }
    }
}
