namespace Anemoi.Hr.Application.Responses;

public sealed record FileResponse(byte[] FileBytes, string ContentType, string FileName);
