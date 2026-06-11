#nullable enable

namespace Anemoi.Hr.Application.Responses;

public sealed record ExportResult(byte[] Content, string ContentType, string FileName);
