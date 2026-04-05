using Anemoi.Contract.MasterData.Constants;
using Anemoi.MasterData.Application.Abstractions;
using Dapper;
using Microsoft.Data.SqlClient;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.MasterData.Infrastructure.Services;

public sealed class DataIngestionService : IDataIngestionService
{
    public async Task IngestDataAsync(string connectionString, string provider, string tableName, List<Dictionary<string, object>> data, CancellationToken cancellationToken = default)
    {
        if (data == null || data.Count == 0) return;

        // Default to SqlServer for backward compatibility
        if (string.IsNullOrWhiteSpace(provider)) provider = SeedProviderType.SqlServer;

        if (provider == SeedProviderType.SqlServer)
        {
            await IngestSqlServerAsync(connectionString, tableName, data, cancellationToken);
        }
        else if (provider == SeedProviderType.PostgreSQL)
        {
            await IngestPostgreSqlAsync(connectionString, tableName, data, cancellationToken);
        }
    }

    private async Task IngestSqlServerAsync(string connectionString, string tableName, List<Dictionary<string, object>> data, CancellationToken cancellationToken)
    {
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
        foreach (DataColumn column in dataTable.Columns)
        {
            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
        }

        await bulkCopy.WriteToServerAsync(dataTable, cancellationToken);
    }

    private async Task IngestPostgreSqlAsync(string connectionString, string tableName, List<Dictionary<string, object>> data, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        // Simple Multi-row INSERT approach for PostgreSQL (supports non-SQL Server environments)
        // For very large datasets, NpgsqlBinaryImporter would be better, but this is more flexible
        var columns = data[0].Keys.ToList();
        var columnNames = string.Join(", ", columns.Select(c => $"\"{c}\""));
        
        // Chunk inserts to avoid extremely large parameter lists
        var batchSize = 1000; 
        for (int i = 0; i < data.Count; i += batchSize)
        {
            var batch = data.Skip(i).Take(batchSize).ToList();
            var sql = new StringBuilder($"INSERT INTO \"{tableName}\" ({columnNames}) VALUES ");
            var parameters = new DynamicParameters();

            for (int r = 0; r < batch.Count; r++)
            {
                var rowParams = new List<string>();
                foreach (var col in columns)
                {
                    var paramName = $"p_{r}_{col}";
                    rowParams.Add($"@{paramName}");
                    parameters.Add(paramName, batch[r][col]);
                }
                sql.Append($"({string.Join(", ", rowParams)}){(r == batch.Count - 1 ? "" : ", ")}");
            }

            await connection.ExecuteAsync(sql.ToString(), parameters);
        }
    }
}
