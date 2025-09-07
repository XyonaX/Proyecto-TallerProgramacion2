using System;
using System.Windows.Forms;

namespace Proyecto_Taller_2
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 🔹 Cargar configuraciones
            SettingsService.Load();

            Application.Run(new Form1());
        }
    }
}
