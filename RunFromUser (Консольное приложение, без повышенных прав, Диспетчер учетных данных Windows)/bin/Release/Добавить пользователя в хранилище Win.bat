@echo off

:: Запускать скрипт с правами администратора

:: Устанавливаем можуль
PowerShell -NoProfile -Command "Install-Module -Name CredentialManager -Force -Scope AllUsers"

:: Добавляем пользователя
PowerShell -NoProfile -Command "New-StoredCredential -Target 'RunFromUser' -Username 'User1' -Password '123456' -Persist LocalMachine"

pause
