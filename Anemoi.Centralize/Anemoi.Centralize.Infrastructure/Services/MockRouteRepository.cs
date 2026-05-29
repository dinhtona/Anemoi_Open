#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.Centralize.Application.Abstractions;
using Anemoi.Centralize.Application.Configurations;

namespace Anemoi.Centralize.Infrastructure.Services;

public sealed class MockRouteRepository : IMockRouteRepository
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public MockRouteRepository(DevEnvironmentsSetting settings)
    {
        var dirPath = "/app/env_data";
        if (!Directory.Exists(dirPath))
        {
            dirPath = Path.Combine(Directory.GetCurrentDirectory(), settings.LocalEnvDir, "env_data");
        }

        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        _filePath = Path.Combine(dirPath, "config.json");
    }

    private async Task<List<MockRouteDto>> ReadAllFromFileAsync()
    {
        if (!File.Exists(_filePath))
        {
            return new List<MockRouteDto>();
        }

        try
        {
            var json = await File.ReadAllTextAsync(_filePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<MockRouteDto>();
            }
            return JsonSerializer.Deserialize<List<MockRouteDto>>(json) ?? new List<MockRouteDto>();
        }
        catch
        {
            return new List<MockRouteDto>();
        }
    }

    private async Task WriteAllToFileAsync(List<MockRouteDto> routes)
    {
        var json = JsonSerializer.Serialize(routes, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_filePath, json);
    }

    public async Task<List<MockRouteDto>> GetAllAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            return await ReadAllFromFileAsync();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<MockRouteDto?> GetByIdAsync(Guid id)
    {
        var routes = await GetAllAsync();
        return routes.FirstOrDefault(r => r.Id == id);
    }

    public async Task<MockRouteDto?> GetMatchingRouteAsync(string path, string method)
    {
        var routes = await GetAllAsync();

        var cleanPath = "/" + (path ?? "").Trim('/');
        var cleanMethod = (method ?? "").Trim().ToUpperInvariant();

        return routes.FirstOrDefault(r =>
            r.IsActive &&
            r.Method.Equals(cleanMethod, StringComparison.OrdinalIgnoreCase) &&
            ("/" + r.Path.Trim('/')).Equals(cleanPath, StringComparison.OrdinalIgnoreCase)
        );
    }

    public async Task AddAsync(MockRouteDto route)
    {
        await _semaphore.WaitAsync();
        try
        {
            var routes = await ReadAllFromFileAsync();
            if (routes.Any(r => r.Id == route.Id))
            {
                throw new InvalidOperationException("Route already exists.");
            }
            routes.Add(route);
            await WriteAllToFileAsync(routes);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task UpdateAsync(MockRouteDto route)
    {
        await _semaphore.WaitAsync();
        try
        {
            var routes = await ReadAllFromFileAsync();
            var index = routes.FindIndex(r => r.Id == route.Id);
            if (index == -1)
            {
                throw new KeyNotFoundException("Route not found.");
            }
            routes[index] = route;
            await WriteAllToFileAsync(routes);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        await _semaphore.WaitAsync();
        try
        {
            var routes = await ReadAllFromFileAsync();
            var removed = routes.RemoveAll(r => r.Id == id);
            if (removed > 0)
            {
                await WriteAllToFileAsync(routes);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
