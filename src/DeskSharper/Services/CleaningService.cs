using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using DeskSharper.Models;
using Microsoft.Extensions.Logging;

namespace DeskSharper.Services;

/// <summary>
/// Core service for desktop cleaning operations
/// </summary>
public class CleaningService(ILogger<CleaningService> logger)
{
    /// <summary>
    /// Scan directory and generate cleaning tasks based on configuration
    /// </summary>
    public Task<List<CleanTask>> ScanDirectoryAsync(
        CleanConfig config,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var tasks = new List<CleanTask>();

        if (!Directory.Exists(config.SourcePath))
        {
            logger.LogWarning("Source path does not exist: {Path}", config.SourcePath);
            return Task.FromResult(tasks);
        }

        progress?.Report($"Scanning {config.SourcePath}...");

        try
        {
            var entries = Directory.GetFileSystemEntries(config.SourcePath);
            var now = DateTime.Now;

            foreach (var entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var info = new FileInfo(entry);
                var dirInfo = new DirectoryInfo(entry);
                var isDirectory = dirInfo.Exists;
                var name = Path.GetFileName(entry);

                // Skip system files
                if (name == "desktop.ini" || name == ".DS_Store")
                    continue;

                // Check if it's a shortcut/symlink
                var isShortcut = IsShortcut(entry);
                if (isShortcut && !config.MoveShortcuts)
                    continue;

                // Skip if in whitelist
                if (config.KeepFiles.Contains(name))
                    continue;

                // Check file extension
                if (!isDirectory)
                {
                    var extension = Path.GetExtension(name).ToLowerInvariant();
                    if (config.KeepExtensions.Any(ext =>
                            ext.ToLowerInvariant() == extension))
                        continue;
                }

                // Skip folders if not configured to move them
                if (isDirectory && !config.MoveFolders)
                    continue;

                // Get time information
                DateTime lastModified = isDirectory ? dirInfo.LastWriteTime : info.LastWriteTime;
                DateTime lastAccessed = isDirectory ? dirInfo.LastAccessTime : info.LastAccessTime;

                // Check time threshold
                if (config.RetentionHours > 0)
                {
                    var checkTime = config.CheckMode == TimeCheckMode.LastModified
                        ? lastModified
                        : lastAccessed;

                    var age = (now - checkTime).TotalHours;
                    if (age < config.RetentionHours)
                        continue;
                }

                // Skip if destination path is within the source path or vice versa
                if (!string.IsNullOrEmpty(config.DestinationPath) &&
                    PathsOverlap(entry, config.DestinationPath))
                    continue;

                var task = new CleanTask
                {
                    Name = name,
                    SourcePath = entry,
                    DestinationPath = string.IsNullOrEmpty(config.DestinationPath)
                        ? string.Empty
                        : Path.Combine(config.DestinationPath, name),
                    IsDirectory = isDirectory,
                    Size = isDirectory ? 0 : info.Length,
                    LastModified = lastModified,
                    LastAccessed = lastAccessed
                };

                tasks.Add(task);
                progress?.Report($"Found: {name}");
            }

            logger.LogInformation("Scan completed. Found {Count} items to process", tasks.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error scanning directory: {Path}", config.SourcePath);
            throw;
        }

        return Task.FromResult(tasks);
    }

    /// <summary>
    /// Execute cleaning tasks
    /// </summary>
    public async Task<CleanResult> ExecuteCleaningAsync(
        List<CleanTask> tasks,
        string destinationPath,
        IProgress<(int current, int total, string item)>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var result = new CleanResult();

        // Ensure destination directory exists if specified
        if (!string.IsNullOrEmpty(destinationPath) && !Directory.Exists(destinationPath))
        {
            Directory.CreateDirectory(destinationPath);
            logger.LogInformation("Created destination directory: {Path}", destinationPath);
        }

        for (int i = 0; i < tasks.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var task = tasks[i];
            progress?.Report((i + 1, tasks.Count, task.Name));

            try
            {
                if (string.IsNullOrEmpty(destinationPath))
                {
                    // Delete mode
                    await DeleteItemAsync(task.SourcePath, task.IsDirectory);
                    result.MovedItems.Add(task.Name);
                    logger.LogInformation("Deleted: {Path}", task.SourcePath);
                }
                else
                {
                    // Move mode
                    // Handle existing file/folder at destination
                    if (File.Exists(task.DestinationPath) || Directory.Exists(task.DestinationPath))
                    {
                        await DeleteItemAsync(task.DestinationPath, task.IsDirectory);
                    }

                    if (task.IsDirectory)
                    {
                        Directory.Move(task.SourcePath, task.DestinationPath);
                    }
                    else
                    {
                        File.Move(task.SourcePath, task.DestinationPath);
                    }

                    result.MovedItems.Add(task.Name);
                    logger.LogInformation("Moved: {Source} -> {Dest}", task.SourcePath, task.DestinationPath);
                }
            }
            catch (Exception ex)
            {
                result.ErrorItems.Add(task.Name);
                logger.LogError(ex, "Error processing item: {Path}", task.SourcePath);
            }
        }

        return result;
    }

    /// <summary>
    /// Delete a file or directory recursively
    /// </summary>
    private async Task DeleteItemAsync(string path, bool isDirectory)
    {
        await Task.Run(() =>
        {
            if (isDirectory)
            {
                Directory.Delete(path, recursive: true);
            }
            else
            {
                File.Delete(path);
            }
        });
    }

    /// <summary>
    /// Check if a path is a shortcut or symbolic link
    /// </summary>
    private static bool IsShortcut(string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.GetExtension(path).Equals(".lnk", StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            // On Unix systems, check for symbolic links
            try
            {
                var fileInfo = new FileInfo(path);
                return fileInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Check if two paths overlap (one contains the other)
    /// </summary>
    private bool PathsOverlap(string path1, string path2)
    {
        try
        {
            var fullPath1 = Path.GetFullPath(path1);
            var fullPath2 = Path.GetFullPath(path2);

            return fullPath1.StartsWith(fullPath2, StringComparison.OrdinalIgnoreCase) ||
                   fullPath2.StartsWith(fullPath1, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get the desktop path for current platform
    /// </summary>
    public static string GetDesktopPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    }
}
