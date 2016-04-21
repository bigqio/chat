@echo off
IF [%1] == [] GOTO Usage
cd BigQChatServer\bin\debug
start BigQChatServer.exe 8222 5000 false
TIMEOUT 2 > NUL
cd ..\..\..

cd BigQChatClient\bin\debug
FOR /L %%i IN (1,1,%1) DO (
ECHO Starting client %%i
start BigQChatClient.exe localhost 8222 node%%i 5000 false
TIMEOUT 1 > NUL
)
cd ..\..\..
@echo on
EXIT /b

:Usage
ECHO Specify the number of client nodes to start.
@echo on
EXIT /b
