@echo off

:: Объединение скомпилированной программы с библиотекой

ILMerge.exe /out:RunFromUser.exe RunFromUser.exe CredentialManagement.dll

:: Удаляем мусор, который может мешать при повторной перекомпиляции и объединению файлов
del RunFromUser.pdb CredentialManagement.dll RunFromUser.exe.config
