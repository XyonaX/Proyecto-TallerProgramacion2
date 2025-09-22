using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Proyecto_Taller_2.Forms
{
    public class NuevoUsuarioForm : Form
    {
        // ===== Resultado para el llamador =====
        public class UsuarioResultado
        {
            public int Dni;
            public string Nombre;
            public string Apellido;
            public string Email;
            public string Telefono;
            public string Rol;
            public bool Activo;
            public DateTime? FechaNacimiento;

            // ⚡️ Nuevo: también enviamos la contraseña en texto plano
            public string PasswordPlano;

            public string NombreCompleto { get { return (Nombre + " " + Apellido).Trim(); } }
        }
        public UsuarioResultado Resultado { get; private set; }

        // ===== UI =====
        TextBox txtDni, txtNombre, txtApellido, txtEmail, txtTelefono, txtPassword, txtConfirm;
        ComboBox cboRol;
        CheckBox chkActivo, chkFecha, chkMostrarPass;
        DateTimePicker dtpFecha;
        Button btnGuardar, btnCancelar;
        Label lblError;

        public NuevoUsuarioForm()
        {
            // ---------- Form ----------
            Text = "Nuevo Usuario";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            Font = new Font("Segoe UI", 10f);
            BackColor = Color.White;
            MinimumSize = new Size(640, 520);
            ClientSize = new Size(640, 520);

            // ---------- Root padding ----------
            var rootPad = new Panel { Dock = DockStyle.Fill, Padding = new Padding(18) };
            Controls.Add(rootPad);

            // ---------- Card ----------
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20),
            };
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 225, 230)))
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };
            rootPad.Controls.Add(card);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));  // título
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // formulario
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));  // botones
            card.Controls.Add(layout);

            // ---------- Título ----------
            var lblTitle = new Label
            {
                Text = "Completar datos del usuario",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(35, 45, 55),
                TextAlign = ContentAlignment.MiddleLeft
            };
            layout.Controls.Add(lblTitle, 0, 0);

            // ---------- Grid de campos ----------
            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 8,
                Padding = new Padding(4)
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 8; i++)
                grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

            // ⬆️ Ajusto la fila de contraseña para que no se corte
            grid.RowStyles[6] = new RowStyle(SizeType.Absolute, 68);

            layout.Controls.Add(grid, 0, 1);

            // Helpers
            Func<string, Label> L = (text) => new Label
            {
                Text = text,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Color.FromArgb(50, 60, 70),
                Padding = new Padding(0, 0, 8, 0)
            };
            Func<bool, TextBox> T = (pwd) => new TextBox
            {
                Dock = DockStyle.Fill,
                UseSystemPasswordChar = pwd,
                Margin = new Padding(0, 6, 0, 6),
                MinimumSize = new Size(0, 32)
            };

            // DNI
            txtDni = T(false);
            grid.Controls.Add(L("DNI *"), 0, 0);
            grid.Controls.Add(txtDni, 1, 0);

            // Nombre
            txtNombre = T(false);
            grid.Controls.Add(L("Nombre *"), 0, 1);
            grid.Controls.Add(txtNombre, 1, 1);

            // Apellido
            txtApellido = T(false);
            grid.Controls.Add(L("Apellido *"), 0, 2);
            grid.Controls.Add(txtApellido, 1, 2);

            // Email
            txtEmail = T(false);
            grid.Controls.Add(L("Email *"), 0, 3);
            grid.Controls.Add(txtEmail, 1, 3);

            // Teléfono
            txtTelefono = T(false);
            grid.Controls.Add(L("Teléfono"), 0, 4);
            grid.Controls.Add(txtTelefono, 1, 4);

            // Rol
            cboRol = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 6, 0, 6)
            };
            cboRol.Items.AddRange(new object[] { "Administrador", "Operador", "Propietario" });
            if (cboRol.Items.Count > 0) cboRol.SelectedIndex = 1;

            grid.Controls.Add(L("Rol *"), 0, 5);
            grid.Controls.Add(cboRol, 1, 5);

            // Contraseña + Confirmación + Mostrar
            var passRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, Padding = new Padding(0) };
            passRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            passRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            passRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            txtPassword = T(true);
            txtConfirm = T(true);

            passRow.Controls.Add(Wrap("Contraseña *", txtPassword), 0, 0);
            passRow.Controls.Add(Wrap("Repetir *", txtConfirm), 1, 0);

            chkMostrarPass = new CheckBox
            {
                Text = "Mostrar",
                AutoSize = true,
                Margin = new Padding(12, 26, 0, 0)
            };
            chkMostrarPass.CheckedChanged += (s, e) =>
            {
                bool show = chkMostrarPass.Checked;
                txtPassword.UseSystemPasswordChar = !show;
                txtConfirm.UseSystemPasswordChar = !show;
            };
            passRow.Controls.Add(chkMostrarPass, 2, 0);

            grid.Controls.Add(new Label(), 0, 6);
            grid.Controls.Add(passRow, 1, 6);

            // Estado + Fecha nacimiento
            var rowEstado = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0, 6, 0, 6)
            };
            chkActivo = new CheckBox { Text = "Activo", Checked = true, AutoSize = true, Margin = new Padding(0, 6, 24, 0) };
            chkFecha = new CheckBox { Text = "Usar fecha nacimiento", AutoSize = true, Margin = new Padding(0, 6, 8, 0) };
            dtpFecha = new DateTimePicker { Format = DateTimePickerFormat.Short, Enabled = false, Width = 140, Margin = new Padding(0, 3, 0, 0) };
            chkFecha.CheckedChanged += (s, e) => dtpFecha.Enabled = chkFecha.Checked;

            rowEstado.Controls.Add(chkActivo);
            rowEstado.Controls.Add(chkFecha);
            rowEstado.Controls.Add(dtpFecha);
            grid.Controls.Add(L("Estado"), 0, 7);
            grid.Controls.Add(rowEstado, 1, 7);

            // Error label
            lblError = new Label
            {
                ForeColor = Color.Firebrick,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0)
            };
            layout.Controls.Add(lblError, 0, 2);

            // Botonera inferior
            var btnBar = new Panel { Dock = DockStyle.Bottom, Height = 56 };
            card.Controls.Add(btnBar);
            btnBar.BringToFront();

            var flBtns = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 10, 0, 0)
            };
            btnBar.Controls.Add(flBtns);

            btnCancelar = new Button
            {
                Text = "Cancelar",
                AutoSize = true,
                DialogResult = DialogResult.Cancel,
                Margin = new Padding(0, 0, 10, 0)
            };
            btnGuardar = new Button
            {
                Text = "Guardar",
                AutoSize = true
            };
            flBtns.Controls.Add(btnCancelar);
            flBtns.Controls.Add(btnGuardar);

            AcceptButton = btnGuardar;
            CancelButton = btnCancelar;

            btnGuardar.Click += (s, e) => Guardar();
        }

        // Helpers
        Control Wrap(string label, Control c)
        {
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
            p.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
            p.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            p.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(0, 0, 0, 2), ForeColor = Color.FromArgb(50, 60, 70) }, 0, 0);
            p.Controls.Add(c, 0, 1);
            return p;
        }

        // Guardar
        void Guardar()
        {
            lblError.Text = "";

            int dni;
            if (!int.TryParse(txtDni.Text.Trim(), out dni) || dni <= 0) { Fail("DNI inválido."); return; }
            if (string.IsNullOrWhiteSpace(txtNombre.Text)) { Fail("Nombre es requerido."); return; }
            if (string.IsNullOrWhiteSpace(txtApellido.Text)) { Fail("Apellido es requerido."); return; }
            if (!EmailOk(txtEmail.Text.Trim())) { Fail("Email inválido."); return; }
            if (cboRol.SelectedIndex < 0) { Fail("Seleccioná un Rol."); return; }
            if (string.IsNullOrWhiteSpace(txtPassword.Text) || txtPassword.Text.Length < 4) { Fail("La contraseña debe tener al menos 4 caracteres."); return; }
            if (txtPassword.Text != txtConfirm.Text) { Fail("Las contraseñas no coinciden."); return; }

            Resultado = new UsuarioResultado
            {
                Dni = dni,
                Nombre = txtNombre.Text.Trim(),
                Apellido = txtApellido.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Telefono = txtTelefono.Text.Trim(),
                Rol = cboRol.SelectedItem.ToString(),
                Activo = chkActivo.Checked,
                FechaNacimiento = chkFecha.Checked ? (DateTime?)dtpFecha.Value.Date : null,

                // ⚡️ Contraseña lista para enviar al repo
                PasswordPlano = txtPassword.Text
            };

            DialogResult = DialogResult.OK;
            Close();
        }

        void Fail(string msg)
        {
            lblError.Text = msg;
            System.Media.SystemSounds.Beep.Play();
        }

        bool EmailOk(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
    }
}
