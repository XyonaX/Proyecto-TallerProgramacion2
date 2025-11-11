using Proyecto_Taller_2.Data.Repositories;
using Proyecto_Taller_2.Domain.Entities;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Proyecto_Taller_2
{
    public partial class FormEditarCliente : Form
    {
        private readonly ClienteRepository _clienteRepository;
        private readonly Cliente _clienteAEditar;

        // --- Controles (Asegúrate que coincidan con tu .Designer.cs) ---
        private TextBox txtNombre;
        private TextBox txtApellido;
        private TextBox txtRazonSocial;
        private TextBox txtDireccion;
        private TextBox txtCUIL;
        private TextBox txtCUIT;
        private TextBox txtEmail;
        private TextBox txtTelefono;
        private Button btnGuardar;
        private Button btnCancelar;
        // ... Labels ...
        private Label label1; private Label label2; private Label label3; private Label label4;
        private Label label5; private Label label6; private Label label7; private Label label8;


        public FormEditarCliente(Cliente cliente)
        {
            InitializeComponent();
            _clienteAEditar = cliente;
            _clienteRepository = new ClienteRepository();
            CargarDatos();
        }

        private void CargarDatos()
        {
            txtNombre.Text = _clienteAEditar.NombreCliente;
            txtApellido.Text = _clienteAEditar.ApellidoCliente;
            txtRazonSocial.Text = _clienteAEditar.RazonSocial;
            txtDireccion.Text = _clienteAEditar.Direccion;
            txtCUIL.Text = _clienteAEditar.CUIL;
            txtCUIT.Text = _clienteAEditar.CUIT;

            // Cargar email y teléfono principal ACTUALES
            txtEmail.Text = _clienteAEditar.Emails.FirstOrDefault(e => e.EsPrincipal)?.Email ?? _clienteAEditar.Emails.FirstOrDefault()?.Email ?? "";
            txtTelefono.Text = _clienteAEditar.Telefonos.FirstOrDefault(t => t.EsPrincipal)?.Telefono ?? _clienteAEditar.Telefonos.FirstOrDefault()?.Telefono ?? "";

            // Habilitar/deshabilitar según tipo (si es necesario)
            bool esPF = _clienteAEditar.Tipo == "PF";
            txtNombre.Enabled = esPF;
            txtApellido.Enabled = esPF;
            txtCUIL.Enabled = esPF;
            txtRazonSocial.Enabled = !esPF;
            txtCUIT.Enabled = !esPF;
        }

        private async void btnGuardar_Click(object sender, EventArgs e)
        {
            // Detener cierre automático
            this.DialogResult = DialogResult.None;

            // 1. Recoger datos
            string nuevoEmail = txtEmail.Text.Trim();
            string nuevoTelefono = txtTelefono.Text.Trim();
            string nuevoCUIL = txtCUIL.Text.Trim();
            string nuevoCUIT = txtCUIT.Text.Trim();

            // 2. Validación
            try
            {
                // Validar DNI/CUIL/CUIT
                if (!string.IsNullOrWhiteSpace(nuevoCUIL) || !string.IsNullOrWhiteSpace(nuevoCUIT))
                {
                    if (await _clienteRepository.CheckDniExistsAsync(nuevoCUIL, nuevoCUIT, _clienteAEditar.IdCliente))
                    { MessageBox.Show("El CUIL o CUIT ya pertenece a otro cliente.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
                }
                // Validar Email
                if (!string.IsNullOrWhiteSpace(nuevoEmail))
                {
                    if (await _clienteRepository.CheckEmailExistsAsync(nuevoEmail, _clienteAEditar.IdCliente))
                    { MessageBox.Show("El Email ya pertenece a otro cliente.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
                }
                // Podrías añadir validación de formato para email/teléfono/CUIT/CUIL aquí

                // 3. Actualizar objeto Cliente (solo tabla principal)
                _clienteAEditar.NombreCliente = txtNombre.Text.Trim();
                _clienteAEditar.ApellidoCliente = txtApellido.Text.Trim();
                _clienteAEditar.RazonSocial = txtRazonSocial.Text.Trim();
                _clienteAEditar.Direccion = txtDireccion.Text.Trim();
                _clienteAEditar.CUIL = nuevoCUIL;
                _clienteAEditar.CUIT = nuevoCUIT;

                // 4. Guardar en BD (pasando email y teléfono)
                await _clienteRepository.UpdateClienteAsync(_clienteAEditar, nuevoEmail, nuevoTelefono);

                MessageBox.Show("Cliente actualizado con éxito.", "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK; // Establecer OK y cerrar
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // No cerramos si hay error
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        #region Código del Diseñador (Designer.cs)
        // --- COPIA AQUÍ EL CÓDIGO DE TU ARCHIVO FormEditarCliente.Designer.cs ---
        // --- Asegúrate de que los nombres de los controles coincidan con los usados arriba ---

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtNombre = new System.Windows.Forms.TextBox();
            this.txtApellido = new System.Windows.Forms.TextBox();
            this.txtRazonSocial = new System.Windows.Forms.TextBox();
            this.txtDireccion = new System.Windows.Forms.TextBox();
            this.txtCUIL = new System.Windows.Forms.TextBox();
            this.txtCUIT = new System.Windows.Forms.TextBox();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.txtTelefono = new System.Windows.Forms.TextBox();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // txtNombre ... txtTelefono, btnGuardar, btnCancelar, labels
            // (Este código es generado por el diseñador visual, ajústalo según tu diseño)
            // Ejemplo básico:
            this.txtNombre.Location = new System.Drawing.Point(111, 28); this.txtNombre.Name = "txtNombre"; this.txtNombre.Size = new System.Drawing.Size(237, 20); this.txtNombre.TabIndex = 0;
            this.label1.AutoSize = true; this.label1.Location = new System.Drawing.Point(31, 31); this.label1.Name = "label1"; this.label1.Size = new System.Drawing.Size(47, 13); this.label1.TabIndex = 10; this.label1.Text = "Nombre:";
            // ... Repetir para todos los controles ...
            this.txtApellido.Location = new System.Drawing.Point(111, 54); this.txtApellido.Name = "txtApellido"; this.txtApellido.Size = new System.Drawing.Size(237, 20); this.txtApellido.TabIndex = 1;
            this.label2.AutoSize = true; this.label2.Location = new System.Drawing.Point(31, 57); this.label2.Name = "label2"; this.label2.Size = new System.Drawing.Size(47, 13); this.label2.TabIndex = 11; this.label2.Text = "Apellido:";
            this.txtRazonSocial.Location = new System.Drawing.Point(111, 80); this.txtRazonSocial.Name = "txtRazonSocial"; this.txtRazonSocial.Size = new System.Drawing.Size(237, 20); this.txtRazonSocial.TabIndex = 2;
            this.label3.AutoSize = true; this.label3.Location = new System.Drawing.Point(31, 83); this.label3.Name = "label3"; this.label3.Size = new System.Drawing.Size(73, 13); this.label3.TabIndex = 12; this.label3.Text = "Razón Social:";
            this.txtDireccion.Location = new System.Drawing.Point(111, 106); this.txtDireccion.Name = "txtDireccion"; this.txtDireccion.Size = new System.Drawing.Size(237, 20); this.txtDireccion.TabIndex = 3;
            this.label4.AutoSize = true; this.label4.Location = new System.Drawing.Point(31, 109); this.label4.Name = "label4"; this.label4.Size = new System.Drawing.Size(55, 13); this.label4.TabIndex = 13; this.label4.Text = "Dirección:";
            this.txtCUIL.Location = new System.Drawing.Point(111, 132); this.txtCUIL.Name = "txtCUIL"; this.txtCUIL.Size = new System.Drawing.Size(237, 20); this.txtCUIL.TabIndex = 4;
            this.label5.AutoSize = true; this.label5.Location = new System.Drawing.Point(31, 135); this.label5.Name = "label5"; this.label5.Size = new System.Drawing.Size(34, 13); this.label5.TabIndex = 14; this.label5.Text = "CUIL:";
            this.txtCUIT.Location = new System.Drawing.Point(111, 158); this.txtCUIT.Name = "txtCUIT"; this.txtCUIT.Size = new System.Drawing.Size(237, 20); this.txtCUIT.TabIndex = 5;
            this.label6.AutoSize = true; this.label6.Location = new System.Drawing.Point(31, 161); this.label6.Name = "label6"; this.label6.Size = new System.Drawing.Size(35, 13); this.label6.TabIndex = 15; this.label6.Text = "CUIT:";
            this.txtEmail.Location = new System.Drawing.Point(111, 184); this.txtEmail.Name = "txtEmail"; this.txtEmail.Size = new System.Drawing.Size(237, 20); this.txtEmail.TabIndex = 6;
            this.label7.AutoSize = true; this.label7.Location = new System.Drawing.Point(31, 187); this.label7.Name = "label7"; this.label7.Size = new System.Drawing.Size(35, 13); this.label7.TabIndex = 16; this.label7.Text = "Email:";
            this.txtTelefono.Location = new System.Drawing.Point(111, 210); this.txtTelefono.Name = "txtTelefono"; this.txtTelefono.Size = new System.Drawing.Size(237, 20); this.txtTelefono.TabIndex = 7;
            this.label8.AutoSize = true; this.label8.Location = new System.Drawing.Point(31, 213); this.label8.Name = "label8"; this.label8.Size = new System.Drawing.Size(52, 13); this.label8.TabIndex = 17; this.label8.Text = "Teléfono:";
            this.btnGuardar.Location = new System.Drawing.Point(192, 252); this.btnGuardar.Name = "btnGuardar"; this.btnGuardar.Size = new System.Drawing.Size(75, 23); this.btnGuardar.TabIndex = 8; this.btnGuardar.Text = "Guardar"; this.btnGuardar.UseVisualStyleBackColor = true; this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            this.btnCancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel; this.btnCancelar.Location = new System.Drawing.Point(273, 252); this.btnCancelar.Name = "btnCancelar"; this.btnCancelar.Size = new System.Drawing.Size(75, 23); this.btnCancelar.TabIndex = 9; this.btnCancelar.Text = "Cancelar"; this.btnCancelar.UseVisualStyleBackColor = true; this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            //
            // FormEditarCliente
            //
            this.AcceptButton = this.btnGuardar; this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F); this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font; this.CancelButton = this.btnCancelar; this.ClientSize = new System.Drawing.Size(384, 301);
            this.Controls.Add(this.label8); this.Controls.Add(this.label7); this.Controls.Add(this.label6); this.Controls.Add(this.label5); this.Controls.Add(this.label4); this.Controls.Add(this.label3); this.Controls.Add(this.label2); this.Controls.Add(this.label1); this.Controls.Add(this.btnCancelar); this.Controls.Add(this.btnGuardar); this.Controls.Add(this.txtTelefono); this.Controls.Add(this.txtEmail); this.Controls.Add(this.txtCUIT); this.Controls.Add(this.txtCUIL); this.Controls.Add(this.txtDireccion); this.Controls.Add(this.txtRazonSocial); this.Controls.Add(this.txtApellido); this.Controls.Add(this.txtNombre);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog; this.MaximizeBox = false; this.MinimizeBox = false; this.Name = "FormEditarCliente"; this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent; this.Text = "Editar Cliente";
            this.ResumeLayout(false); this.PerformLayout();
        }
        #endregion
    }
}