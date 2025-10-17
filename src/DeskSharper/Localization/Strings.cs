using System.Collections.Generic;
using System.Globalization;

namespace DeskSharper.Localization;

/// <summary>
/// Localization strings for the application
/// </summary>
public static class Strings
{
    private static readonly Dictionary<string, Dictionary<string, string>> Translations = new()
    {
        ["en"] = new Dictionary<string, string>
        {
            // Window titles
            ["MainWindowTitle"] = "DeskSharper - Desktop Cleaner",
            ["ProgressWindowTitle"] = "DeskSharper - Progress",

            // Menu items
            ["FileMenu"] = "File",
            ["NewConfig"] = "New Configuration",
            ["LoadConfig"] = "Load Configuration",
            ["SaveConfig"] = "Save Configuration",
            ["DeleteConfig"] = "Delete Configuration",
            ["Exit"] = "Exit",
            ["OptionsMenu"] = "Options",
            ["Language"] = "Language",
            ["HelpMenu"] = "Help",
            ["About"] = "About DeskSharper",

            // Configuration labels
            ["ConfigName"] = "Configuration Name:",
            ["SourcePath"] = "Source Path:",
            ["DestinationPath"] = "Destination Path (empty to delete):",
            ["Browse"] = "Browse...",

            // Options
            ["MoveOptions"] = "Move Options:",
            ["MoveFolders"] = "Move Folders",
            ["MoveShortcuts"] = "Move Shortcuts",

            // Advanced options
            ["AdvancedOptions"] = "Advanced Options",
            ["KeepFiles"] = "Keep Files (whitelist):",
            ["KeepExtensions"] = "Keep Extensions:",
            ["RetentionTime"] = "Retention Time (hours):",
            ["CheckMode"] = "Check Mode:",
            ["LastModified"] = "Last Modified Time",
            ["LastAccessed"] = "Last Accessed Time",

            // Startup options
            ["StartupOptions"] = "Startup Options:",
            ["AutoRun"] = "Run automatically on startup",

            // Buttons
            ["Preview"] = "Preview",
            ["Run"] = "Run",
            ["Save"] = "Save",
            ["Cancel"] = "Cancel",
            ["Close"] = "Close",

            // Progress
            ["Scanning"] = "Scanning...",
            ["Processing"] = "Processing: {0}/{1}",
            ["CurrentFile"] = "Current file:",
            ["Completed"] = "Completed",

            // Notifications
            ["CleaningComplete"] = "Cleaning Complete",
            ["ItemsMoved"] = "{0} items moved/deleted",
            ["ErrorsOccurred"] = "{0} errors occurred",
            ["NoItemsToProcess"] = "No items to process",
            ["DesktopClean"] = "Desktop is clean",
            ["ErrorsTitle"] = "Some Errors Occurred",
            ["ErrorsMessage"] = "The following items couldn't be processed:",

            // Preview
            ["PreviewTitle"] = "Preview - Items to Process",
            ["NoItemsFound"] = "No items found matching the criteria",
            ["ItemCount"] = "{0} items will be processed",

            // Validation
            ["ConfigNameRequired"] = "Configuration name is required",
            ["SourcePathRequired"] = "Source path is required",
            ["SourcePathNotExist"] = "Source path does not exist",
            ["ConfirmDelete"] = "Are you sure you want to delete configuration '{0}'?",
            ["ConfirmEmptyDestination"] =
                "Destination path is empty. Items will be DELETED (not recoverable). Continue?",

            // About
            ["AboutMessage"] =
                "DeskSharper - Desktop Cleaning Tool\n\nVersion: 1.0.0\nA cross-platform desktop organizer\n\n© 2025 DeskSharper"
        },

        ["zh"] = new Dictionary<string, string>
        {
            // Window titles
            ["MainWindowTitle"] = "DeskSharper - 桌面清理工具",
            ["ProgressWindowTitle"] = "DeskSharper - 进度",

            // Menu items
            ["FileMenu"] = "文件",
            ["NewConfig"] = "新建配置",
            ["LoadConfig"] = "加载配置",
            ["SaveConfig"] = "保存配置",
            ["DeleteConfig"] = "删除配置",
            ["Exit"] = "退出",
            ["OptionsMenu"] = "选项",
            ["Language"] = "语言",
            ["HelpMenu"] = "帮助",
            ["About"] = "关于 DeskSharper",

            // Configuration labels
            ["ConfigName"] = "配置名称：",
            ["SourcePath"] = "源路径：",
            ["DestinationPath"] = "目标路径（留空删除）：",
            ["Browse"] = "浏览...",

            // Options
            ["MoveOptions"] = "移动选项：",
            ["MoveFolders"] = "移动文件夹",
            ["MoveShortcuts"] = "移动快捷方式",

            // Advanced options
            ["AdvancedOptions"] = "高级选项",
            ["KeepFiles"] = "保留文件（白名单）：",
            ["KeepExtensions"] = "保留扩展名：",
            ["RetentionTime"] = "保留时间（小时）：",
            ["CheckMode"] = "检查模式：",
            ["LastModified"] = "最后修改时间",
            ["LastAccessed"] = "最后访问时间",

            // Startup options
            ["StartupOptions"] = "启动选项：",
            ["AutoRun"] = "开机自动运行",

            // Buttons
            ["Preview"] = "预览",
            ["Run"] = "运行",
            ["Save"] = "保存",
            ["Cancel"] = "取消",
            ["Close"] = "关闭",

            // Progress
            ["Scanning"] = "扫描中...",
            ["Processing"] = "处理中：{0}/{1}",
            ["CurrentFile"] = "当前文件：",
            ["Completed"] = "完成",

            // Notifications
            ["CleaningComplete"] = "清理完成",
            ["ItemsMoved"] = "已移动/删除 {0} 个项目",
            ["ErrorsOccurred"] = "发生 {0} 个错误",
            ["NoItemsToProcess"] = "没有项目需要处理",
            ["DesktopClean"] = "桌面很干净",
            ["ErrorsTitle"] = "发生一些错误",
            ["ErrorsMessage"] = "以下项目无法处理：",

            // Preview
            ["PreviewTitle"] = "预览 - 待处理项目",
            ["NoItemsFound"] = "未找到符合条件的项目",
            ["ItemCount"] = "将处理 {0} 个项目",

            // Validation
            ["ConfigNameRequired"] = "配置名称不能为空",
            ["SourcePathRequired"] = "源路径不能为空",
            ["SourcePathNotExist"] = "源路径不存在",
            ["ConfirmDelete"] = "确定要删除配置 '{0}' 吗？",
            ["ConfirmEmptyDestination"] = "目标路径为空。项目将被删除（不可恢复）。继续吗？",

            // About
            ["AboutMessage"] = "DeskSharper - 桌面清理工具\n\n版本：1.0.0\n跨平台桌面整理工具\n\n© 2025 DeskSharper"
        }
    };

    private static string _currentLanguage = "en";

    public static string CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            if (Translations.ContainsKey(value))
            {
                _currentLanguage = value;
            }
        }
    }

    public static string Get(string key, params object[] args)
    {
        if (Translations.TryGetValue(_currentLanguage, out var dict) &&
            dict.TryGetValue(key, out var value))
        {
            return args.Length > 0 ? string.Format(value, args) : value;
        }

        return key; // Return key if translation not found
    }

    public static void InitializeFromCulture()
    {
        var culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (culture == "zh")
        {
            CurrentLanguage = "zh";
        }
        else
        {
            CurrentLanguage = "en";
        }
    }
}
