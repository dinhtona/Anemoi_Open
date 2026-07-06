using System.Text.RegularExpressions;
using Xunit;

namespace Anemoi.Hr.Test.Application.Workflow;

public class WorkflowStartAsyncStaticAnalysisTests
{
    private static readonly string ApplicationSourceDir = FindApplicationSourceDir();

    private static string FindApplicationSourceDir()
    {
        var assemblyDir = AppDomain.CurrentDomain.BaseDirectory;
        var dir = new DirectoryInfo(assemblyDir);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "Anemoi.Hr.Application");
            if (Directory.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }

        var repoRoot = Path.GetFullPath(
            Path.Combine(assemblyDir, "..", "..", "..", "..", "..", ".."));
        return Path.Combine(repoRoot, "Anemoi.Hr", "Anemoi.Hr.Application");
    }

    [Fact]
    public void All_StartAsync_Calls_In_Handlers_Must_Check_Result()
    {
        if (!Directory.Exists(ApplicationSourceDir))
            return;

        var sourceFiles = Directory.GetFiles(
            ApplicationSourceDir, "*Handler.cs", SearchOption.AllDirectories);

        var violations = new List<string>();

        foreach (var file in sourceFiles)
        {
            var content = File.ReadAllText(file);
            var lines = content.Split('\n');

            for (var i = 0; i < lines.Length; i++)
            {
                var trimmed = lines[i].Trim();
                if (!trimmed.Contains("workflowEngine.StartAsync(") &&
                    !trimmed.Contains("workflowEngine.StartAsync ("))
                    continue;

                if (trimmed.StartsWith("//"))
                    continue;

                var resultCaptured = Regex.IsMatch(trimmed,
                    @"\bvar\s+\w+\s*=\s*await\s+workflowEngine\.StartAsync");

                if (!resultCaptured)
                {
                    violations.Add(
                        $"{Path.GetFileName(file)}:{i + 1} — StartAsync result not captured to variable");
                    continue;
                }

                var hasCheckInNextLines = false;
                for (var j = i + 1; j < Math.Min(i + 15, lines.Length); j++)
                {
                    if (lines[j].Contains("TryPickT1"))
                    {
                        hasCheckInNextLines = true;
                        break;
                    }
                }

                if (!hasCheckInNextLines)
                {
                    violations.Add(
                        $"{Path.GetFileName(file)}:{i + 1} — StartAsync called but no TryPickT1 check within 15 lines");
                }
            }
        }

        if (violations.Count > 0)
        {
            var message = "The following StartAsync calls do not properly check the result:\n"
                + string.Join("\n", violations);
            Assert.Fail(message);
        }
    }
}
