using System.Drawing;
using System.Windows.Forms;

namespace Proyecto_Taller_2
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private Panel pnlSidebar;
        private Button btnVentas;
        private Button btnInventario;
        private Button btnUsuarios;
        private Button btnConfiguracion;

        private Panel pnlTop;
        private Label lblTitulo;

        private Panel pnlContent;

        protected override void Dispose(bool disposing)
        { if (disposing && (components != null)) components.Dispose(); base.Dispose(disposing); }

        #region Código generado por el Diseñador de Windows Forms
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.pnlSidebar = new System.Windows.Forms.Panel();
            this.btnConfiguracion = new System.Windows.Forms.Button();
            this.btnUsuarios = new System.Windows.Forms.Button();
            this.btnInventario = new System.Windows.Forms.Button();
            this.btnVentas = new System.Windows.Forms.Button();

            this.pnlTop = new System.Windows.Forms.Panel();
            this.lblTitulo = new System.Windows.Forms.Label();

            this.pnlContent = new System.Windows.Forms.Panel();

            this.pnlSidebar.SuspendLayout();
            this.pnlTop.SuspendLayout();
            this.SuspendLayout();

            // pnlSidebar
            this.pnlSidebar.BackColor = Color.FromArgb(236, 243, 236);
            this.pnlSidebar.Controls.Add(this.btnConfiguracion);
            this.pnlSidebar.Controls.Add(this.btnUsuarios);
            this.pnlSidebar.Controls.Add(this.btnInventario);
            this.pnlSidebar.Controls.Add(this.btnVentas);
            this.pnlSidebar.Dock = DockStyle.Left;
            this.pnlSidebar.Location = new System.Drawing.Point(0, 0);
            this.pnlSidebar.Name = "pnlSidebar";
            this.pnlSidebar.Size = new System.Drawing.Size(200, 700);
            this.pnlSidebar.TabIndex = 2;

            // btnConfiguracion
            this.btnConfiguracion.BackColor = Color.FromArgb(236, 243, 236);
            this.btnConfiguracion.Dock = DockStyle.Top;
            this.btnConfiguracion.FlatAppearance.BorderSize = 0;
            this.btnConfiguracion.FlatStyle = FlatStyle.Flat;
            this.btnConfiguracion.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnConfiguracion.ForeColor = Color.FromArgb(34, 47, 34);
            this.btnConfiguracion.Location = new System.Drawing.Point(0, 132);
            this.btnConfiguracion.Name = "btnConfiguracion";
            this.btnConfiguracion.Padding = new Padding(16, 0, 0, 0);
            this.btnConfiguracion.Size = new System.Drawing.Size(200, 44);
            this.btnConfiguracion.TabIndex = 0;
            this.btnConfiguracion.Text = "Configuración";
            this.btnConfiguracion.TextAlign = ContentAlignment.MiddleLeft;
            this.btnConfiguracion.UseVisualStyleBackColor = false;

            // btnUsuarios
            this.btnUsuarios.BackColor = Color.FromArgb(236, 243, 236);
            this.btnUsuarios.Dock = DockStyle.Top;
            this.btnUsuarios.FlatAppearance.BorderSize = 0;
            this.btnUsuarios.FlatStyle = FlatStyle.Flat;
            this.btnUsuarios.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnUsuarios.ForeColor = Color.FromArgb(34, 47, 34);
            this.btnUsuarios.Location = new System.Drawing.Point(0, 88);
            this.btnUsuarios.Name = "btnUsuarios";
            this.btnUsuarios.Padding = new Padding(16, 0, 0, 0);
            this.btnUsuarios.Size = new System.Drawing.Size(200, 44);
            this.btnUsuarios.TabIndex = 1;
            this.btnUsuarios.Text = "Usuarios";
            this.btnUsuarios.TextAlign = ContentAlignment.MiddleLeft;
            this.btnUsuarios.UseVisualStyleBackColor = false;

            // btnInventario
            this.btnInventario.BackColor = Color.FromArgb(236, 243, 236);
            this.btnInventario.Dock = DockStyle.Top;
            this.btnInventario.FlatAppearance.BorderSize = 0;
            this.btnInventario.FlatStyle = FlatStyle.Flat;
            this.btnInventario.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnInventario.ForeColor = Color.FromArgb(34, 47, 34);
            this.btnInventario.Location = new System.Drawing.Point(0, 44);
            this.btnInventario.Name = "btnInventario";
            this.btnInventario.Padding = new Padding(16, 0, 0, 0);
            this.btnInventario.Size = new System.Drawing.Size(200, 44);
            this.btnInventario.TabIndex = 2;
            this.btnInventario.Text = "Inventario";
            this.btnInventario.TextAlign = ContentAlignment.MiddleLeft;
            this.btnInventario.UseVisualStyleBackColor = false;

            // btnVentas
            this.btnVentas.BackColor = Color.FromArgb(236, 243, 236);
            this.btnVentas.Dock = DockStyle.Top;
            this.btnVentas.FlatAppearance.BorderSize = 0;
            this.btnVentas.FlatStyle = FlatStyle.Flat;
            this.btnVentas.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnVentas.ForeColor = Color.FromArgb(34, 47, 34);
            this.btnVentas.Location = new System.Drawing.Point(0, 0);
            this.btnVentas.Name = "btnVentas";
            this.btnVentas.Padding = new Padding(16, 0, 0, 0);
            this.btnVentas.Size = new System.Drawing.Size(200, 44);
            this.btnVentas.TabIndex = 3;
            this.btnVentas.Text = "Ventas";
            this.btnVentas.TextAlign = ContentAlignment.MiddleLeft;
            this.btnVentas.UseVisualStyleBackColor = false;
            // importante: NO enganches handler acá si ya lo hacés en Form1.cs

            // pnlTop
            this.pnlTop.BackColor = Color.White;
            this.pnlTop.Controls.Add(this.lblTitulo);
            this.pnlTop.Dock = DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(200, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(900, 56);
            this.pnlTop.TabIndex = 1;

            // lblTitulo
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Dock = DockStyle.Left;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = Color.FromArgb(34, 47, 34);
            this.lblTitulo.Location = new System.Drawing.Point(0, 0);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Padding = new Padding(12, 0, 0, 0);
            this.lblTitulo.Size = new System.Drawing.Size(165, 32);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Sistema ERP";

            // pnlContent
            this.pnlContent.BackColor = Color.White;
            this.pnlContent.Dock = DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(200, 56);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Size = new System.Drawing.Size(900, 644);
            this.pnlContent.TabIndex = 0;

            // Form1
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.White;
            this.ClientSize = new System.Drawing.Size(1100, 700);
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.pnlSidebar);
            this.MinimumSize = new System.Drawing.Size(900, 600);
            this.Name = "Form1";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Sistema ERP";

            this.pnlSidebar.ResumeLayout(false);
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.ResumeLayout(false);
        }
        #endregion
    }
}
