/*
	Простое приложение для запуска программ от имени другого пользователя с повышенными правами.

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
using CredentialManagement;             // Библиотека для работы с диспетчером учетных данных Windows

namespace RunFromUser
{
    class Program
    {
        static void Main(string[] args)
        {
            //Принимаем ровно 1 аргумент
            if (args.Length != 1)
            {
                //Для отладки, если сменить тип на консольное приложение в свойствах проекта
                Console.WriteLine("Использование: Run.exe <Путь до запускаемой программы>");
                return;
            }

            string applicationPath = args[0];   // Полный путь до запускаемой программы, который нужно передать "Run.exe"

            string runExePath = @"C:\Windows\SysWOW64\Run.exe"; // Путь к самой программе "Run.exe" (для удобства рекомендую переместить её сюда)

            // Запуск процесса от имени пользователя
            StartProcessAsUser(runExePath, applicationPath);    // Запуск процесса "Run.exe" от пользователя, который сохранён в хранилище учетных данных под именем "RunFromUser", а так же передача ему аргумента в виде пути к запускаемой программе
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
        public static void StartProcessAsUser(string runExePath, string applicationPath)
        {
            //Получение имени пользователя и пароля из хранилища Windows
            if (!GetUserCredentials(out string username, out string password))
            {
                //Для отладки, если сменить тип на консольное приложение в свойствах проекта
                //Console.WriteLine("Не удалось получить учетные данные.");
                return;
            }

            //Выполняем вход с предоставленными учетными данными
            bool returnValue = LogonUser(username, ".", password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out IntPtr tokenHandle);

            if (!returnValue)
            {
                //Для отладки, если сменить тип на консольное приложение в свойствах проекта
                //Console.WriteLine($"Ошибка входа пользователя в систему, код ошибки: {Marshal.GetLastWin32Error()}.");
                return;
            }

            try
            {
                //Создаём структуры, необходимые для настройки запуска процесса
                StartupInfo startupInfo = new StartupInfo();
                ProcessInformation processInformation = new ProcessInformation();

                startupInfo.cb = Marshal.SizeOf(startupInfo);                   // Размер структуры "StartupInfo" для корректного создания процесса
                string commandLine = $"\"{runExePath}\" \"{applicationPath}\""; // Формируем строку с аргументами

                //Создание процесса
                bool processCreated = CreateProcessWithLogonW(
                    username,
                    ".",
                    password,
                    0,
                    runExePath,
                    commandLine,
                    0,
                    IntPtr.Zero,
                    null,
                    out startupInfo,
                    out processInformation);

                //if (processCreated)
                //{
                //    //Для отладки, если сменить тип на консольное приложение в свойствах проекта
                //    Console.WriteLine($"Процесс запущен: {applicationPath}");
                //}
            }
            catch
            {
                //Для отладки, если сменить тип на консольное приложение в свойствах проекта
                //Console.WriteLine($"Ошибка создания процесса, код ошибки: {Marshal.GetLastWin32Error()}.");
            }
            finally
            {
                CloseHandle(tokenHandle);   // Закрытие дескриптора токена пользователя, для освобождения ресурсов
            }
        }

        //Получение учетных данных из "Диспетчер учетных данных Windows"
        private static bool GetUserCredentials(out string username, out string password)
        {
            using (var cred = new Credential())    // Создаём новый объект "Credential"
            {
                cred.Target = "RunFromUser"; // Имя для поиска учетных данных в хранилище

                //Попытка загрузить учётные данные
                if (cred.Load())
                {
                    username = cred.Username;   // Присваиваем логин
                    password = cred.Password;   // Присваиваем пароль
                    return true;                // Поднимаем флаг
                }
                else
                {
                    //Для отладки, если сменить тип на консольное приложение в свойствах проекта
                    Console.WriteLine("Не удалось загрузить учетные данные.");

                    username = null;    // Пустые данные
                    password = null;
                    return false;       // Опускаем флаг
                }
            }
        }
    }
}
