@echo off
rem Requires Inkscape and ImageMagick installed in default path

if "%1"=="clean" goto clean
setlocal enableDelayedExpansion
set icolist=
set inkscape_path=%ProgramFiles%\Inkscape\
for %%k in (*.svg) do (
  "%inkscape_path%inkscape" -C -f "%%k" -e "%%~nk.png"
  set icolist=!icolist! "%%~nk.png"
)

magick%icolist% ..\DotNetEditor\DotNetEditor.ico
endlocal
goto end

:clean
del *.png /q

:end
