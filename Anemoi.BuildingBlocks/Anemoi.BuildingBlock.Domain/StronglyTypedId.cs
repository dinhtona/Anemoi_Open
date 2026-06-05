using System.ComponentModel;
using System.Text.Json.Serialization;
using Anemoi.BuildingBlock.Domain.Abstractions;
using Anemoi.BuildingBlock.Domain.StronglyTypedHelper;

namespace Anemoi.BuildingBlock.Domain;

[TypeConverter(typeof(StronglyTypedIdConverter))]
[JsonConverter(typeof(StronglyTypedIdJsonConverterFactory))]
public record StronglyTypedId<TValue>(TValue Value) : IAggregateRoot where TValue : notnull
{
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString();
}