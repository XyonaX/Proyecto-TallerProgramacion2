using System;
using System.Windows.Forms;
using Proyecto_Taller_2.Domain.Entities;

namespace Proyecto_Taller_2.UI
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var login = new LoginForm()) 
            {
                if (login.ShowDialog() == DialogResult.OK && login.CurrentUser != null)
                {
                    Application.Run(new Form1(login.CurrentUser));
                }
            }
        }
    }
}
