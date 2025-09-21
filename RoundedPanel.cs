using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Proyecto_Taller_2
{
    public class RoundedPanel : Panel
    {
        public int Radius { get; set; } = 14;
        public int BorderThickness { get; set; } = 1;
        public Color BorderColor { get; set; } = Color.Gainsboro;

        public RoundedPanel()
        {
            DoubleBuffered = true;
            Padding = new Padding(16);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var r = ClientRectangle;
            r.Width -= 1;
            r.Height -= 1;

            using (var gp = new GraphicsPath())
            {
                gp.AddArc(new Rectangle(r.Left, r.Top, Radius, Radius), 180, 90);
                gp.AddArc(new Rectangle(r.Right - Radius, r.Top, Radius, Radius), 270, 90);
                gp.AddArc(new Rectangle(r.Right - Radius, r.Bottom - Radius, Radius, Radius), 0, 90);
                gp.AddArc(new Rectangle(r.Left, r.Bottom - Radius, Radius, Radius), 90, 90);
                gp.CloseFigure();

                Region = new Region(gp);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                using (var pen = new Pen(BorderColor, BorderThickness))
                {
                    e.Graphics.DrawPath(pen, gp);
                }
            }
        }
    }
}
