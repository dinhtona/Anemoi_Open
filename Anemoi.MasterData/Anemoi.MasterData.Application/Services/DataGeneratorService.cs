using Anemoi.Contract.MasterData.Responses;
using Anemoi.MasterData.Application.Abstractions;
using Anemoi.MasterData.Application.Models;
using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.MasterData.Application.Services;

public sealed class DataGeneratorService : IDataGeneratorService
{
    private readonly Faker _faker = new();

    public Task<List<Dictionary<string, object>>> GenerateDataAsync(
        TableSchema tableSchema,
        TableConfig tableConfig,
        List<RelationshipConfig> activeRelationships,
        Dictionary<string, List<Dictionary<string, object>>> seedingContext,
        CancellationToken cancellationToken = default)
    {
        var result = new List<Dictionary<string, object>>();

        // Find the "primary" relationship (for Parent-Child distribution)
        // In this simple version, we assume a table has at most one parent relationship influencing its generation count
        var relationship = activeRelationships.FirstOrDefault(r => r.ChildTable == tableConfig.TableName);

        if (relationship != null && seedingContext.TryGetValue(relationship.ParentTable, out var parentRows))
        {
            List<Dictionary<string, object>> firstParentChildren = null;

            for (var pIdx = 0; pIdx < parentRows.Count; pIdx++)
            {
                var parentRow = parentRows[pIdx];

                // Strategy: COPY_FROM_FIRST_PARENT
                if (pIdx > 0 && relationship.ChildDataStrategy == "COPY_FROM_FIRST_PARENT" && firstParentChildren != null)
                {
                    foreach (var baseChild in firstParentChildren)
                    {
                        var clonedChild = new Dictionary<string, object>(baseChild);
                        // Update Join Keys to point to the current parent
                        foreach (var key in relationship.JoinKeys)
                        {
                            if (parentRow.TryGetValue(key.ParentColumn, out var parentVal))
                                clonedChild[key.ChildColumn] = parentVal;
                        }
                        result.Add(clonedChild);
                    }
                    continue;
                }

                // Generate new rows for this parent
                var currentParentChildren = new List<Dictionary<string, object>>();
                for (var i = 0; i < tableConfig.RowCount; i++)
                {
                    var row = GenerateRow(tableSchema, tableConfig.ColumnRules);
                    
                    // Set Join Keys
                    foreach (var key in relationship.JoinKeys)
                    {
                        if (parentRow.TryGetValue(key.ParentColumn, out var parentVal))
                            row[key.ChildColumn] = parentVal;
                    }
                    
                    currentParentChildren.Add(row);
                    result.Add(row);
                }

                if (pIdx == 0) firstParentChildren = currentParentChildren;
            }
        }
        else
        {
            // Standard generation (no parent context or parent not found)
            for (var i = 0; i < tableConfig.RowCount; i++)
            {
                result.Add(GenerateRow(tableSchema, tableConfig.ColumnRules));
            }
        }

        return Task.FromResult(result);
    }

    private Dictionary<string, object> GenerateRow(TableSchema tableSchema, List<ColumnRule> rules)
    {
        var row = new Dictionary<string, object>();
        
        // First pass: Generate values for columns without custom rules or with STATIC rules
        foreach (var column in tableSchema.Columns)
        {
            var rule = rules.FirstOrDefault(r => r.ColumnName == column.ColumnName);
            if (rule != null && rule.RuleType == "STATIC")
            {
                row[column.ColumnName] = rule.Parameters.FirstOrDefault() ?? "";
            }
            else if (rule == null || rule.RuleType != "SUM") // Sum needs other columns to be ready
            {
                row[column.ColumnName] = GenerateBogusValue(column, rule);
            }
        }

        // Second pass: Calculate SUM rules
        foreach (var rule in rules.Where(r => r.RuleType == "SUM"))
        {
            decimal sum = 0;
            foreach (var param in rule.Parameters)
            {
                if (row.TryGetValue(param, out var val) && decimal.TryParse(val?.ToString(), out var numericVal))
                {
                    sum += numericVal;
                }
            }
            row[rule.ColumnName] = sum;
        }

        return row;
    }

    private object GenerateBogusValue(ColumnSchema column, ColumnRule rule)
    {
        if (rule?.RuleType == "CUSTOM")
        {
            // Placeholder for custom scripts (e.g. JS or C# scripts)
            return _faker.Lorem.Word();
        }

        var dataType = column.DataType.ToLower();

        if (dataType.Contains("int")) return _faker.Random.Int(1, 10000);
        if (dataType.Contains("decimal") || dataType.Contains("numeric") || dataType.Contains("money")) return _faker.Finance.Amount();
        if (dataType.Contains("bit") || dataType.Contains("bool")) return _faker.Random.Bool();
        if (dataType.Contains("datetime") || dataType.Contains("date")) return _faker.Date.Past();
        if (dataType.Contains("guid") || dataType.Contains("uniqueidentifier")) return Guid.NewGuid();
        
        if (column.ColumnName.ToLower().Contains("email")) return _faker.Internet.Email();
        if (column.ColumnName.ToLower().Contains("name")) return _faker.Name.FullName();
        if (column.ColumnName.ToLower().Contains("phone")) return _faker.Phone.PhoneNumber();
        
        return _faker.Lorem.Word();
    }
}
