using System.Drawing;
using System.Windows.Forms;

namespace Proyecto_Taller_2
{
    public static class ThemeHelper
    {
        public static void ApplyTheme(Control root, AppSettings s)
        {
            var dark = s.Tema == "Oscuro" || (s.Tema == "Sistema" && SystemInformation.HighContrast);
            Color bg = dark ? Color.FromArgb(30, 33, 36) : Color.White;
            Color fg = dark ? Color.WhiteSmoke : Color.FromArgb(30, 30, 30);
            Color card = dark ? Color.FromArgb(40, 44, 48) : Color.FromArgb(248, 250, 248);

            root.BackColor = bg;
            ApplyRecursive(root, fg, bg, card, s);
        }

        private static void ApplyRecursive(Control c, Color fg, Color bg, Color card, AppSettings s)
        {
            c.Font = new Font(c.Font.FontFamily, s.FontSize, c.Font.Style);
            c.ForeColor = fg;

            if (c is RoundedPanel rp)
            {
                rp.BackColor = card;
                rp.BorderColor = Color.FromArgb(210, 214, 210);
            }

            foreach (Control child in c.Controls)
                ApplyRecursive(child, fg, bg, card, s);
        }
    }
}
