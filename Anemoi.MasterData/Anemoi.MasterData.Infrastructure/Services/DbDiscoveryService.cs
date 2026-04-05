using Anemoi.Contract.MasterData.Responses;
using Anemoi.Contract.MasterData.Constants;
using Anemoi.MasterData.Application.Abstractions;
using Dapper;
using Microsoft.Data.SqlClient;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.MasterData.Infrastructure.Services;

public sealed class DbDiscoveryService : IDbDiscoveryService
{
    private IDbConnection CreateConnection(string connectionString, string provider)
    {
        // Default to SqlServer for backward compatibility with existing data
        if (string.IsNullOrWhiteSpace(provider)) provider = SeedProviderType.SqlServer;

        return provider switch
        {
            SeedProviderType.PostgreSQL => new NpgsqlConnection(connectionString),
            SeedProviderType.SqlServer => new SqlConnection(connectionString),
            _ => throw new NotSupportedException($"Provider {provider} not supported.")
        };
    }

    public async Task<DbSchemaResponse> GetSchemaAsync(string connectionString, string provider, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(provider)) provider = SeedProviderType.SqlServer;
        
        using var connection = CreateConnection(connectionString, provider);
        if (connection is SqlConnection sqlConn) await sqlConn.OpenAsync(cancellationToken);
        else if (connection is NpgsqlConnection npgConn) await npgConn.OpenAsync(cancellationToken);

        var tablesQuery = provider == SeedProviderType.PostgreSQL 
            ? "SELECT table_name as TableName, table_schema as SchemaName FROM information_schema.tables WHERE table_type = 'BASE TABLE' AND table_schema NOT IN ('information_schema', 'pg_catalog')"
            : "SELECT TABLE_NAME as TableName, TABLE_SCHEMA as SchemaName FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME NOT LIKE '__EFMigrationsHistory'";

        var tables = (await connection.QueryAsync<(string TableName, string SchemaName)>(tablesQuery)).ToList();
        var response = new DbSchemaResponse { Tables = new List<TableSchema>() };

        foreach (var table in tables)
        {
            var columnsQuery = provider == SeedProviderType.PostgreSQL
                ? "SELECT column_name as ColumnName, data_type as DataType, (is_nullable = 'YES') as IsNullable FROM information_schema.columns WHERE table_name = @TableName AND table_schema = @SchemaName"
                : "SELECT COLUMN_NAME as ColumnName, DATA_TYPE as DataType, CAST(CASE WHEN IS_NULLABLE = 'YES' THEN 1 ELSE 0 END AS BIT) as IsNullable FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName AND TABLE_SCHEMA = @SchemaName";

            var columns = await connection.QueryAsync<ColumnSchema>(columnsQuery, new { table.TableName, table.SchemaName });

            response.Tables.Add(new TableSchema
            {
                TableName = table.TableName,
                Columns = columns.ToList()
            });
        }

        return response;
    }

    public async Task<bool> TestConnectionAsync(string connectionString, string provider, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = CreateConnection(connectionString, provider);
            if (connection is SqlConnection sqlConn) await sqlConn.OpenAsync(cancellationToken);
            else if (connection is NpgsqlConnection npgConn) await npgConn.OpenAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TestConnection Error]: {ex.Message}");
            return false;
        }
    }
}
