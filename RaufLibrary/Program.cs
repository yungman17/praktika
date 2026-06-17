// ============================================================
//  Рауф — Итоговый проект: "Система управления библиотекой"
//  Файл: Program.cs — точка входа
// ============================================================
using System;
using System.Windows.Forms;

namespace LibrarySystem
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
