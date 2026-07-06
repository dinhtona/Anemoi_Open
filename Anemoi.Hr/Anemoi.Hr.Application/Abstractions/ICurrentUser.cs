#nullable enable

namespace Anemoi.Hr.Application.Abstractions;

public interface ICurrentUser
{
    string UserId { get; }
}
