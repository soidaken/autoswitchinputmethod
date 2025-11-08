# 快速开始指南

## 立即编译并运行

### 最简单的方式 (推荐)
双击运行 `build.bat` 即可自动编译,完成后会自动打开输出目录。

### 使用命令行
```powershell
# 进入项目目录
cd E:\soida\selftools\inputautoswitch

# 编译 (单文件发布)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true

# 运行
.\bin\Release\net6.0-windows\win-x64\publish\InputAutoSwitch.exe
```

## 编译选项对比

| 方式 | 命令 | 体积 | 需要运行时 |
|------|------|------|-----------|
| 依赖框架 | `dotnet build -c Release` | ~100KB | 需要.NET 6 |
| 单文件 | `dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true` | ~15MB | 不需要 |

**推荐使用**: 单文件版本 (约15MB),无需安装运行时,复制到任何Windows 10+电脑都能直接运行。

> **说明**: Windows Forms 不支持裁剪发布,但15MB对于现代电脑来说已经很小了。

## 快速测试

1. 编译完成后,运行 `InputAutoSwitch.exe`
2. 程序会最小化到系统托盘(任务栏右下角)
3. 打开任意文本编辑器,切换到中文输入法
4. 输入一些文字后,停止输入,等待2秒
5. 观察输入法是否自动切换到英文模式

## 配置开机自启动

右键托盘图标 -> 勾选"开机自启动"即可。
