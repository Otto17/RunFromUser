@echo off

:: ��ꥤ������ ᪮�����஢����� �ணࠬ�� � ������⥪��

ILMerge.exe /out:RunFromUser.exe RunFromUser.exe CredentialManagement.dll

:: ����塞 ����, ����� ����� ����� �� ����୮� ��४������樨 � ��ꥤ������ 䠩���
del RunFromUser.pdb CredentialManagement.dll RunFromUser.exe.config
