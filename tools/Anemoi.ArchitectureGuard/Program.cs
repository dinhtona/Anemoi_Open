using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

var scanPath = GetArgument(args, "--path") ?? Directory.GetCurrentDirectory();
var root = Path.GetFullPath(scanPath);

if (!Directory.Exists(root))
{
    Console.Error.WriteLine($"Scan path does not exist: {root}");
    return 2;
}

var exemptions = GuardExemptions.Load(root);
foreach (var warning in exemptions.Warnings)
{
    Console.Error.WriteLine($"Exemption warning: {warning}");
}

var violations = new List<Violation>();
var files = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
    .Where(file => !IsExcluded(file))
    .Where(file => !exemptions.IsScanExcluded(file))
    .ToArray();

foreach (var file in files)
{
    var relativePath = ToRelativePath(root, file);
    var text = File.ReadAllText(file);
    var tree = CSharpSyntaxTree.ParseText(text, path: file);
    var rootNode = tree.GetCompilationUnitRoot();

    CheckControllerDbContext(relativePath, rootNode, violations);
    CheckGuidNewGuid(relativePath, file, rootNode, exemptions, violations);
    CheckAutoMapper(relativePath, rootNode, violations);
    CheckRoleBasedAuthorize(relativePath, rootNode, violations);
    CheckControllerBusinessLogic(relativePath, rootNode, violations);
}

if (violations.Count == 0)
{
    Console.WriteLine("Architecture Guard PASS");
    Console.WriteLine($"Scanned C# files: {files.Length}");
    Console.WriteLine($"Exemptions: {exemptions.Summary}");
    return 0;
}

Console.Error.WriteLine("Architecture Guard FAILED");
foreach (var violation in violations.OrderBy(v => v.File).ThenBy(v => v.Rule))
{
    Console.Error.WriteLine($"{violation.Rule} | {violation.File} | {violation.Message}");
}

return 1;

static string? GetArgument(string[] args, string name)
{
    for (var i = 0; i < args.Length - 1; i++)
    {
        if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
        {
            return args[i + 1];
        }
    }

    return null;
}

static string ToRelativePath(string root, string file)
{
    return Path.GetRelativePath(root, file).Replace('\\', '/');
}

static bool IsExcluded(string file)
{
    var normalized = file.Replace('\\', '/');
    return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
        || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
        || normalized.Contains("/.git/", StringComparison.OrdinalIgnoreCase)
        || normalized.Contains("/node_modules/", StringComparison.OrdinalIgnoreCase)
        || normalized.Contains("/Migrations/", StringComparison.OrdinalIgnoreCase)
        || normalized.Contains("/Migration", StringComparison.OrdinalIgnoreCase);
}

static bool IsTestFile(string path)
{
    return path.Contains("Test", StringComparison.OrdinalIgnoreCase)
        || path.Contains("Tests", StringComparison.OrdinalIgnoreCase);
}

static void CheckControllerDbContext(string path, CompilationUnitSyntax root, List<Violation> violations)
{
    if (!path.EndsWith("Controller.cs", StringComparison.OrdinalIgnoreCase))
    {
        return;
    }

    var containsDbContextType = root.DescendantNodes()
        .OfType<IdentifierNameSyntax>()
        .Any(node => node.Identifier.ValueText.EndsWith("DbContext", StringComparison.Ordinal));

    if (containsDbContextType)
    {
        violations.Add(new("ANEMOI001", path, "Controller references DbContext. Controllers must delegate to CQRS handlers."));
    }
}

static void CheckGuidNewGuid(string path, string fullPath, CompilationUnitSyntax root, GuardExemptions exemptions, List<Violation> violations)
{
    if (IsTestFile(path))
    {
        return;
    }

    if (exemptions.IsGuidNewGuidExempt(fullPath))
    {
        return;
    }

    var calls = root.DescendantNodes()
        .OfType<InvocationExpressionSyntax>()
        .Where(invocation => invocation.Expression is MemberAccessExpressionSyntax memberAccess
            && memberAccess.Name.Identifier.ValueText == "NewGuid"
            && memberAccess.Expression.ToString() == "Guid");

    foreach (var _ in calls)
    {
        violations.Add(new("ANEMOI002", path, "Production code calls Guid.NewGuid(). Use IdGenerator.NextGuid() where generated GUIDs are required by project convention."));
        return;
    }
}

static void CheckAutoMapper(string path, CompilationUnitSyntax root, List<Violation> violations)
{
    var hasAutoMapperUsing = root.Usings.Any(usingDirective => usingDirective.Name?.ToString() == "AutoMapper");
    var hasIMapper = root.DescendantNodes()
        .OfType<IdentifierNameSyntax>()
        .Any(identifier => identifier.Identifier.ValueText == "IMapper");

    if (hasAutoMapperUsing || hasIMapper)
    {
        violations.Add(new("ANEMOI003", path, "AutoMapper usage detected. Use Mapperly according to project rules."));
    }
}

static void CheckRoleBasedAuthorize(string path, CompilationUnitSyntax root, List<Violation> violations)
{
    var authorizeAttributes = root.DescendantNodes()
        .OfType<AttributeSyntax>()
        .Where(attribute => attribute.Name.ToString().Contains("Authorize", StringComparison.Ordinal));

    foreach (var attribute in authorizeAttributes)
    {
        var text = attribute.ToString();
        if (!text.Contains("Roles", StringComparison.Ordinal))
            continue;

        // ADR-036: the reserved SystemRoles.Administrator check on trust-anchor
        // endpoints is a sanctioned exception. Exempt exactly that constant —
        // any other Roles usage is still flagged.
        if (IsAdministratorOnlyRolesArgument(attribute))
            continue;

        violations.Add(new("ANEMOI004", path, "Role-name-based authorization detected. Use permission constants and [HasPermission]. See ADR-036 for the SystemRoles.Administrator exception."));
        return;
    }
}

static bool IsAdministratorOnlyRolesArgument(AttributeSyntax attribute)
{
    if (attribute.ArgumentList is null)
        return false;

    foreach (var argument in attribute.ArgumentList.Arguments)
    {
        if (argument.NameEquals?.Name.Identifier.ValueText != "Roles")
            continue;

        // Exempt only the exact sanctioned constant; e.g. Roles = "Manager"
        // or Roles = SystemRoles.SomethingElse must still be flagged.
        return argument.Expression.ToString() == "SystemRoles.Administrator";
    }

    return false;
}

static void CheckControllerBusinessLogic(string path, CompilationUnitSyntax root, List<Violation> violations)
{
    if (!path.EndsWith("Controller.cs", StringComparison.OrdinalIgnoreCase))
    {
        return;
    }

    var methodBodies = root.DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Where(method => method.Body is not null)
        .Select(method => method.Body!)
        .ToArray();

    foreach (var body in methodBodies)
    {
        var hasLoop = body.DescendantNodes().Any(node => node is ForStatementSyntax or ForEachStatementSyntax or WhileStatementSyntax);
        var hasTransactionLikeCall = body.ToString().Contains("SaveChanges", StringComparison.Ordinal)
            || body.ToString().Contains("BeginTransaction", StringComparison.Ordinal);

        if (hasLoop || hasTransactionLikeCall)
        {
            violations.Add(new("ANEMOI006", path, "Controller contains logic that looks too heavy. Keep controllers thin and delegate to CQRS handlers."));
            return;
        }
    }
}

internal sealed record Violation(string Rule, string File, string Message);

/// <summary>
/// Shared, documented exemptions loaded from scripts/architecture-guard-exemptions.json.
/// Both this analyzer and scripts/architecture-guard.ps1 read the same file so the
/// PowerShell pre-check and the Roslyn pass cannot drift apart.
/// </summary>
internal sealed class GuardExemptions
{
    private const string RelativeJsonPath = "scripts/architecture-guard-exemptions.json";

    private readonly HashSet<string> _guidExemptions;
    private readonly string[] _excludedScanPrefixes;

    private GuardExemptions(HashSet<string> guidExemptions, string[] excludedScanPrefixes, string? sourcePath, IReadOnlyList<string> warnings)
    {
        _guidExemptions = guidExemptions;
        _excludedScanPrefixes = excludedScanPrefixes;
        SourcePath = sourcePath;
        Warnings = warnings;
    }

    public string? SourcePath { get; }

    public string? RepoRoot => SourcePath is null
        ? null
        : Path.GetDirectoryName(Path.GetDirectoryName(SourcePath));

    public IReadOnlyList<string> Warnings { get; }

    public string Summary => SourcePath is null
        ? "none (exemptions file not found)"
        : $"{_guidExemptions.Count} Guid.NewGuid() file(s), {_excludedScanPrefixes.Length} excluded scan prefix(es)";

    public static GuardExemptions Load(string scanRoot)
    {
        var defaults = new GuardExemptions(new HashSet<string>(StringComparer.OrdinalIgnoreCase), [], null, []);
        var jsonPath = FindExemptionsFile(scanRoot) ?? FindExemptionsFile(AppContext.BaseDirectory);
        if (jsonPath is null)
        {
            return defaults;
        }

        ExemptionsDocument? document;
        try
        {
            document = JsonSerializer.Deserialize<ExemptionsDocument>(File.ReadAllText(jsonPath), SerializerOptions);
        }
        catch (Exception exception) when (exception is JsonException or IOException)
        {
            return new GuardExemptions(defaults._guidExemptions, defaults._excludedScanPrefixes, jsonPath,
                [$"Exemptions file could not be read: {exception.Message}"]);
        }

        if (document is null)
        {
            return new GuardExemptions(defaults._guidExemptions, defaults._excludedScanPrefixes, jsonPath,
                ["Exemptions file is empty."]);
        }

        var warnings = new List<string>();
        var guidExemptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in document.GuidNewGuidExemptions ?? [])
        {
            var normalized = NormalizePath(entry.Path);
            if (normalized.Length == 0)
            {
                warnings.Add("Skipped a Guid.NewGuid() exemption without a path.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(entry.Reason))
            {
                warnings.Add($"Guid.NewGuid() exemption has no reason: {normalized}");
            }

            guidExemptions.Add(normalized);
        }

        var prefixes = (document.ExcludedScanPrefixes ?? [])
            .Select(prefix => NormalizePath(prefix))
            .Where(prefix => prefix.Length > 0)
            .Select(prefix => prefix.EndsWith('/') ? prefix : prefix + "/")
            .ToArray();

        return new GuardExemptions(guidExemptions, prefixes, jsonPath, warnings);
    }

    public bool IsGuidNewGuidExempt(string pathOrFullPath)
    {
        return _guidExemptions.Contains(ToKey(pathOrFullPath));
    }

    public bool IsScanExcluded(string pathOrFullPath)
    {
        var key = ToKey(pathOrFullPath);
        return _excludedScanPrefixes.Any(prefix => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Exemption entries are repository-relative. A caller may pass either a
    /// repository-relative path or an absolute one; absolute paths are rebased on the
    /// directory that holds the exemptions file so scanning a subfolder still matches.
    /// </summary>
    private string ToKey(string pathOrFullPath)
    {
        if (RepoRoot is not null && Path.IsPathRooted(pathOrFullPath))
        {
            return NormalizePath(Path.GetRelativePath(RepoRoot, pathOrFullPath));
        }

        return NormalizePath(pathOrFullPath);
    }

    private static string? FindExemptionsFile(string startDirectory)
    {
        var directory = new DirectoryInfo(startDirectory);
        for (var depth = 0; directory is not null && depth < 6; depth++, directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, RelativeJsonPath.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static string NormalizePath(string? path)
    {
        return (path ?? string.Empty).Replace('\\', '/').Trim().TrimStart('.', '/').TrimEnd('/');
    }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private sealed class ExemptionsDocument
    {
        public string[]? ExcludedScanPrefixes { get; set; }

        public GuidExemptionEntry[]? GuidNewGuidExemptions { get; set; }
    }

    private sealed class GuidExemptionEntry
    {
        public string? Path { get; set; }

        public string? Reason { get; set; }
    }
}
