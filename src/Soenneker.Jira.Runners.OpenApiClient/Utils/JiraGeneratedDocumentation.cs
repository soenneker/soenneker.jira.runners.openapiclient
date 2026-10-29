using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Jira.Runners.OpenApiClient.Utils;

internal static class JiraGeneratedDocumentation
{
    public static async Task Fix(string sourceDirectory, CancellationToken cancellationToken)
    {
        foreach (string file in Directory.EnumerateFiles(sourceDirectory, "*.cs", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(sourceDirectory, file);
            if (relative.StartsWith("bin" + Path.DirectorySeparatorChar) || relative.StartsWith("obj" + Path.DirectorySeparatorChar))
                continue;

            string source = await File.ReadAllTextAsync(file, cancellationToken);
            string updated = Regex.Replace(source, "cref=\"List<([^\"]+)>\"", "cref=\"System.Collections.Generic.List{$1}\"");
            if (updated != source)
                await File.WriteAllTextAsync(file, updated, cancellationToken);
        }
    }
}
