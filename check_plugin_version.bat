@echo off
echo Checking which Contextform plugin is installed...
echo.

echo Looking for Contextform plugin files...
echo.

echo === Your NEW plugin location ===
dir "D:\contextform_rhino\Contextform.rhp" 2>nul
echo.

echo === Rhino 7 Plugin Cache ===
dir "%APPDATA%\McNeel\Rhinoceros\7.0\Plug-ins\*" /s /b 2>nul | findstr /i "contextform"
echo.

echo === Rhino 8 Plugin Cache ===
dir "%APPDATA%\McNeel\Rhinoceros\8.0\Plug-ins\*" /s /b 2>nul | findstr /i "contextform"
echo.

echo === Recently Modified Files ===
echo Looking for recently modified plugin files...
forfiles /P "%APPDATA%\McNeel\Rhinoceros" /S /M *.rhp /D +0 /C "cmd /c echo @path - @fdate @ftime" 2>nul

echo.
pause