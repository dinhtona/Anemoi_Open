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

var violations = new List<Violation>();
var files = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
    .Where(file => !IsExcluded(file))
    .ToArray();

foreach (var file in files)
{
    var relativePath = Path.GetRelativePath(root, file).Replace('\\', '/');
    var text = File.ReadAllText(file);
    var tree = CSharpSyntaxTree.ParseText(text, path: file);
    var rootNode = tree.GetCompilationUnitRoot();

    CheckControllerDbContext(relativePath, rootNode, violations);
    CheckGuidNewGuid(relativePath, rootNode, violations);
    CheckAutoMapper(relativePath, rootNode, violations);
    CheckRoleBasedAuthorize(relativePath, rootNode, violations);
    CheckControllerBusinessLogic(relativePath, rootNode, violations);
}

if (violations.Count == 0)
{
    Console.WriteLine("Architecture Guard PASS");
    Console.WriteLine($"Scanned C# files: {files.Length}");
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

static void CheckGuidNewGuid(string path, CompilationUnitSyntax root, List<Violation> violations)
{
    if (IsTestFile(path))
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
        if (text.Contains("Roles", StringComparison.Ordinal))
        {
            violations.Add(new("ANEMOI004", path, "Role-name-based authorization detected. Use permission constants and [HasPermission]."));
            return;
        }
    }
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
