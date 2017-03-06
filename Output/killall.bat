@echo off  
taskkill /f /im efwplusBase.exe
taskkill /f /im efwplusRoute.exe
taskkill /f /im efwplusWebAPI.exe
taskkill /f /im efwplusServerCmd.exe
taskkill /f /im efwplusServer.exe
taskkill /f /im mongod.exe
taskkill /f /im nginx.exe
exit