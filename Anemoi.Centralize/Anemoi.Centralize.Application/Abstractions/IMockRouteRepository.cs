#nullable enable
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.Centralize.Domain.ModelIds;

namespace Anemoi.Centralize.Application.Abstractions;

public sealed class MockRouteIdJsonConverter : JsonConverter<MockRouteId>
{
    public override MockRouteId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetGuid());

    public override void Write(Utf8JsonWriter writer, MockRouteId value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}

public sealed class MockRouteIdNewtonsoftJsonConverter : Newtonsoft.Json.JsonConverter<MockRouteId>
{
    public override MockRouteId ReadJson(
        Newtonsoft.Json.JsonReader reader,
        Type objectType,
        MockRouteId? existingValue,
        bool hasExistingValue,
        Newtonsoft.Json.JsonSerializer serializer) =>
        new(Guid.Parse((string)reader.Value!));

    public override void WriteJson(
        Newtonsoft.Json.JsonWriter writer,
        MockRouteId? value,
        Newtonsoft.Json.JsonSerializer serializer) =>
        writer.WriteValue(value?.Value);
}

public class MockRouteDto
{
    [JsonConverter(typeof(MockRouteIdJsonConverter))]
    [Newtonsoft.Json.JsonConverter(typeof(MockRouteIdNewtonsoftJsonConverter))]
    public MockRouteId Id { get; set; } = new(IdGenerator.NextGuid());
    public string Method { get; set; } = "GET";
    public string Path { get; set; } = "/";
    public int StatusCode { get; set; } = 200;
    public string ContentType { get; set; } = "application/json";
    public string ResponseBody { get; set; } = "{}";
    public string Description { get; set; } = "";
    public bool IsActive { get; set; } = true;
}

public interface IMockRouteRepository
{
    Task<List<MockRouteDto>> GetAllAsync();
    Task<MockRouteDto?> GetByIdAsync(MockRouteId id);
    Task<MockRouteDto?> GetMatchingRouteAsync(string path, string method);
    Task AddAsync(MockRouteDto route);
    Task UpdateAsync(MockRouteDto route);
    Task DeleteAsync(MockRouteId id);
}
