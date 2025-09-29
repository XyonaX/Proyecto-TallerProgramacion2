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
            Width = 420; Height = 430;

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), RowCount = 2 };
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            Controls.Add(root);

            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoSize = true };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            int r = 0;
            txtDni = AddText(grid, "DNI", ref r);
            txtNombre = AddText(grid, "Nombre", ref r);
            txtApellido = AddText(grid, "Apellido", ref r);
            txtEmail = AddText(grid, "Email", ref r);
            txtTelefono = AddText(grid, "Teléfono", ref r);
            txtPassword = AddText(grid, "Contraseña", ref r);
            txtPassword.UseSystemPasswordChar = true;

            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            grid.Controls.Add(new Label { Text = "Activo", Anchor = AnchorStyles.Left, AutoSize = true }, 0, r);
            chkActivo = new CheckBox { Text = "Usuario activo", Dock = DockStyle.Left };
            grid.Controls.Add(chkActivo, 1, r);
            r++;

            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            grid.Controls.Add(new Label { Text = "Rol", Anchor = AnchorStyles.Left, AutoSize = true }, 0, r);
            cmbRol = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
            grid.Controls.Add(cmbRol, 1, r);
            r++;

            root.Controls.Add(grid, 0, 0);

            var panelBtns = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill };
            var btnOk = new Button { Text = "Guardar", DialogResult = DialogResult.OK, AutoSize = true };
            var btnCancel = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, AutoSize = true };
            panelBtns.Controls.Add(btnOk);
            panelBtns.Controls.Add(btnCancel);
            root.Controls.Add(panelBtns, 0, 1);

            this.AcceptButton = btnOk;

            Load += delegate
            {
                var roles = _repo.ObtenerRoles(); // List<(int, string)>
                cmbRol.DataSource = roles.Select(rp => new { Id = rp.Item1, Nombre = rp.Item2 }).ToList();
                cmbRol.DisplayMember = "Nombre";
                cmbRol.ValueMember = "Id";
                if (roles.Count > 0) cmbRol.SelectedValue = _original.IdRol;

                txtDni.Text = _original.Dni.ToString();
                txtNombre.Text = _original.Nombre ?? "";
                txtApellido.Text = _original.Apellido ?? "";
                txtEmail.Text = _original.Email ?? "";
                txtTelefono.Text = _original.Telefono ?? "";
                txtPassword.Text = ""; // vacío => no cambiar contraseña
                chkActivo.Checked = _original.Activo;
            };

            btnOk.Click += delegate
            {
                if (!Validar()) { DialogResult = DialogResult.None; return; }

                var u = new Usuario
                {
                    IdUsuario = _original.IdUsuario, // mantener PK para UPDATE
                    Dni = int.Parse(txtDni.Text.Trim()),
                    Nombre = txtNombre.Text.Trim(),
                    Apellido = txtApellido.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Telefono = string.IsNullOrWhiteSpace(txtTelefono.Text) ? null : txtTelefono.Text.Trim(),
                    Password = string.IsNullOrWhiteSpace(txtPassword.Text) ? null : txtPassword.Text,
                    Activo = chkActivo.Checked,
                    IdRol = (int)cmbRol.SelectedValue
                };

                Resultado = u;
            };
        }

        private TextBox AddText(TableLayoutPanel grid, string label, ref int row)
        {
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            grid.Controls.Add(new Label { Text = label, Anchor = AnchorStyles.Left, AutoSize = true }, 0, row);
            var tb = new TextBox { Dock = DockStyle.Fill };
            grid.Controls.Add(tb, 1, row);
            row++;
            return tb;
        }

        private bool Validar()
        {
            if (!int.TryParse(txtDni.Text.Trim(), out var dni)) { MessageBox.Show("DNI inválido"); return false; }
            if (string.IsNullOrWhiteSpace(txtNombre.Text)) { MessageBox.Show("Nombre requerido"); return false; }
            if (string.IsNullOrWhiteSpace(txtApellido.Text)) { MessageBox.Show("Apellido requerido"); return false; }
            if (string.IsNullOrWhiteSpace(txtEmail.Text)) { MessageBox.Show("Email requerido"); return false; }
            if (cmbRol.SelectedValue == null) { MessageBox.Show("Rol requerido"); return false; }

            string email = txtEmail.Text.Trim().ToLowerInvariant();

            if (_usuarios.Exists(x => x.Dni == dni && x.IdUsuario != _original.IdUsuario))
            {
                MessageBox.Show("Ya existe un usuario con ese DNI.");
                return false;
            }
            if (_usuarios.Exists(x => (x.Email ?? "").ToLowerInvariant() == email && x.IdUsuario != _original.IdUsuario))
            {
                MessageBox.Show("Ya existe un usuario con ese email.");
                return false;
            }
            return true;
        }
    }
}
