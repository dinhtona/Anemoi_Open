using Anemoi.Contract.MasterData.Responses;
using Anemoi.MasterData.Application.Abstractions;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Anemoi.MasterData.Infrastructure.Services;

public sealed class DbDiscoveryService : IDbDiscoveryService
{
    public async Task<DbSchemaResponse> GetSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var tablesQuery = @"
            SELECT TABLE_NAME as TableName, TABLE_SCHEMA as SchemaName 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME NOT LIKE '__EFMigrationsHistory'";

        var tables = (await connection.QueryAsync<(string TableName, string SchemaName)>(tablesQuery)).ToList();
        var response = new DbSchemaResponse { Tables = new List<TableSchema>() };

        foreach (var table in tables)
        {
            var columnsQuery = @"
                SELECT COLUMN_NAME as ColumnName, DATA_TYPE as DataType, IS_NULLABLE as IsNullable, CHARACTER_MAXIMUM_LENGTH as MaxLength
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = @TableName AND TABLE_SCHEMA = @SchemaName";

            var columns = await connection.QueryAsync<ColumnSchema>(columnsQuery, new { table.TableName, table.SchemaName });

            response.Tables.Add(new TableSchema
            {
                TableName = table.TableName,
                Columns = columns.ToList()
            });
        }

        return response;
    }
}
