using System.Collections.Generic;
using System.Text.Json.Serialization;
using DeskSharper.Models;

namespace DeskSharper.Services;

/// <summary>
/// JSON serialization context for AOT-compatible serialization
/// </summary>
[JsonSerializable(typeof(List<CleanConfig>))]
[JsonSerializable(typeof(CleanConfig))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public partial class ConfigJsonContext : JsonSerializerContext;
