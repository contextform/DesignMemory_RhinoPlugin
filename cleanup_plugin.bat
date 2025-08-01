@echo off
echo Cleaning up old Contextform plugin installations...
echo.

REM Kill Rhino if running
echo Closing Rhino if it's running...
taskkill /F /IM Rhino.exe 2>nul
timeout /t 2 >nul

REM Search and remove old plugin installations
echo Searching for old Contextform installations...
for /d %%i in ("%APPDATA%\McNeel\Rhinoceros\*\Plug-ins\Contextform*") do (
    echo Found: %%i
    rmdir /s /q "%%i" 2>nul
    echo Removed: %%i
)

REM Also check for numbered versions
for /d %%i in ("%APPDATA%\McNeel\Rhinoceros\*\Plug-ins\*Contextform*") do (
    echo Found: %%i
    rmdir /s /q "%%i" 2>nul
    echo Removed: %%i
)

echo.
echo Cleanup complete!
echo.
echo You can now:
echo 1. Open Rhino
echo 2. Drag and drop the new Contextform.rhp file
echo.
pause