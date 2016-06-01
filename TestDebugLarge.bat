@echo off
IF [%1] == [] GOTO Usage
cd BigQChatServer\bin\debug
start BigQChatServer.exe 8222 0 false
TIMEOUT 3 > NUL
cd ..\..\..

cd BigQChatClient\bin\debug
FOR /L %%i IN (1,1,%1) DO (
ECHO Starting client %%i
start BigQChatClient.exe localhost 8222 node%%i 0 false
TIMEOUT 5 > NUL
)
cd ..\..\..
@echo on
EXIT /b

:Usage
ECHO Specify the number of client nodes to start.
@echo on
EXIT /b
