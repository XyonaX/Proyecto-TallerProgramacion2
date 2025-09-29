using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Proyecto_Taller_2.Domain.Models;
using Proyecto_Taller_2.Data.Repositories;

namespace Proyecto_Taller_2
{
    public class EditarUsuarioForm : Form
    {
        public Usuario Resultado { get; private set; }
        private readonly UsuarioRepository _repo;
        private readonly Usuario _original;
        private readonly List<Usuario> _usuarios;
        private TextBox txtDni, txtNombre, txtApellido, txtEmail, txtTelefono, txtPassword;
        private DateTimePicker dtpNac;
        private CheckBox chkActivo;
        private ComboBox cmbRol;

        public EditarUsuarioForm(UsuarioRepository repo, Usuario usuario, List<Usuario> usuarios)
        {
            _repo = repo;
            _original = usuario;
            _usuarios = usuarios;
            Text = "Editar Usuario";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            Width = 420; Height = 460;

            TableLayoutPanel root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), RowCount = 2 };
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            Controls.Add(root);

            TableLayoutPanel grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoSize = true };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            txtDni = AddText(grid, "DNI");
            txtNombre = AddText(grid, "Nombre");
            txtApellido = AddText(grid, "Apellido");
            txtEmail = AddText(grid, "Email");
            txtTelefono = AddText(grid, "Teléfono");
            txtPassword = AddText(grid, "Contraseña"); txtPassword.UseSystemPasswordChar = true;

            grid.Controls.Add(new Label { Text = "Fecha Nac.", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 6);
            dtpNac = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Dock = DockStyle.Fill };
            grid.Controls.Add(dtpNac, 1, 6);

            grid.Controls.Add(new Label { Text = "Estado", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 7);
            chkActivo = new CheckBox { Text = "Activo", Dock = DockStyle.Left };
            grid.Controls.Add(chkActivo, 1, 7);

            grid.Controls.Add(new Label { Text = "Rol", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 8);
            cmbRol = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
            grid.Controls.Add(cmbRol, 1, 8);

            root.Controls.Add(grid, 0, 0);

            FlowLayoutPanel panelBtns = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill };
            Button btnOk = new Button { Text = "Guardar", DialogResult = DialogResult.OK, AutoSize = true };
            Button btnCancel = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, AutoSize = true };
            panelBtns.Controls.Add(btnOk);
            panelBtns.Controls.Add(btnCancel);
            root.Controls.Add(panelBtns, 0, 1);

            Load += delegate
            {
                List<ValueTuple<int, string>> roles = _repo.ObtenerRoles();
                cmbRol.DataSource = roles.Select(r => new { Id = r.Item1, Nombre = r.Item2 }).ToList();
                cmbRol.DisplayMember = "Nombre";
                cmbRol.ValueMember = "Id";
                if (roles.Count > 0) cmbRol.SelectedValue = usuario.IdRol;

                txtDni.Text = usuario.Dni.ToString();
                txtNombre.Text = usuario.Nombre;
                txtApellido.Text = usuario.Apellido;
                txtEmail.Text = usuario.Email;
                txtTelefono.Text = usuario.Telefono;
                txtPassword.Text = usuario.Password;
                dtpNac.Value = usuario.FechaNacimiento ?? DateTime.Now;
                dtpNac.Checked = usuario.FechaNacimiento.HasValue;
                chkActivo.Checked = usuario.Estado;
            };

            btnOk.Click += delegate
            {
                if (!Validar()) { DialogResult = DialogResult.None; return; }
                Usuario u = new Usuario
                {
                    Dni = int.Parse(txtDni.Text.Trim()),
                    Nombre = txtNombre.Text.Trim(),
                    Apellido = txtApellido.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Telefono = string.IsNullOrWhiteSpace(txtTelefono.Text) ? null : txtTelefono.Text.Trim(),
                    Password = txtPassword.Text, // puede ir vacío
                    FechaNacimiento = dtpNac.Checked ? (DateTime?)dtpNac.Value.Date : null,
                    Estado = chkActivo.Checked,
                    IdRol = (int)cmbRol.SelectedValue,
                    RolNombre = (cmbRol.SelectedItem as dynamic).Nombre
                };
                Resultado = u;
            };
        }

        private TextBox AddText(TableLayoutPanel grid, string label)
        {
            int r = grid.RowCount++;
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            grid.Controls.Add(new Label { Text = label, Anchor = AnchorStyles.Left, AutoSize = true }, 0, r);
            TextBox tb = new TextBox { Dock = DockStyle.Fill };
            grid.Controls.Add(tb, 1, r);
            return tb;
        }

        private bool Validar()
        {
            int tmp;
            if (!int.TryParse(txtDni.Text.Trim(), out tmp)) { MessageBox.Show("DNI inválido"); return false; }
            if (string.IsNullOrWhiteSpace(txtNombre.Text)) { MessageBox.Show("Nombre requerido"); return false; }
            if (string.IsNullOrWhiteSpace(txtApellido.Text)) { MessageBox.Show("Apellido requerido"); return false; }
            if (string.IsNullOrWhiteSpace(txtEmail.Text)) { MessageBox.Show("Email requerido"); return false; }
            if (cmbRol.SelectedValue == null) { MessageBox.Show("Rol requerido"); return false; }
            // Unicidad DNI y Email (excepto el usuario actual)
            int dni = int.Parse(txtDni.Text.Trim());
            string email = txtEmail.Text.Trim().ToLowerInvariant();
            if (_usuarios.Any(x => x.Dni == dni && x.Dni != _original.Dni))
            {
                MessageBox.Show("Ya existe un usuario con ese DNI.");
                return false;
            }
            if (_usuarios.Any(x => x.Email.ToLowerInvariant() == email && x.Dni != _original.Dni))
            {
                MessageBox.Show("Ya existe un usuario con ese email.");
                return false;
            }
            return true;
        }
    }
}
