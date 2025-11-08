@echo off
chcp 65001 >nul
echo ====================================
echo   输入法自动切换工具 - 快速编译脚本
echo ====================================
echo.

echo [1/3] 清理旧的编译文件...
if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj

echo [2/3] 开始编译 (Release 配置)...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true

if %errorlevel% equ 0 (
    echo.
    echo [3/3] 编译成功!
    echo.
    echo 输出文件位置:
    echo bin\Release\net6.0-windows\win-x64\publish\InputAutoSwitch.exe
    echo.
    echo 按任意键打开输出目录...
    pause >nul
    explorer bin\Release\net6.0-windows\win-x64\publish
) else (
    echo.
    echo [错误] 编译失败,请检查错误信息
    echo.
    pause
)
