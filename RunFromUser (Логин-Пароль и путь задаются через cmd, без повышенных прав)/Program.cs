/*
	Простое приложение для запуска программ от имени другого пользователя (без повышенных привилегий).

    Данная программа НЕ может запустить программу, которая требует обязательно запуск только с повышенными правами, но это можно легко обойти.

    Для обхода запуска программ с повышенными правами эта программа "RunFromUser" должна запустить программу "Run" от пользователя с административными правами,
    а программа "RUN" в свою очередь запускает уже программу от пользователя с административными правами + с повышенными правами.
    К сожалению в одной программе такое провернуть нельзя из-за ограничений безопасности Windows.

    Это консольное приложение, но в свойствах проекта можно изменить на десктопное, что бы убрать кратковременное всплытие консоли при запуске программы.
    Меняется тип так: Проект -> Свойства Run -> Приложение -> Тип выходных данных -> "Приложение Windows" или "Консольное приложение".

	Данная программа является свободным программным обеспечением, распространяющимся по лицензии MIT.
	Копия лицензии: https://opensource.org/licenses/MIT

	Copyright (c) 2024 Otto
	Автор: Otto
	Версия: 01.10.24
	GitHub страница:  https://github.com/Otto17/RunFromUser
	GitFlic страница: https://gitflic.ru/project/otto/runfromuser

	г. Омск 2024
*/


using System;                           // Библиотека предоставляет доступ к базовым классам и функциональности .NET Framework
using System.Runtime.InteropServices;   // Библиотека для взаимодействия с неуправляемым кодом

namespace RunFromUser
{
    class Program
    {
        static void Main(string[] args)
        {
            //Принимаем ровно 3 аргумента
            if (args.Length != 3)
            {
                //Для отладки, если сменить тип на консольное приложение в свойствах проекта
                Console.WriteLine("Использование: RunFromUser.exe <Имя пользователя> <Пароль> <Путь до программы>");
                return;
            }

            string userName = args[0];          // Имя пользователя
            string password = args[1];          // Пароль
            string applicationPath = args[2];   // Путь до запускаемой программы

            //Запуск процесса от имени пользователя
            StartProcessAsUser(userName, password, applicationPath);
        }

        //Подключение библиотеки для аутентификации пользователя по имени пользователя, паролю и домену. Она возвращает токен, который будет использоваться для создания процесса
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken);

        //Подключение библиотеки для  закрытия дескрипторов Windows и освобождения ресурсов
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        extern static bool CloseHandle(IntPtr handle);

        //Подключение библиотеки для запуска нового процесса с использованием учетных данных, полученных от функции "LogonUser"
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CreateProcessWithLogonW(
            string username,
            string domain,
            string password,
            int logonFlags,
            string applicationName,
            string commandLine,
            uint creationFlags,
            IntPtr environment,
            string currentDirectory,
            [Out] out StartupInfo startupInfo,
            [Out] out ProcessInformation processInformation);

        //Структура, для хранения информации о том, как запускать новый процесс. Содержит параметры: размер окна, заголовок, позиция окна и т.д.
        [StructLayout(LayoutKind.Sequential)]
        public struct StartupInfo
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public int dwProcessId;
            public int dwThreadId;
        }

        //Структура для получения информации о созданном процессе, такой как дескрипторы процесса и потока, а также их идентификаторы
        [StructLayout(LayoutKind.Sequential)]
        public struct ProcessInformation
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        //Эти константы используются при вызове "LogonUser" и "CreateProcessWithLogonW", чтобы определить тип входа и провайдера входа
        const int LOGON32_LOGON_INTERACTIVE = 2;
        const int LOGON32_PROVIDER_DEFAULT = 0;


        //Метод запуска процесса от указанного пользователя
        public static void StartProcessAsUser(string userName, string password, string applicationPath)
        {
            //Выполняем вход с предоставленными учетными данными
            bool returnValue = LogonUser(userName, ".", password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out IntPtr tokenHandle);

            if (!returnValue)
            {
                //Для отладки, если сменить тип на консольное приложение в свойствах проекта
                Console.WriteLine($"Ошибка входа пользователя в систему, код ошибки: {Marshal.GetLastWin32Error()}.");
                return;
            }

            try
            {
                //Создаём структуры, необходимые для настройки запуска процесса
                StartupInfo startupInfo = new StartupInfo();
                ProcessInformation processInformation = new ProcessInformation();

                startupInfo.cb = Marshal.SizeOf(startupInfo);   // Размер структуры "StartupInfo" для корректного создания процесса

                //Создание процесса
                bool processCreated = CreateProcessWithLogonW(
                    userName,
                    ".",
                    password,
                    0,
                    applicationPath,
                    null,
                    0,
                    IntPtr.Zero,
                    null,
                    out startupInfo,
                    out processInformation);

                if (processCreated)
                {
                    //Для отладки, если сменить тип на консольное приложение в свойствах проекта
                    Console.WriteLine($"Процесс запущен: {applicationPath}");
                }
            }
            catch
            {
                //Для отладки, если сменить тип на консольное приложение в свойствах проекта
                Console.WriteLine($"Ошибка создания процесса, код ошибки: {Marshal.GetLastWin32Error()}.");
            }
            finally
            {
                // Закрытие токена
                CloseHandle(tokenHandle);   // Закрытие дескриптора токена пользователя, для освобождения ресурсов
            }
        }
    }
}
