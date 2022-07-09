@echo off
setlocal enabledelayedexpansion
pushd %~dp0

set OutputName=SymbolTest
set MainSource=Main
set Assembler=..\..\asar.exe

echo --------------------------------------------------
echo               %date% %time%
echo --------------------------------------------------

rem Erase files to avoid patch mode
move /Y "%OutputName%.sfc" "%OutputName%.sfc.old" > NUL 2>&1

:Assemble
%Assembler% %MainSource%.asm "%OutputName%.sfc" > build.log
type build.log

popd
