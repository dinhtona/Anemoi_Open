using System.Collections.Generic;

namespace Anemoi.MasterData.Application.Models;

public sealed class SeedTemplateConfig
{
    public string ReferenceServerId { get; set; }
    public List<TableConfig> Tables { get; set; } = new();
    public List<RelationshipConfig> Relationships { get; set; } = new();
}

public sealed class TableConfig
{
    public string TableName { get; set; }
    public int Order { get; set; }
    public int RowCount { get; set; }
    public List<ColumnRule> ColumnRules { get; set; } = new();
}

public sealed class ColumnRule
{
    public string ColumnName { get; set; }
    public string RuleType { get; set; } // SUM | STATIC | CUSTOM
    public List<string> Parameters { get; set; } = new();
}

public sealed class RelationshipConfig
{
    public string ParentTable { get; set; }
    public string ChildTable { get; set; }
    public List<JoinKey> JoinKeys { get; set; } = new();
    public string ChildDataStrategy { get; set; } // GENERATE_NEW | COPY_FROM_FIRST_PARENT
}

public sealed class JoinKey
{
    public string ParentColumn { get; set; }
    public string ChildColumn { get; set; }
}
