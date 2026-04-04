namespace Anemoi.Contract.MasterData.Responses;

public sealed class DbSchemaResponse
{
    public List<TableSchema> Tables { get; set; } = new();
}

public sealed class TableSchema
{
    public string TableName { get; set; }
    public List<ColumnSchema> Columns { get; set; } = new();
}

public sealed class ColumnSchema
{
    public string ColumnName { get; set; }
    public string DataType { get; set; }
    public bool IsNullable { get; set; }
}
