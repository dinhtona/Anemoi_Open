using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Anemoi.BuildingBlock.Domain.StronglyTypedHelper;

public sealed class StronglyTypedIdJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        StronglyTypedIdHelper.IsStronglyTypedId(typeToConvert, out _);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        StronglyTypedIdHelper.IsStronglyTypedId(typeToConvert, out var idType);
        var converterType = typeof(StronglyTypedIdJsonConverter<,>).MakeGenericType(typeToConvert, idType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

public sealed class StronglyTypedIdJsonConverter<TId, TValue> : JsonConverter<TId>
    where TId : StronglyTypedId<TValue>
    where TValue : notnull
{
    public override TId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;

        TValue value = default;
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject) break;
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    reader.Read();
                    if (string.Equals(propertyName, "value", StringComparison.OrdinalIgnoreCase))
                    {
                        var valueConverter = (JsonConverter<TValue>)options.GetConverter(typeof(TValue));
                        value = valueConverter.Read(ref reader, typeof(TValue), options);
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
            }
        }
        else
        {
            var valueConverter = (JsonConverter<TValue>)options.GetConverter(typeof(TValue));
            value = valueConverter.Read(ref reader, typeof(TValue), options);
        }

        var factory = StronglyTypedIdHelper.GetFactory<TValue>(typeToConvert);
        return (TId)factory(value);
    }

    public override void Write(Utf8JsonWriter writer, TId value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            var valueConverter = (JsonConverter<TValue>)options.GetConverter(typeof(TValue));
            valueConverter.Write(writer, value.Value, options);
        }
    }
}
