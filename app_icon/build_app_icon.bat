@echo off
if "%1"=="build" goto build
if "%1"=="clean" goto clean

echo ICO file builder
echo ================
echo Builds the target ICO file with every SVG file in this directory. Requires
echo Inkscape and ImageMagick to be installed before running.
echo+
echo Usage:
echo     %~n0 build - builds the ICO file (and intermediate PNG files)
echo     %~n0 clean - deletes the intermediate PNG files
echo+

choice /m "Continue to build the ICO file"
if errorlevel 2 goto end
echo+

:build
echo Creating PNG files from SVGs...
echo+
echo Note: Inkscape may fail to exit after creating the PNG file. In this case,
echo terminate Inkscape in the Task Manager to continue execution.
echo+
setlocal enableDelayedExpansion
set target=..\DotNetEditor\DotNetEditor.ico

set icolist=
set inkscape_path=%ProgramFiles%\Inkscape\
for %%k in (*.svg) do (
  echo Processing %%k...
  "%inkscape_path%inkscape" -C -f "%%k" -e "%%~nk.png">nul
  set icolist=!icolist! "%%~nk.png"
)

echo+
echo PNG creation finished. Now creating ICO file from PNGs...
magick%icolist% "%target%"
endlocal
goto end

:clean
echo Removing PNG files...
del *.png /q

:end
