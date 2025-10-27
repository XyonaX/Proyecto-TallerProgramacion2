using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Proyecto_Taller_2.Domain.Models;
using Proyecto_Taller_2.Data.Repositories;

namespace Proyecto_Taller_2.Forms
{
    public class FrmAjusteModern : Form
    {
        private readonly InventarioItem _item;
        private readonly InventarioRepository _repo = new InventarioRepository();
        private NumericUpDown numCantidad;
        private TextBox txtObservacion;
        private Button btnOk, btnCancel;

        public FrmAjusteModern(InventarioItem item)
        {
            _item = item ?? throw new ArgumentNullException(nameof(item));
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = $"Ajuste Stock - {_item.NombreProducto}";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(420, 220);

            var tl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4, Padding = new Padding(12) };
            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tl.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            tl.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            tl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tl.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            tl.Controls.Add(new Label { Text = "Ajuste (+/-):", Anchor = AnchorStyles.Left | AnchorStyles.Bottom }, 0, 0);
            numCantidad = new NumericUpDown { Minimum = -100000, Maximum = 100000, Value = 1, Anchor = AnchorStyles.Left }; tl.Controls.Add(numCantidad, 1, 0);

            tl.Controls.Add(new Label { Text = "Descripción:", Anchor = AnchorStyles.Left | AnchorStyles.Bottom }, 0, 1);
            txtObservacion = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, Width = 220 }; tl.Controls.Add(txtObservacion, 1, 1);

            // Notas / info
            var lblInfo = new Label { Text = "Este ajuste quedará registrado en el historial de movimientos.", ForeColor = Color.Gray, AutoSize = true };
            tl.SetColumnSpan(lblInfo, 2); tl.Controls.Add(lblInfo, 0, 2);

            var fl = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            btnOk = new Button { Text = "Aceptar", Width = 90 }; btnCancel = new Button { Text = "Cancelar", Width = 90 }; fl.Controls.Add(btnOk); fl.Controls.Add(btnCancel);
            tl.SetColumnSpan(fl, 2); tl.Controls.Add(fl, 0, 3);

            this.Controls.Add(tl);

            btnOk.Click += BtnOk_Click;
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            int cantidad = (int)numCantidad.Value;
            string obs = txtObservacion.Text.Trim();
            if (cantidad == 0) { MessageBox.Show("Cantidad no puede ser cero.", "Validadción"); return; }
            if (string.IsNullOrWhiteSpace(obs)) { MessageBox.Show("Descripción obligatoria.", "Validadción"); return; }
            try
            {
                _repo.Movimiento(_item.IdProducto, 'A', cantidad, obs, "Ajuste Moderno", null);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar ajuste: {ex.Message}", "Error");
            }
        }
    }
}
