/*
	Простое приложение для запуска программ с повышенными правами (Запуск от имени администратора).

    Это консольное приложение, но в свойствах проекта изменено на десктопное, что бы убрать кратковременное всплытие консоли при запуске программы.
    Меняется тип так: Проект -> Свойсва Run -> Приложение -> Тип выходных данных -> "Приложение Windows" или "Консольное приложение".

	Данная программа является свободным программным обеспечением, распространяющимся по лицензии MIT.
	Копия лицензии: https://opensource.org/licenses/MIT

	Copyright (c) 2024 Otto
	Автор: Otto
	Версия: 01.10.24
	GitHub страница:  https://github.com/Otto17/RunFromUser
	GitFlic страница: https://gitflic.ru/project/otto/runfromuser

	г. Омск 2024
*/


using System;               // Библиотека предоставляет доступ к базовым классам и функциональности .NET Framework
using System.Diagnostics;   // Библиотека для работы с отладкой

namespace Run
{
    class Program
    {
        static void Main(string[] args)
        {
            //Принимаем ровно 1 аргумент
            if (args.Length != 1)
            {
                //Для отладки, если сменить тип на консольное приложение в свойствах проекта
                //Console.WriteLine("Использование: Run.exe <Путь до запускаемой программы с повышенными правами>");
                return;
            }

            string applicationPath = args[0];   // Путь до запускаемой программы из аргумента

            try
            {
                //Настройки для запуска процесса с повышенными привилегиями
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = applicationPath,             // Файл, который будем запускать
                    Verb = "runas",                         // Указание запуска с правами администратора
                    UseShellExecute = true,                 // Используем оболочку Windows (Explorer)
                    WindowStyle = ProcessWindowStyle.Normal // Обычный запуск окна программы
                };

                Process process = Process.Start(startInfo); // Запуск нового процесса
            }
            catch (Exception)
            {
                //Для отладки, если сменить тип на консольное приложение в свойствах проекта
               //Console.WriteLine("Ошибка при запуске программы: " + ex.Message);
            }
        }
    }
}
