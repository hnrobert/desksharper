using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace DeskSharper.Services;

/// <summary>
/// Cross-platform notification service
/// </summary>
public class NotificationService(ILogger<NotificationService> logger)
{
    /// <summary>
    /// Show a notification
    /// </summary>
    public async Task ShowNotificationAsync(string title, string message)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                await ShowWindowsNotificationAsync(title, message);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                await ShowMacNotificationAsync(title, message);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                await ShowLinuxNotificationAsync(title, message);
            }

            logger.LogInformation("Notification shown: {Title}", title);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error showing notification");
        }
    }

    private async Task ShowWindowsNotificationAsync(string title, string message)
    {
        // Use PowerShell to show Windows toast notification
        var escapedTitle = title.Replace("'", "''");
        var escapedMessage = message.Replace("'", "''");

        var script = $@"
            [Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime] | Out-Null
            [Windows.UI.Notifications.ToastNotification, Windows.UI.Notifications, ContentType = WindowsRuntime] | Out-Null
            [Windows.Data.Xml.Dom.XmlDocument, Windows.Data.Xml.Dom.XmlDocument, ContentType = WindowsRuntime] | Out-Null

            $template = @""
            <toast>
                <visual>
                    <binding template='ToastGeneric'>
                        <text>{escapedTitle}</text>
                        <text>{escapedMessage}</text>
                    </binding>
                </visual>
            </toast>
            ""@

            $xml = New-Object Windows.Data.Xml.Dom.XmlDocument
            $xml.LoadXml($template)
            $toast = New-Object Windows.UI.Notifications.ToastNotification $xml
            [Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier('DeskSharper').Show($toast)
        ";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        await process.WaitForExitAsync();
    }

    private async Task ShowMacNotificationAsync(string title, string message)
    {
        var escapedTitle = EscapeShellArgument(title);
        var escapedMessage = EscapeShellArgument(message);

        var script = $"display notification \"{escapedMessage}\" with title \"{escapedTitle}\"";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e '{script}'",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        await process.WaitForExitAsync();
    }

    private async Task ShowLinuxNotificationAsync(string title, string message)
    {
        var escapedTitle = EscapeShellArgument(title);
        var escapedMessage = EscapeShellArgument(message);

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "notify-send",
                Arguments = $"\"{escapedTitle}\" \"{escapedMessage}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        await process.WaitForExitAsync();
    }

    private string EscapeShellArgument(string argument)
    {
        return argument.Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("'", "\\'");
    }
}
