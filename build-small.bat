@echo off
chcp 65001 >nul
echo ====================================
echo   输入法自动切换 - 小体积编译
echo ====================================
echo.

echo 正在编译依赖框架单文件版本 (约500KB-1MB)...
echo 需要系统安装 .NET 6 Desktop Runtime
echo.

if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj

dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true

if %errorlevel% equ 0 (
    echo.
    echo ✓ 编译成功!
    echo.
    echo 输出: bin\Release\net6.0-windows\win-x64\publish\InputAutoSwitch.exe
    echo 体积: 约 500KB-1MB (单文件)
    echo.
    cd bin\Release\net6.0-windows\win-x64\publish
    dir InputAutoSwitch.exe
    cd ..\..\..\..\..\
    echo.
    echo 此版本是单文件，可直接运行（需要.NET 6 Runtime）
    echo.
    pause
    explorer bin\Release\net6.0-windows\win-x64\publish
) else (
    echo.
    echo ✗ 编译失败
    pause
)
