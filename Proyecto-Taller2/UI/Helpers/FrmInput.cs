using System;
using System.Drawing;
using System.Windows.Forms;

namespace Proyecto_Taller_2.UI.Helpers
{
    public static class FrmInput
    {
        public static string Show(string title, string prompt, string defaultValue = "")
        {
            using (var form = new Form())
            {
                form.Text = title;
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.Size = new Size(400, 150);
                
                var label = new Label()
                {
                    Text = prompt,
                    Location = new Point(12, 12),
                    Size = new Size(360, 20)
                };
                
                var textBox = new TextBox()
                {
                    Text = defaultValue,
                    Location = new Point(12, 35),
                    Size = new Size(360, 20)
                };
                
                var buttonOk = new Button()
                {
                    Text = "OK",
                    Location = new Point(220, 70),
                    Size = new Size(75, 23),
                    DialogResult = DialogResult.OK
                };
                
                var buttonCancel = new Button()
                {
                    Text = "Cancelar",
                    Location = new Point(305, 70),
                    Size = new Size(75, 23),
                    DialogResult = DialogResult.Cancel
                };
                
                form.Controls.Add(label);
                form.Controls.Add(textBox);
                form.Controls.Add(buttonOk);
                form.Controls.Add(buttonCancel);
                
                form.AcceptButton = buttonOk;
                form.CancelButton = buttonCancel;
                
                textBox.SelectAll();
                textBox.Focus();
                
                var result = form.ShowDialog();
                
                return result == DialogResult.OK ? textBox.Text : null;
            }
        }
    }
}