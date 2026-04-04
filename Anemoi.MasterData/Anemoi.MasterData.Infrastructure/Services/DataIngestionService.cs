using System.Data;
using Anemoi.MasterData.Application.Abstractions;
using Microsoft.Data.SqlClient;

namespace Anemoi.MasterData.Infrastructure.Services;

public sealed class DataIngestionService : IDataIngestionService
{
    public async Task IngestDataAsync(string connectionString, string tableName, List<Dictionary<string, object>> data, CancellationToken cancellationToken = default)
    {
        if (data.Count == 0) return;

        var dataTable = new DataTable();
        foreach (var key in data[0].Keys)
        {
            dataTable.Columns.Add(key);
        }

        foreach (var row in data)
        {
            var dataRow = dataTable.NewRow();
            foreach (var kvp in row)
            {
                dataRow[kvp.Key] = kvp.Value ?? DBNull.Value;
            }
            dataTable.Rows.Add(dataRow);
        }

        using var bulkCopy = new SqlBulkCopy(connectionString);
        bulkCopy.DestinationTableName = tableName;
        
        // Add mappings to be safe
        foreach (DataColumn column in dataTable.Columns)
        {
            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
        }

        await bulkCopy.WriteToServerAsync(dataTable, cancellationToken);
    }
}
