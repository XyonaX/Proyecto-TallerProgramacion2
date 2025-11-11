using System;
using System.Windows.Forms;
using Proyecto_Taller_2.Domain.Models;

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
                var result = login.ShowDialog();
                if (result == DialogResult.OK && login.CurrentUser != null)
                {
                    Usuario usuario = login.CurrentUser;  // 👈 Usuario del dominio
                    Application.Run(new Form1(usuario));
                }
                else
                {
                    // Login fallido o cancelado → cerrar app
                    return;
                }
            }
        }
    }
}
