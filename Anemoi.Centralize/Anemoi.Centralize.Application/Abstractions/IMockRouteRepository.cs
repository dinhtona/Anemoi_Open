#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Anemoi.Centralize.Application.Abstractions;

public class MockRouteDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Method { get; set; } = "GET";
    public string Path { get; set; } = "/";
    public int StatusCode { get; set; } = 200;
    public string ContentType { get; set; } = "application/json";
    public string ResponseBody { get; set; } = "{}";
    public string Description { get; set; } = "";
    public bool IsActive { get; set; } = true;
}

public interface IMockRouteRepository
{
    Task<List<MockRouteDto>> GetAllAsync();
    Task<MockRouteDto?> GetByIdAsync(Guid id);
    Task<MockRouteDto?> GetMatchingRouteAsync(string path, string method);
    Task AddAsync(MockRouteDto route);
    Task UpdateAsync(MockRouteDto route);
    Task DeleteAsync(Guid id);
}
