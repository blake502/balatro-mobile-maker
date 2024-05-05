@echo off

set app_version_name=beta-0.8.3

echo Clearing bin, publish, and obj folders.
rmdir bin /s /q
rmdir obj /s /q
rmdir publish /s /q

mkdir publish

echo Building win-x64
dotnet publish -o bin\publish\win-x64 --self-contained -f net8.0 --runtime win-x64 -p:PublishTrimmed=true
move .\bin\publish\win-x64\balatro-mobile-maker.exe .\publish\balatro-mobile-maker-%app_version_name%-win-x64.exe

rem echo Building win-x86
rem dotnet publish -o bin\publish\win-x86 --self-contained -f net8.0 --runtime win-x86 -p:PublishTrimmed=true
rem move .\bin\publish\win-x86\balatro-mobile-maker.exe .\publish\balatro-mobile-maker-%app_version_name%-win-x86.exe

rem Apparently this is a thing, but it doesn't build on my system. Oh well.
rem echo Building win-arm
rem dotnet publish -o bin\publish\win-arm --self-contained -f net8.0 --runtime win-arm -p:PublishTrimmed=true
rem move .\bin\publish\win-arm\balatro-mobile-maker.exe .\publish\balatro-mobile-maker-%app_version_name%-win-arm.exe

echo Building win-arm64
dotnet publish -o bin\publish\win-arm64 --self-contained -f net8.0 --runtime win-arm64 -p:PublishTrimmed=true
move .\bin\publish\win-arm64\balatro-mobile-maker.exe .\publish\UNTESTED-balatro-mobile-maker-%app_version_name%-win-arm64.exe

echo Building osx-x64
dotnet publish -o bin\publish\osx-x64 --self-contained -f net8.0 --runtime osx-x64 -p:PublishTrimmed=true
move .\bin\publish\osx-x64\balatro-mobile-maker .\publish\UNTESTED-balatro-mobile-maker-%app_version_name%-osx-x64

echo Building osx-arm64
dotnet publish -o bin\publish\osx-arm64 --self-contained -f net8.0 --runtime osx-arm64 -p:PublishTrimmed=true
move .\bin\publish\osx-arm64\balatro-mobile-maker .\publish\balatro-mobile-maker-%app_version_name%-osx-arm64-expeimental

echo Building linux-x64
dotnet publish -o bin\publish\linux-x64 --self-contained -f net8.0 --runtime linux-x64 -p:PublishTrimmed=true
move .\bin\publish\linux-x64\balatro-mobile-maker .\publish\balatro-mobile-maker-%app_version_name%-linux-x64-expeimental

rem echo Building linux-arm
rem dotnet publish -o bin\publish\linux-arm --self-contained -f net8.0 --runtime linux-arm -p:PublishTrimmed=true
rem move .\bin\publish\linux-arm\balatro-mobile-maker .\publish\balatro-mobile-maker-%app_version_name%-linux-arm

echo Building linux-arm64
dotnet publish -o bin\publish\linux-arm64 --self-contained -f net8.0 --runtime linux-arm64 -p:PublishTrimmed=true
move .\bin\publish\linux-arm64\balatro-mobile-maker .\publish\UNTESTED-balatro-mobile-maker-%app_version_name%-linux-arm64