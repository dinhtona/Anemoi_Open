using System;
using System.Text.RegularExpressions;

namespace Anemoi.BuildingBlock.Application.Helpers;

/// <summary>
/// Resolves <c>${ENV_VAR}</c> placeholders in configuration values from environment variables.
/// SEC-006: secrets must never be hard-coded in appsettings files — config files carry the
/// placeholder and the real value comes from the environment (or <c>.env</c> via docker compose).
/// </summary>
public static class ConfigurationPlaceholders
{
    /// <summary>
    /// Stable error code surfaced when a required configuration value (or its environment
    /// variable) is missing. Also registered in <c>SharedResource.*.resx</c> for localization.
    /// </summary>
    public const string MissingConfigurationErrorCode = "MissingRequiredConfiguration";

    private static readonly Regex PlaceholderPattern =
        new(@"\$\{([A-Za-z_][A-Za-z0-9_]*)\}", RegexOptions.Compiled);

    /// <summary>
    /// Expands every <c>${ENV_VAR}</c> in <paramref name="configuredValue"/> from the environment.
    /// Throws <see cref="InvalidOperationException"/> carrying the stable error code when the
    /// value itself or any referenced environment variable is missing/empty, so misconfiguration
    /// fails fast and loudly at startup instead of silently running with a broken credential.
    /// </summary>
    public static string ResolveRequired(string? configuredValue, string configPath)
    {
        if (string.IsNullOrWhiteSpace(configuredValue))
            throw new InvalidOperationException(
                $"[{MissingConfigurationErrorCode}] '{configPath}' is not configured. " +
                "Provide the value or a ${ENV_VAR} placeholder.");

        return PlaceholderPattern.Replace(configuredValue, match =>
        {
            var envName = match.Groups[1].Value;
            var envValue = Environment.GetEnvironmentVariable(envName);
            if (string.IsNullOrEmpty(envValue))
                throw new InvalidOperationException(
                    $"[{MissingConfigurationErrorCode}] '{configPath}' references environment variable " +
                    $"'{envName}' which is not set or empty. Set it before starting the service.");
            return envValue;
        });
    }
}
