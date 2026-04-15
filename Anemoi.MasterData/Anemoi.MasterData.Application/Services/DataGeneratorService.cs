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
                    var row = GenerateRow(tableSchema, tableConfig.ColumnRules, i);
                    
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
                result.Add(GenerateRow(tableSchema, tableConfig.ColumnRules, i));
            }
        }

        return Task.FromResult(result);
    }

    private Dictionary<string, object> GenerateRow(TableSchema tableSchema, List<ColumnRule> rules, int rowIndex)
    {
        var row = new Dictionary<string, object>();
        
        // First pass: Generate values for columns without custom rules or with STATIC rules
        foreach (var column in tableSchema.Columns)
        {
            if (column.IsIdentity)
            {
                Console.WriteLine($"[DataGenerator] Skipping Identity column {column.ColumnName}. Let DB auto-generate.");
                continue;
            }

            var rule = rules.FirstOrDefault(r => r.ColumnName.Equals(column.ColumnName, StringComparison.OrdinalIgnoreCase));
            
            Console.WriteLine($"[DataGenerator] Processing column {column.ColumnName} (IsIdentity: {column.IsIdentity}, Type: {column.DataType}) with Rule: {rule?.RuleType ?? "None"}");
            
            if (rule != null && rule.RuleType == "STATIC")
            {
                var staticValue = rule.Parameters.FirstOrDefault();
                row[column.ColumnName] = string.IsNullOrWhiteSpace(staticValue) ? (column.IsNullable ? null : DefaultValue(column)) : staticValue;
            }
            else if (rule != null && rule.RuleType == "SEQUENCE")
            {
                var pattern = rule.Parameters.ElementAtOrDefault(0) ?? "{SEQ}";
                var startValueStr = rule.Parameters.ElementAtOrDefault(1) ?? "1";
                if (long.TryParse(startValueStr, out var startValue))
                {
                    var currentVal = startValue + rowIndex;
                    row[column.ColumnName] = pattern.Replace("{SEQ}", currentVal.ToString($"D{startValueStr.Length}"));
                }
                else
                {
                    row[column.ColumnName] = pattern.Replace("{SEQ}", rowIndex.ToString());
                }
            }
            else if (rule == null || rule.RuleType != "SUM") // Sum needs other columns to be ready
            {
                row[column.ColumnName] = GenerateBogusValue(column, rule);
            }
            
            // Ensure we have a value for NOT NULL columns (skip SUM columns as they are handled in the second pass)
            if (rule?.RuleType != "SUM" && (!row.TryGetValue(column.ColumnName, out var val) || val == null) && !column.IsNullable)
            {
                Console.WriteLine($"[DataGenerator] WARNING: Column {column.ColumnName} is NOT NULL but value is null. Forcing default.");
                row[column.ColumnName] = DefaultValue(column);
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
        var value = GenerateRawBogusValue(column, rule);

        // Enforce MaxLength for string values
        if (value is string s && column.MaxLength > 0)
        {
            return s.Length <= column.MaxLength.Value ? s : s.Substring(0, column.MaxLength.Value);
        }

        return value;
    }

    private object GenerateRawBogusValue(ColumnSchema column, ColumnRule rule)
    {
        if (rule?.RuleType == "CUSTOM")
        {
            var param = rule.Parameters.FirstOrDefault()?.ToLower();
            if (param == "price") return _faker.Finance.Amount(10, 1000);
            if (param == "quantity") return _faker.Random.Int(1, 100);
            if (param == "sku") return _faker.Commerce.Ean13();
            return _faker.Lorem.Word();
        }

        var dataType = column.DataType.ToLower();
        var columnName = column.ColumnName.ToLower();

        // 1. Check by Data Type (Numeric, Date, Bit, Binary, Guid, XML)
        // These are prioritized to avoid type conversion errors if a column name matches a pattern (e.g., an 'int' column named 'status_name')
        if (dataType.Contains("tinyint")) return (byte)_faker.Random.Int(0, 255);
        if (dataType.Contains("smallint")) return (short)_faker.Random.Int(0, 32767);
        if (dataType.Contains("int")) return _faker.Random.Int(1, 10000);
        if (dataType.Contains("decimal") || dataType.Contains("numeric") || dataType.Contains("money")) return _faker.Finance.Amount();
        if (dataType.Contains("bit") || dataType.Contains("bool")) return _faker.Random.Bool();
        
        if (dataType.Contains("datetimeoffset")) return _faker.Date.RecentOffset(30);
        if (dataType.Contains("datetime") || dataType.Contains("date")) return _faker.Date.Recent(30);
        
        // Exact match for 'rowversion' or 'timestamp' (which technically is an 8-byte binary)
        if (dataType.Contains("rowversion") || dataType.Contains("timestamp")) return _faker.Random.Bytes(8);

        // Exact match for 'time' to avoid partial matches with 'datetime' (though handled by order, this is safer)
        if (dataType.Contains("time")) return TimeSpan.FromTicks(_faker.Random.Long(0, TimeSpan.FromDays(1).Ticks - 1));
        
        if (dataType.Contains("binary") || dataType.Contains("image") || dataType.Contains("varbinary"))
        {
            var length = column.MaxLength is > 0 and <= 8000 ? column.MaxLength.Value : 16;
            return _faker.Random.Bytes(length);
        }

        if (dataType.Contains("xml")) return $"<root><id>{_faker.Random.Int()}</id><data>{_faker.Lorem.Word()}</data></root>";
        
        if (dataType.Contains("guid") || dataType.Contains("uniqueidentifier")) return Guid.NewGuid();
        
        if (dataType.Contains("float") || dataType.Contains("real")) return _faker.Random.Double();

        // 2. Check by Column Name Patterns (Best effort for string-like columns)
        if (columnName.Contains("email")) return _faker.Internet.Email();
        if (columnName.Contains("phone")) return _faker.Phone.PhoneNumber();
        if (columnName.Contains("address")) return _faker.Address.FullAddress();
        if (columnName.Contains("city")) return _faker.Address.City();
        if (columnName.Contains("country")) return _faker.Address.Country();
        if (columnName.Contains("zip") || columnName.Contains("postal")) return _faker.Address.ZipCode();
        if (columnName.Contains("company")) return _faker.Company.CompanyName();
        
        if (columnName.Contains("price") || columnName.Contains("amount") || columnName.Contains("cost") || columnName.Contains("total")) 
            return _faker.Finance.Amount(5, 5000);
            
        if (columnName.Contains("quantity") || columnName.Contains("qty") || columnName.Contains("count") || columnName.Contains("stock"))
            return _faker.Random.Int(0, 1000);

        if (columnName.Contains("name")) 
        {
            if (columnName.Contains("first")) return _faker.Name.FirstName();
            if (columnName.Contains("last")) return _faker.Name.LastName();
            if (columnName.Contains("product")) return _faker.Commerce.ProductName();
            return _faker.Name.FullName();
        }

        if (columnName.Contains("description") || columnName.Contains("comment") || columnName.Contains("note"))
            return _faker.Lorem.Paragraph();

        // 3. Fallback for other string types
        if (dataType.Contains("text") || dataType.Contains("ntext")) return _faker.Lorem.Paragraphs(2);
        
        return _faker.Lorem.Word();
    }

    private object DefaultValue(ColumnSchema column)
    {
        return GenerateBogusValue(column, null);
    }
}
