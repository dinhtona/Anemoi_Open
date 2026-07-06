using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Configurations;

public sealed class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    private const string Format = "HH:mm";

    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
            return default;
        return TimeOnly.Parse(value, CultureInfo.InvariantCulture);
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
}
