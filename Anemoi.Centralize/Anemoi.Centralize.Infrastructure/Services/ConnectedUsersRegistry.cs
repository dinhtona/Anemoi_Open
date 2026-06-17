using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Anemoi.Centralize.Application.Abstractions;

namespace Anemoi.Centralize.Infrastructure.Services;

public sealed class ConnectedUsersRegistry : IConnectedUsersRegistry
{
    private readonly ConcurrentDictionary<string, int> _connectedUsers = new();

    public void AddUser(string userId)
    {
        if (string.IsNullOrEmpty(userId)) return;
        _connectedUsers.AddOrUpdate(userId, 1, (_, count) => count + 1);
    }

    public void RemoveUser(string userId)
    {
        if (string.IsNullOrEmpty(userId)) return;
        _connectedUsers.AddOrUpdate(userId, 0, (_, count) => Math.Max(0, count - 1));
    }

    public IReadOnlyCollection<string> GetActiveUserIds()
    {
        var entries = _connectedUsers.ToArray();
        foreach (var kvp in entries)
        {
            if (kvp.Value <= 0)
                _connectedUsers.TryRemove(kvp.Key, out _);
        }
        return entries.Where(kvp => kvp.Value > 0).Select(kvp => kvp.Key).ToList();
    }
}
