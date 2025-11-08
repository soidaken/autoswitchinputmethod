@echo off
chcp 65001 >nul
echo ====================================
echo   输入法自动切换 - 单文件编译
echo ====================================
echo.

echo 正在编译单文件独立版本 (约8-15MB)...
echo 无需安装运行时，可在任何 Windows 10+ 运行
echo.

if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj

dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true

if %errorlevel% equ 0 (
    echo.
    echo ✓ 编译成功!
    echo.
    echo 输出: bin\Release\net6.0-windows\win-x64\publish\InputAutoSwitch.exe
    echo 体积: 约 8-15MB
    echo.
    dir /b bin\Release\net6.0-windows\win-x64\publish\*.exe
    echo.
    pause
    explorer bin\Release\net6.0-windows\win-x64\publish
) else (
    echo.
    echo ✗ 编译失败
    pause
)
