# DeskSharper 🧹

<div align="center">

![DeskSharper Logo](https://via.placeholder.com/150x150?text=🧹)

**_A cross-platform desktop cleaning tool built with .NET 9 and Avalonia UI_**

[![Build Status](https://github.com/hnrobert/desksharper/workflows/Build%20and%20Test/badge.svg)](https://github.com/hnrobert/desksharper/actions)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![Avalonia](https://img.shields.io/badge/Avalonia-11.3-red.svg)](https://avaloniaui.net/)

[English](#english) | [中文](#中文)

</div>

---

## English

### 📖 Overview

**DeskSharper** is a modern, cross-platform desktop cleaning and organization tool inspired by the Movefile project. It provides intelligent file management capabilities to automatically clean and organize your desktop or any specified directory across Windows, Linux, and macOS.

### ✨ Features

- 🌍 **Cross-Platform**: Runs natively on Windows, Linux, and macOS
- ⚡ **Native AOT Compilation**: Fast startup and low memory footprint thanks to .NET Native AOT
- 🎨 **Modern UI**: Beautiful, responsive interface built with Avalonia UI
- 🔧 **Flexible Configuration**: Multiple saved configurations with customizable rules
- 📁 **Smart Filtering**:
  - Whitelist files by name
  - Keep specific file extensions
  - Set retention time (hours) for file age filtering
  - Choose between last modified or last accessed time
- 🗂️ **Advanced Options**:
  - Move or delete files
  - Handle folders and shortcuts
  - Preview before execution
- 🚀 **Auto-Run**: Configure tasks to run automatically on system startup
- 🌐 **Multilingual**: Supports English and Chinese (automatically detects system language)
- 📝 **Comprehensive Logging**: Track all operations with detailed logs

### 🛠️ Technology Stack

- **.NET 9**: Latest .NET framework with Native AOT support
- **Avalonia UI 11.3**: Cross-platform XAML-based UI framework
- **MVVM Pattern**: Clean architecture with CommunityToolkit.Mvvm
- **Dependency Injection**: Built-in DI container for service management
- **Serilog**: Structured logging with file rotation

### 📦 Installation & Usage

For detailed installation instructions, build guides, and usage examples, please see the full README content above.

---

## 中文

### 📖 概述

**DeskSharper** 是一个现代化的跨平台桌面清理和整理工具，受 Movefile 项目启发。它提供智能文件管理功能，可在 Windows、Linux 和 macOS 上自动清理和整理您的桌面或任何指定目录。

### ✨ 功能特性

- 🌍 **跨平台**：原生支持 Windows、Linux 和 macOS
- ⚡ **Native AOT 编译**：得益于 .NET Native AOT，启动快速，内存占用低
- 🎨 **现代化界面**：使用 Avalonia UI 构建的美观、响应式界面
- 🔧 **灵活配置**：多个保存的配置，可自定义规则
- 📁 **智能过滤**：按名称白名单、保留扩展名、文件保留时间
- 🚀 **自动运行**：配置任务在系统启动时自动运行
- 🌐 **多语言**：支持英文和中文（自动检测系统语言）

### 🚀 快速开始

```bash
# 克隆仓库
git clone https://github.com/hnrobert/desksharper.git
cd desksharper

# 构建并运行
dotnet run --project src/DeskSharper/DeskSharper.csproj
```

详细使用说明请参考上方完整 README 内容。

---

<div align="center">

Made with ❤️ by the DeskSharper Team

**[Report Bug](https://github.com/hnrobert/desksharper/issues)** · **[Request Feature](https://github.com/hnrobert/desksharper/issues)**

</div>
