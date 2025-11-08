# 开发环境配置

## 前置要求

### 1. 安装 .NET 6.0 SDK

**下载地址**: https://dotnet.microsoft.com/download/dotnet/6.0

选择以下任一版本:
- **.NET 6.0 SDK** (开发和编译需要)
- **.NET 6.0 Runtime - Desktop** (仅运行程序需要)

**安装步骤**:
1. 下载 Windows x64 版本的安装包
2. 运行安装程序,按默认选项安装
3. 安装完成后,打开PowerShell验证:
   ```powershell
   dotnet --version
   ```
   应该显示类似: `6.0.xxx`

### 2. 可选: 安装 Visual Studio 2022

如果你想使用图形界面开发:

**下载地址**: https://visualstudio.microsoft.com/zh-hans/downloads/

选择 **Visual Studio 2022 Community** (免费)

**安装时选择的工作负载**:
- ✅ .NET 桌面开发

## 快速开始

### 如果你已经安装了 .NET SDK:

```powershell
# 1. 进入项目目录
cd E:\soida\selftools\inputautoswitch

# 2. 恢复依赖
dotnet restore

# 3. 快速编译 (调试版本,用于测试)
dotnet build

# 4. 运行程序
dotnet run

# 5. 发布版本 (单文件exe)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true
```

### 如果你没有安装 .NET SDK:

#### 方案A: 使用在线编译服务 (不推荐)
如果不想安装SDK,可以:
1. 将项目上传到 GitHub
2. 使用 GitHub Actions 自动编译
3. 下载编译好的exe

#### 方案B: 使用已编译的exe (如果有)
如果其他地方已经编译好了exe,直接复制使用即可,无需任何运行时。

#### 方案C: 安装 .NET SDK (推荐)
这是最简单的方式,下载安装即可,安装包约100MB。

## 项目结构说明

```
inputautoswitch/
├── InputAutoSwitch.csproj    # 项目配置文件
├── Program.cs                # 程序入口
├── MainForm.cs               # 主窗体(托盘程序)
├── KeyboardHook.cs           # 键盘钩子
├── InputMethodManager.cs     # 输入法管理
├── AutoStartManager.cs       # 开机自启动管理
├── build.bat                 # 快速编译脚本
├── README.md                 # 详细说明文档
├── QUICKSTART.md             # 快速开始指南
└── SETUP.md                  # 本文件
```

## 编译输出位置

不同编译方式的输出位置:

| 编译方式 | 输出路径 |
|---------|---------|
| `dotnet build` | `bin\Debug\net6.0-windows\` |
| `dotnet build -c Release` | `bin\Release\net6.0-windows\` |
| `dotnet publish` | `bin\Release\net6.0-windows\win-x64\publish\` |
| 使用 `build.bat` | `bin\Release\net6.0-windows\win-x64\publish\` |

## 常见问题

### Q: 我应该安装 SDK 还是 Runtime?

- **开发/编译程序**: 安装 SDK (包含Runtime)
- **只运行程序**: 安装 Runtime (更小,只有50MB左右)
- **运行单文件发布的exe**: 什么都不需要安装

### Q: 编译后的exe太大了?

单文件发布版本约15MB,这已经是WinForms应用的正常大小(不支持裁剪)。如果需要更小体积,使用依赖框架的版本只有~100KB,但用户需要安装.NET 6运行时。

### Q: 可以不安装.NET直接运行吗?

可以! 使用单文件发布方式编译的exe,内置了所有依赖,可以直接在任何Windows 10+系统上运行。

### Q: 我想要最小的exe体积

有两个选择:
1. **依赖框架版本** (~100KB): 需要系统安装.NET 6 Runtime
2. **单文件独立版本** (~15MB): 无需任何依赖,独立运行

建议使用方案2,15MB对于现代电脑来说很小,而且无需用户安装运行时。

## 下一步

安装好.NET SDK后:
1. 阅读 `QUICKSTART.md` 了解如何编译和运行
2. 阅读 `README.md` 了解程序的详细功能
3. 运行 `build.bat` 一键编译

祝使用愉快! 🎉
