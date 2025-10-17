using System.Collections.Generic;

namespace DeskSharper.Models;

/// <summary>
/// Result of a cleaning operation
/// </summary>
public class CleanResult
{
    /// <summary>
    /// Successfully moved/deleted items
    /// </summary>
    public List<string> MovedItems { get; set; } = new();

    /// <summary>
    /// Items that failed to move/delete
    /// </summary>
    public List<string> ErrorItems { get; set; } = new();

    /// <summary>
    /// Total items processed
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public int TotalItems => MovedItems.Count + ErrorItems.Count;

    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success => ErrorItems.Count == 0;
}
