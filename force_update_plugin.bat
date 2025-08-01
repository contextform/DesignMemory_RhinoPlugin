@echo off
echo Force updating Contextform plugin...
echo.

REM Kill Rhino completely
echo Step 1: Closing all Rhino processes...
taskkill /F /IM Rhino.exe 2>nul
taskkill /F /IM Rhino7.exe 2>nul
taskkill /F /IM Rhino8.exe 2>nul
timeout /t 3 >nul

REM Clean all possible plugin locations
echo Step 2: Removing ALL Contextform plugin traces...

REM Rhino 7 locations
rmdir /s /q "%APPDATA%\McNeel\Rhinoceros\7.0\Plug-ins\Contextform (3f85ca5d-7b5b-4c0e-9f2a-1d6e8c4b9a2e)" 2>nul
rmdir /s /q "%APPDATA%\McNeel\Rhinoceros\7.0\Plug-ins\3f85ca5d-7b5b-4c0e-9f2a-1d6e8c4b9a2e" 2>nul
del /q "%APPDATA%\McNeel\Rhinoceros\7.0\Plug-ins\Contextform.rhp" 2>nul

REM Rhino 8 locations  
rmdir /s /q "%APPDATA%\McNeel\Rhinoceros\8.0\Plug-ins\Contextform (3f85ca5d-7b5b-4c0e-9f2a-1d6e8c4b9a2e)" 2>nul
rmdir /s /q "%APPDATA%\McNeel\Rhinoceros\8.0\Plug-ins\3f85ca5d-7b5b-4c0e-9f2a-1d6e8c4b9a2e" 2>nul
del /q "%APPDATA%\McNeel\Rhinoceros\8.0\Plug-ins\Contextform.rhp" 2>nul

REM Clean registry entries for Rhino 7
echo Step 3: Cleaning registry entries...
reg delete "HKEY_CURRENT_USER\Software\McNeel\Rhinoceros\7.0\Plug-Ins\3f85ca5d-7b5b-4c0e-9f2a-1d6e8c4b9a2e" /f 2>nul
reg delete "HKEY_LOCAL_MACHINE\Software\McNeel\Rhinoceros\7.0\Plug-Ins\3f85ca5d-7b5b-4c0e-9f2a-1d6e8c4b9a2e" /f 2>nul

REM Clean registry entries for Rhino 8
reg delete "HKEY_CURRENT_USER\Software\McNeel\Rhinoceros\8.0\Plug-Ins\3f85ca5d-7b5b-4c0e-9f2a-1d6e8c4b9a2e" /f 2>nul
reg delete "HKEY_LOCAL_MACHINE\Software\McNeel\Rhinoceros\8.0\Plug-Ins\3f85ca5d-7b5b-4c0e-9f2a-1d6e8c4b9a2e" /f 2>nul

echo.
echo Step 4: Creating clean install script...

REM Create a Rhino script to install the plugin
echo _-PluginManager _Load "D:\contextform_rhino\Contextform.rhp" _Enter > "%TEMP%\install_contextform.txt"

echo.
echo =====================================================
echo Cleanup complete!
echo.
echo Now please:
echo 1. Open Rhino
echo 2. Type: ReadCommandFile
echo 3. Select: %TEMP%\install_contextform.txt
echo.
echo Or manually drag and drop:
echo D:\contextform_rhino\Contextform.rhp
echo =====================================================
echo.
pause