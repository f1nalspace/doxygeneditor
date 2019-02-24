@echo off
FastTokenizer.exe custom "..\..\..\..\..\fgt\final_platform_layer.h" > bla_custom.txt
FastTokenizer.exe superpower "..\..\..\..\..\fgt\final_platform_layer.h" > bla_sp.txt
"c:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\devenv.exe" /diff bla_sp.txt bla_custom.txt 
