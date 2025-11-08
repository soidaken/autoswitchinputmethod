# 输入法自动切换工具

一个轻量级的Windows工具,自动检测输入法状态并在空闲时切换到英文模式。

## 功能特性

- ✅ **自动检测**: 键盘空闲2秒后自动检测输入法状态
- ✅ **智能切换**: 检测到中文输入法时自动切换到英文模式
- ✅ **系统托盘**: 最小化到系统托盘,不占用任务栏
- ✅ **开机自启**: 支持开机自动启动
- ✅ **资源占用低**: 使用Windows原生API,无第三方依赖
- ✅ **防止多开**: 自动检测并防止重复运行

## 工作原理

1. 程序在后台持续监听键盘活动
2. 检测到键盘输入后,重置2秒定时器
3. 当键盘空闲2秒后,检查当前输入法状态
4. 如果输入法为中文模式,自动模拟按下 `Ctrl+Space` 切换到英文
5. 如果已经是英文模式,不执行任何操作

## 系统要求

- Windows 10 或更高版本
- .NET 6.0 Runtime (Windows桌面版)

## 编译方法

### 方法一: 使用 Visual Studio 2022
1. 打开 Visual Studio 2022
2. 打开项目: `InputAutoSwitch.csproj`
3. 选择 Release 配置
4. 生成 -> 生成解决方案
5. 编译后的exe位于: `bin\Release\net6.0-windows\`

### 方法二: 使用命令行 (推荐)

**标准编译 (需要.NET 6运行时):**
```powershell
cd E:\soida\selftools\inputautoswitch
dotnet publish -c Release -r win-x64
```
编译输出: `bin\Release\net6.0-windows\win-x64\publish\InputAutoSwitch.exe`

**单文件发布 (无需运行时,体积 ~15MB):**
```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true
```

> **注意**: Windows Forms 不支持裁剪发布(PublishTrimmed),单文件版本约15MB已经很小了。

### 方法三: 使用 .NET Framework 版本 (最小体积)
如果需要更小的体积,可以改用 .NET Framework 4.8 版本:
- 修改 `.csproj` 中的 `TargetFramework` 为 `net48`
- 编译后的exe只有几十KB,但需要系统已安装.NET Framework 4.8

## 使用说明

### 启动程序
双击 `InputAutoSwitch.exe` 运行,程序会自动最小化到系统托盘。

### 托盘菜单功能
右键点击托盘图标,可以看到以下选项:
- **启用自动切换**: 开启/关闭自动切换功能(默认开启)
- **开机自启动**: 设置开机自动运行
- **退出**: 退出程序

### 查看状态
双击托盘图标可以查看当前程序状态和功能说明。

## 注意事项

1. **首次运行**: 程序需要监听全局键盘事件,Windows可能会弹出防火墙提示,请选择允许
2. **输入法兼容性**: 本工具支持微信输入法、微软拼音等标准IME输入法
3. **循环触发**: 程序已实现防循环机制,自动模拟的按键不会被重复检测
4. **管理员权限**: 在某些应用中使用时可能需要管理员权限运行

## 卸载方法

1. 右键托盘图标 -> 取消勾选"开机自启动"
2. 右键托盘图标 -> 退出
3. 删除程序文件

## 技术架构

- **语言**: C# 10
- **框架**: .NET 6.0 / Windows Forms
- **API**: Win32 API (user32.dll, imm32.dll)
  - `SetWindowsHookEx`: 全局键盘钩子
  - `ImmGetConversionStatus`: 输入法状态检测
  - `SendInput`: 按键模拟

## 常见问题

**Q: 为什么有时候不自动切换?**  
A: 确保满足以下条件:
   - 程序正在运行(托盘有图标)
   - "启用自动切换"已勾选
   - 键盘真正空闲了2秒
   - 当前确实是中文输入模式

**Q: 如何修改空闲时间?**  
A: 修改 `MainForm.cs` 中的 `IDLE_SECONDS` 常量,然后重新编译

**Q: 支持其他输入法切换快捷键吗?**  
A: 当前版本只支持 `Ctrl+Space`,如需其他快捷键需修改 `InputMethodManager.cs` 中的 `SendCtrlSpace` 方法

## 版本历史

- **v1.0.0** (2025-10-31)
  - 初始版本
  - 实现基本的自动切换功能
  - 支持系统托盘和开机自启动

## 许可协议

本项目仅供个人学习和使用。

## 作者

Soida

---

**如有问题或建议,欢迎反馈!**
