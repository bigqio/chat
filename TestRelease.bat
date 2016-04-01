@echo off
cd BigQChatServer\bin\release
start BigQChatServer.exe
TIMEOUT 1 > NUL
cd ..\..\..

cd BigQChatClient\bin\release
start BigQChatClient.exe
start BigQChatClient.exe
cd ..\..\..
@echo on
