using Anemoi.Contract.MasterData.Responses;
using Anemoi.MasterData.Application.Abstractions;
using Bogus;
using Newtonsoft.Json;

namespace Anemoi.MasterData.Application.Services;

public sealed class DataGeneratorService : IDataGeneratorService
{
    private readonly Faker _faker = new();

    public Task<List<Dictionary<string, object>>> GenerateDataAsync(TableSchema tableSchema, int count, string configJson, CancellationToken cancellationToken = default)
    {
        var result = new List<Dictionary<string, object>>();
        var configs = string.IsNullOrWhiteSpace(configJson) 
            ? new Dictionary<string, string>() 
            : JsonConvert.DeserializeObject<Dictionary<string, string>>(configJson) ?? new();

        for (var i = 0; i < count; i++)
        {
            var row = new Dictionary<string, object>();
            foreach (var column in tableSchema.Columns)
            {
                row[column.ColumnName] = GenerateValue(column, configs);
            }
            result.Add(row);
        }

        return Task.FromResult(result);
    }

    private object GenerateValue(ColumnSchema column, Dictionary<string, string> configs)
    {
        // Check for template override
        if (configs.TryGetValue(column.ColumnName, out var typeOverride))
        {
            return typeOverride.ToLower() switch
            {
                "email" => _faker.Internet.Email(),
                "name" => _faker.Name.FullName(),
                "phone" => _faker.Phone.PhoneNumber(),
                "address" => _faker.Address.FullAddress(),
                "company" => _faker.Company.CompanyName(),
                _ => GenerateDefaultValue(column)
            };
        }

        return GenerateDefaultValue(column);
    }

    private object GenerateDefaultValue(ColumnSchema column)
    {
        var dataType = column.DataType.ToLower();

        if (dataType.Contains("int")) return _faker.Random.Int(1, 1000);
        if (dataType.Contains("decimal") || dataType.Contains("numeric") || dataType.Contains("money")) return _faker.Finance.Amount();
        if (dataType.Contains("bit") || dataType.Contains("bool")) return _faker.Random.Bool();
        if (dataType.Contains("datetime") || dataType.Contains("date")) return _faker.Date.Past();
        if (dataType.Contains("guid") || dataType.Contains("uniqueidentifier")) return Guid.NewGuid();
        
        return _faker.Lorem.Word();
    }
}
