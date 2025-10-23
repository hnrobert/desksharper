using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeskSharper.Models;
using DeskSharper.Services;
using DeskSharper.Localization;
using Avalonia.Controls;
using System.Collections.ObjectModel;

namespace DeskSharper.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly ConfigService _configService;
    private readonly CleaningService _cleaningService;
    private readonly NotificationService _notificationService;

    [ObservableProperty] private string _configName = string.Empty;

    [ObservableProperty] private string _sourcePath = CleaningService.GetDesktopPath();

    [ObservableProperty] private string _destinationPath = string.Empty;

    [ObservableProperty] private bool _moveFolders;

    [ObservableProperty] private bool _moveShortcuts = true;

    [ObservableProperty] private int _retentionHours;

    [ObservableProperty] private TimeCheckMode _checkMode = TimeCheckMode.LastModified;

    [ObservableProperty] private int _checkModeIndex;

    [ObservableProperty] private bool _autoRun;

    [ObservableProperty] private string _keepFilesText = string.Empty;

    [ObservableProperty] private string _keepExtensionsText = string.Empty;

    [ObservableProperty] private bool _isProcessing;

    [ObservableProperty] private string _statusMessage = string.Empty;

    public bool HasStatusMessage => !string.IsNullOrEmpty(StatusMessage);

    [ObservableProperty] private ObservableCollection<string> _configNames = new();

    [ObservableProperty] private string? _selectedConfigName;

    // Preview-related properties
    [ObservableProperty] private bool _hasPreviewResults;

    [ObservableProperty] private string _previewScanTime = string.Empty;

    [ObservableProperty] private string _previewItemCount = string.Empty;

    [ObservableProperty] private string _previewDestination = string.Empty;

    [ObservableProperty] private string _previewItemList = string.Empty;

    [ObservableProperty] private int _languageIndex;

    public bool HasPreviewOrStatus => HasPreviewResults || !string.IsNullOrEmpty(StatusMessage) || IsProcessing;

    // Parameterless constructor for AOT/trimming and designer support
    public MainViewModel() : this(null!, null!, null!)
    {
    }

    public MainViewModel(
        ConfigService configService,
        CleaningService cleaningService,
        NotificationService notificationService)
    {
        _configService = configService;
        _cleaningService = cleaningService;
        _notificationService = notificationService;

        // Only initialize if services are provided (not in designer mode)
        _ = LoadConfigNamesAsync();

        // Update HasStatusMessage when StatusMessage changes
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(StatusMessage))
            {
                OnPropertyChanged(nameof(HasStatusMessage));
                OnPropertyChanged(nameof(HasPreviewOrStatus));
            }

            if (e.PropertyName == nameof(HasPreviewResults))
            {
                OnPropertyChanged(nameof(HasPreviewOrStatus));
            }

            if (e.PropertyName == nameof(IsProcessing))
            {
                OnPropertyChanged(nameof(HasPreviewOrStatus));
            }

            if (e.PropertyName == nameof(CheckModeIndex))
            {
                CheckMode = CheckModeIndex == 0 ? TimeCheckMode.LastModified : TimeCheckMode.LastAccessed;
            }
        };
    }

    [RelayCommand]
    private async Task BrowseSourcePathAsync()
    {
        var window = GetMainWindow();
        var folders = await window.StorageProvider.OpenFolderPickerAsync(
            new Avalonia.Platform.Storage.FolderPickerOpenOptions
            {
                Title = Strings.Get("SourcePath"),
                AllowMultiple = false
            });

        if (folders.Count > 0)
        {
            SourcePath = folders[0].Path.LocalPath;
        }
    }

    [RelayCommand]
    private async Task BrowseDestinationPathAsync()
    {
        var window = GetMainWindow();
        var folders = await window.StorageProvider.OpenFolderPickerAsync(
            new Avalonia.Platform.Storage.FolderPickerOpenOptions
            {
                Title = Strings.Get("DestinationPath"),
                AllowMultiple = false
            });

        if (folders.Count > 0)
        {
            DestinationPath = folders[0].Path.LocalPath;
        }
    }

    [RelayCommand]
    private async Task PreviewAsync()
    {
        if (!ValidateConfig())
            return;

        IsProcessing = true;
        StatusMessage = Strings.Get("Scanning");
        HasPreviewResults = false;

        try
        {
            var startTime = DateTime.Now;
            var config = CreateConfigFromCurrentSettings();
            var tasks = await _cleaningService.ScanDirectoryAsync(
                config,
                new Progress<string>(msg => StatusMessage = msg));

            var scanDuration = DateTime.Now - startTime;

            // Update preview properties
            PreviewScanTime = $"{scanDuration.TotalSeconds:F2}s";
            PreviewItemCount = tasks.Count.ToString();
            PreviewDestination = string.IsNullOrWhiteSpace(DestinationPath)
                ? "删除 / Delete"
                : DestinationPath;

            if (tasks.Count == 0)
            {
                PreviewItemList = "无 / None";
            }
            else
            {
                var items = tasks.Select(t => $"• {t.Name}").ToList();
                if (items.Count > 50)
                {
                    var displayItems = items.Take(50).ToList();
                    displayItems.Add($"... 以及其他 {items.Count - 50} 个项目 / and {items.Count - 50} more items");
                    PreviewItemList = string.Join("\n", displayItems);
                }
                else
                {
                    PreviewItemList = string.Join("\n", items);
                }
            }

            HasPreviewResults = true;
            StatusMessage = string.Empty;
        }
        catch (Exception ex)
        {
            await ShowMessageAsync("Error", ex.Message);
            HasPreviewResults = false;
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task RunAsync()
    {
        if (!ValidateConfig())
            return;

        // Confirm if destination is empty (delete mode)
        if (string.IsNullOrWhiteSpace(DestinationPath))
        {
            var confirmed = await ShowConfirmAsync(
                Strings.Get("ConfirmEmptyDestination"));
            if (!confirmed)
                return;
        }

        IsProcessing = true;
        HasPreviewResults = false; // Clear preview when running

        try
        {
            var config = CreateConfigFromCurrentSettings();

            // Scan
            StatusMessage = Strings.Get("Scanning");
            var tasks = await _cleaningService.ScanDirectoryAsync(
                config,
                new Progress<string>(msg => StatusMessage = msg));

            if (tasks.Count == 0)
            {
                await _notificationService.ShowNotificationAsync(
                    Strings.Get("DesktopClean"),
                    Strings.Get("NoItemsToProcess"));
                StatusMessage = Strings.Get("NoItemsToProcess");
                return;
            }

            // Execute
            var progress = new Progress<(int current, int total, string item)>(p =>
            {
                StatusMessage = $"{Strings.Get("Processing", p.current, p.total)} - {p.item}";
            });

            var result = await _cleaningService.ExecuteCleaningAsync(
                tasks,
                DestinationPath,
                progress);

            // Show results
            var notification = result.Success
                ? $"{Strings.Get("CleaningComplete")}\n{Strings.Get("ItemsMoved", result.MovedItems.Count)}"
                : $"{Strings.Get("ItemsMoved", result.MovedItems.Count)}\n{Strings.Get("ErrorsOccurred", result.ErrorItems.Count)}";

            await _notificationService.ShowNotificationAsync(
                Strings.Get("CleaningComplete"),
                notification);

            if (result.ErrorItems.Count > 0)
            {
                var errorList = string.Join("\n", result.ErrorItems.Select(e => $"• {e}"));
                await ShowMessageAsync(
                    Strings.Get("ErrorsTitle"),
                    $"{Strings.Get("ErrorsMessage")}\n\n{errorList}");
            }

            StatusMessage = Strings.Get("Completed");
        }
        catch (Exception ex)
        {
            await ShowMessageAsync("Error", ex.Message);
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task SaveConfigAsync()
    {
        if (string.IsNullOrWhiteSpace(ConfigName))
        {
            await ShowMessageAsync("Error", Strings.Get("ConfigNameRequired"));
            return;
        }

        if (!ValidateConfig())
            return;

        try
        {
            var config = CreateConfigFromCurrentSettings();
            config.Name = ConfigName;

            await _configService.SaveConfigAsync(config);
            await LoadConfigNamesAsync();

            StatusMessage = $"Configuration '{ConfigName}' saved";
        }
        catch (Exception ex)
        {
            await ShowMessageAsync("Error", ex.Message);
        }
    }

    [RelayCommand]
    private async Task LoadConfigAsync()
    {
        if (string.IsNullOrEmpty(SelectedConfigName))
            return;

        try
        {
            var config = await _configService.GetConfigAsync(SelectedConfigName);
            if (config != null)
            {
                LoadConfigToUI(config);
                StatusMessage = $"Configuration '{SelectedConfigName}' loaded";
            }
        }
        catch (Exception ex)
        {
            await ShowMessageAsync("Error", ex.Message);
        }
    }

    [RelayCommand]
    private async Task DeleteConfigAsync()
    {
        if (string.IsNullOrEmpty(SelectedConfigName))
            return;

        var confirmed = await ShowConfirmAsync(
            Strings.Get("ConfirmDelete", SelectedConfigName));

        if (!confirmed)
            return;

        try
        {
            await _configService.DeleteConfigAsync(SelectedConfigName);
            await LoadConfigNamesAsync();
            StatusMessage = $"Configuration '{SelectedConfigName}' deleted";
            SelectedConfigName = null;
        }
        catch (Exception ex)
        {
            await ShowMessageAsync("Error", ex.Message);
        }
    }

    [RelayCommand]
    private async Task NewConfigAsync()
    {
        ConfigName = string.Empty;
        SourcePath = CleaningService.GetDesktopPath();
        DestinationPath = string.Empty;
        MoveFolders = false;
        MoveShortcuts = true;
        RetentionHours = 0;
        CheckMode = TimeCheckMode.LastModified;
        AutoRun = false;
        KeepFilesText = string.Empty;
        KeepExtensionsText = string.Empty;
        StatusMessage = "New configuration";

        await Task.CompletedTask;
    }

    private async Task LoadConfigNamesAsync()
    {
        var configs = await _configService.LoadConfigsAsync();
        ConfigNames.Clear();
        foreach (var config in configs)
        {
            ConfigNames.Add(config.Name);
        }
    }

    private CleanConfig CreateConfigFromCurrentSettings()
    {
        return new CleanConfig
        {
            Name = ConfigName,
            SourcePath = SourcePath,
            DestinationPath = DestinationPath,
            KeepFiles = ParseCommaSeparated(KeepFilesText),
            KeepExtensions = ParseCommaSeparated(KeepExtensionsText),
            RetentionHours = RetentionHours,
            CheckMode = CheckMode,
            MoveFolders = MoveFolders,
            MoveShortcuts = MoveShortcuts,
            AutoRun = AutoRun
        };
    }

    // ReSharper disable once InconsistentNaming
    private void LoadConfigToUI(CleanConfig config)
    {
        ConfigName = config.Name;
        SourcePath = config.SourcePath;
        DestinationPath = config.DestinationPath;
        KeepFilesText = string.Join(", ", config.KeepFiles);
        KeepExtensionsText = string.Join(", ", config.KeepExtensions);
        RetentionHours = config.RetentionHours;
        CheckMode = config.CheckMode;
        CheckModeIndex = config.CheckMode == TimeCheckMode.LastModified ? 0 : 1;
        MoveFolders = config.MoveFolders;
        MoveShortcuts = config.MoveShortcuts;
        AutoRun = config.AutoRun;
    }

    private List<string> ParseCommaSeparated(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<string>();

        return text.Split(',')
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();
    }

    private bool ValidateConfig()
    {
        if (string.IsNullOrWhiteSpace(SourcePath))
        {
            _ = ShowMessageAsync("Error", Strings.Get("SourcePathRequired"));
            return false;
        }

        if (!Directory.Exists(SourcePath))
        {
            _ = ShowMessageAsync("Error", Strings.Get("SourcePathNotExist"));
            return false;
        }

        return true;
    }

    private Window GetMainWindow()
    {
        return App.MainWindow ?? throw new InvalidOperationException("Main window not found");
    }

    private async Task ShowMessageAsync(string title, string message)
    {
        var window = GetMainWindow();
        var messageBox = new Window
        {
            Title = title,
            Width = 400,
            Height = 200,
            Content = new TextBlock
            {
                Text = message,
                Margin = new Avalonia.Thickness(20),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            }
        };

        await messageBox.ShowDialog(window);
    }

    private async Task<bool> ShowConfirmAsync(string message)
    {
        // Simple implementation - in a real app, use a proper dialog
        var window = GetMainWindow();
        var result = false;

        var dialog = new Window
        {
            Title = "Confirm",
            Width = 400,
            Height = 150
        };

        dialog.Content = new StackPanel
        {
            Children =
            {
                new TextBlock
                {
                    Text = message,
                    Margin = new Avalonia.Thickness(20),
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                },
                new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Children =
                    {
                        new Button
                        {
                            Content = Strings.Get("Cancel"),
                            Margin = new Avalonia.Thickness(5),
                            Command = new RelayCommand(() =>
                            {
                                result = false;
                                dialog.Close();
                            })
                        },
                        new Button
                        {
                            Content = "OK",
                            Margin = new Avalonia.Thickness(5),
                            Command = new RelayCommand(() =>
                            {
                                result = true;
                                dialog.Close();
                            })
                        }
                    }
                }
            }
        };

        await dialog.ShowDialog(window);
        return result;
    }
}