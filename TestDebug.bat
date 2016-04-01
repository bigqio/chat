@echo off
cd BigQChatServer\bin\debug
start BigQChatServer.exe
TIMEOUT 1 > NUL
cd ..\..\..

cd BigQChatClient\bin\debug
start BigQChatClient.exe
start BigQChatClient.exe
cd ..\..\..
@echo on
