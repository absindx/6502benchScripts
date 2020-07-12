@rem --------------------------------------------------
@rem  6502bench SourceGen -> Mesen source debug bridge
@rem --------------------------------------------------

@echo off

rem Assembler settings
set cc65=..\cc65
set Assembler=%cc65%\bin\ca65.exe
set Linker=%cc65%\bin\ld65.exe

rem ROM settings
set Name=RomName
set NameSuffix=_Rebuild

@rem --------------------------------------------------

rem Assemble with debug info
%Assembler% --debug-info --target none "%Name%.nes_cc65.S"

rem Generate dbg file
%Linker% --dbgfile "%Name%.dbg" -C "%Name%.nes_cc65.cfg" -o "%Name%%NameSuffix%.nes" "%Name%.nes_cc65.o"

rem Delete intermediate files
del "%Name%.nes_cc65.o"
rem del "%Name%%NameSuffix%.nes"
