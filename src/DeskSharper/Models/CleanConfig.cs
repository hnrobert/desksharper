using System.Collections.Generic;

namespace DeskSharper.Models;

/// <summary>
/// Represents a desktop cleaning configuration
/// </summary>
public class CleanConfig
{
    /// <summary>
    /// Configuration name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Source path to clean (e.g., Desktop)
    /// </summary>
    public string SourcePath { get; set; } = string.Empty;

    /// <summary>
    /// Destination path to move files to (empty means delete)
    /// </summary>
    public string DestinationPath { get; set; } = string.Empty;

    /// <summary>
    /// Files to keep (whitelist by name)
    /// </summary>
    public List<string> KeepFiles { get; set; } = new();

    /// <summary>
    /// File extensions to keep (e.g., .txt, .pdf)
    /// </summary>
    public List<string> KeepExtensions { get; set; } = new();

    /// <summary>
    /// Time threshold in hours (files older than this will be processed)
    /// </summary>
    public int RetentionHours { get; set; }

    /// <summary>
    /// Check mode: 1 = Last Modified, 2 = Last Accessed
    /// </summary>
    public TimeCheckMode CheckMode { get; set; } = TimeCheckMode.LastModified;

    /// <summary>
    /// Whether to move folders
    /// </summary>
    public bool MoveFolders { get; set; }

    /// <summary>
    /// Whether to move shortcuts (.lnk on Windows, symlinks on Unix)
    /// </summary>
    public bool MoveShortcuts { get; set; } = true;

    /// <summary>
    /// Whether to run automatically on startup
    /// </summary>
    public bool AutoRun { get; set; }

    /// <summary>
    /// Last edit flag
    /// </summary>
    public bool LastEdit { get; set; } = false;
}

public enum TimeCheckMode
{
    /// <summary>
    /// Check last modification time
    /// </summary>
    LastModified = 1,

    /// <summary>
    /// Check last access time
    /// </summary>
    LastAccessed = 2
}
