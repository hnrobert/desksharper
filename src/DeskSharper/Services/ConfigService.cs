using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using DeskSharper.Models;
using Microsoft.Extensions.Logging;

namespace DeskSharper.Services;

/// <summary>
/// Service for managing cleaning configurations
/// </summary>
public class ConfigService
{
    private readonly ILogger<ConfigService> _logger;
    private readonly string _configDirectory;
    private readonly string _configFilePath;

    public ConfigService(ILogger<ConfigService> logger)
    {
        _logger = logger;

        // Use platform-appropriate config directory
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _configDirectory = Path.Combine(appDataPath, "DeskSharper");
        _configFilePath = Path.Combine(_configDirectory, "configs.json");

        EnsureConfigDirectoryExists();
    }

    private void EnsureConfigDirectoryExists()
    {
        if (!Directory.Exists(_configDirectory))
        {
            Directory.CreateDirectory(_configDirectory);
            _logger.LogInformation("Created config directory: {Directory}", _configDirectory);
        }
    }

    /// <summary>
    /// Load all configurations
    /// </summary>
    public async Task<List<CleanConfig>> LoadConfigsAsync()
    {
        try
        {
            if (!File.Exists(_configFilePath))
            {
                _logger.LogInformation("No config file found, returning empty list");
                return new List<CleanConfig>();
            }

            var json = await File.ReadAllTextAsync(_configFilePath);
            var configs = JsonSerializer.Deserialize(json, ConfigJsonContext.Default.ListCleanConfig) ??
                          new List<CleanConfig>();

            _logger.LogInformation("Loaded {Count} configurations", configs.Count);
            return configs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading configurations");
            return new List<CleanConfig>();
        }
    }

    /// <summary>
    /// Save all configurations
    /// </summary>
    public async Task SaveConfigsAsync(List<CleanConfig> configs)
    {
        try
        {
            var json = JsonSerializer.Serialize(configs, ConfigJsonContext.Default.ListCleanConfig);
            await File.WriteAllTextAsync(_configFilePath, json);

            _logger.LogInformation("Saved {Count} configurations", configs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving configurations");
            throw;
        }
    }

    /// <summary>
    /// Get a specific configuration by name
    /// </summary>
    public async Task<CleanConfig?> GetConfigAsync(string name)
    {
        var configs = await LoadConfigsAsync();
        return configs.FirstOrDefault(c => c.Name == name);
    }

    /// <summary>
    /// Save or update a configuration
    /// </summary>
    public async Task SaveConfigAsync(CleanConfig config)
    {
        var configs = await LoadConfigsAsync();
        var existingIndex = configs.FindIndex(c => c.Name == config.Name);

        if (existingIndex >= 0)
        {
            configs[existingIndex] = config;
            _logger.LogInformation("Updated configuration: {Name}", config.Name);
        }
        else
        {
            configs.Add(config);
            _logger.LogInformation("Added new configuration: {Name}", config.Name);
        }

        await SaveConfigsAsync(configs);
    }

    /// <summary>
    /// Delete a configuration
    /// </summary>
    public async Task DeleteConfigAsync(string name)
    {
        var configs = await LoadConfigsAsync();
        var removed = configs.RemoveAll(c => c.Name == name);

        if (removed > 0)
        {
            await SaveConfigsAsync(configs);
            _logger.LogInformation("Deleted configuration: {Name}", name);
        }
    }

    /// <summary>
    /// Get configurations marked for auto-run
    /// </summary>
    public async Task<List<CleanConfig>> GetAutoRunConfigsAsync()
    {
        var configs = await LoadConfigsAsync();
        return configs.Where(c => c.AutoRun).ToList();
    }

    /// <summary>
    /// Get the config directory path
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public string GetConfigDirectory() => _configDirectory;
}
