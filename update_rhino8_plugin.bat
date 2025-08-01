@echo off
echo Updating Rhino 8 Contextform plugin...
echo.

REM Close Rhino
echo Closing Rhino...
taskkill /F /IM Rhino.exe 2>nul
taskkill /F /IM Rhino8.exe 2>nul
timeout /t 2 >nul

REM Backup old plugin (just in case)
echo Backing up old plugin...
copy "C:\Users\Joseph\AppData\Roaming\McNeel\Rhinoceros\packages\8.0\Contextform\1.0.0.0\Contextform.rhp" "C:\Users\Joseph\AppData\Roaming\McNeel\Rhinoceros\packages\8.0\Contextform\1.0.0.0\Contextform.rhp.backup" 2>nul

REM Replace with new plugin
echo Replacing with new plugin...
copy /Y "D:\contextform_rhino\Contextform.rhp" "C:\Users\Joseph\AppData\Roaming\McNeel\Rhinoceros\packages\8.0\Contextform\1.0.0.0\Contextform.rhp"

REM Also update the debug folder
copy /Y "D:\contextform_rhino\Contextform.rhp" "C:\Users\Joseph\AppData\Roaming\McNeel\Rhinoceros\packages\8.0\Contextform\1.0.0.0\bin\Debug\net48\Contextform.rhp"

echo.
echo =====================================================
echo Update complete!
echo.
echo The plugin has been updated with the new API endpoint.
echo You can now open Rhino and the new version will load.
echo =====================================================
echo.
pause