namespace Anemoi.BuildingBlock.Domain;

public sealed class DomainException(string message) : Exception(message);
