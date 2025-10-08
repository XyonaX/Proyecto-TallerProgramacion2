using System;
using System.Drawing;
using System.Windows.Forms;

namespace Proyecto_Taller_2.UI.Helpers
{
    public class FrmInput : Form
    {
        private readonly TextBox _txt;
        private readonly Button _ok;
        private readonly Button _cancel;

        public string Value { get; private set; }

        public FrmInput(string title, string prompt, string defaultValue = "")
        {
            Text = title;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(380, 150);
            MaximizeBox = false;
            MinimizeBox = false;

            Label lbl = new Label
            {
                Text = prompt,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Top,
                Height = 40
            };
            Controls.Add(lbl);

            _txt = new TextBox
            {
                Text = defaultValue,
                Dock = DockStyle.Top,
                Margin = new Padding(10),
                Width = 340
            };
            Controls.Add(_txt);

            FlowLayoutPanel buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(10)
            };

            _ok = new Button { Text = "Aceptar", Width = 90 };
            _ok.Click += (s, e) =>
            {
                Value = _txt.Text;
                DialogResult = DialogResult.OK;
            };

            _cancel = new Button { Text = "Cancelar", Width = 90 };
            _cancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            buttons.Controls.Add(_ok);
            buttons.Controls.Add(_cancel);
            Controls.Add(buttons);
        }

        public static string Show(string title, string prompt, string defaultValue = "")
        {
            using (FrmInput f = new FrmInput(title, prompt, defaultValue))
            {
                return f.ShowDialog() == DialogResult.OK ? f.Value : null;
            }
        }
    }
}
