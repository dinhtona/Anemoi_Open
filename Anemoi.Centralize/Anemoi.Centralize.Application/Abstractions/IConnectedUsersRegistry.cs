using System.Collections.Generic;

namespace Anemoi.Centralize.Application.Abstractions;

public interface IConnectedUsersRegistry
{
    void AddUser(string userId);
    void RemoveUser(string userId);
    IReadOnlyCollection<string> GetActiveUserIds();
}
