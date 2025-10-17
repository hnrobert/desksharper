using System;

namespace DeskSharper.Models;

/// <summary>
/// Represents a single cleaning task (file/folder to process)
/// </summary>
public class CleanTask
{
    /// <summary>
    /// Item name (file or folder name)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Full source path
    /// </summary>
    public string SourcePath { get; set; } = string.Empty;

    /// <summary>
    /// Full destination path
    /// </summary>
    public string DestinationPath { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a directory
    /// </summary>
    public bool IsDirectory { get; set; }

    /// <summary>
    /// File size in bytes (0 for directories)
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Last modified time
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// Last accessed time
    /// </summary>
    public DateTime LastAccessed { get; set; }
}
