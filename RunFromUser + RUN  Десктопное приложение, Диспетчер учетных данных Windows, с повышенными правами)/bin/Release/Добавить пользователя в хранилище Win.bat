@echo off

:: ����᪠�� �ਯ� � �ࠢ��� �����������

:: ��⠭�������� �����
PowerShell -NoProfile -Command "Install-Module -Name CredentialManager -Force -Scope AllUsers"

:: ������塞 ���짮��⥫�
PowerShell -NoProfile -Command "New-StoredCredential -Target 'RunFromUser' -Username 'User1' -Password '123456' -Persist LocalMachine"

pause
