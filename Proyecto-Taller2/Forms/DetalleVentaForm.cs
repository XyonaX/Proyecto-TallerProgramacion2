using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Proyecto_Taller_2.Data.Repositories;
using Proyecto_Taller_2.Domain.Models;

namespace Proyecto_Taller_2.Forms
{
    public partial class DetalleVentaForm : Form
    {
        private readonly Venta _venta;
        private readonly VentaRepository _ventaRepo;
        private bool _modificado = false;

        // Controles
        private TableLayoutPanel tlRoot;
        private GroupBox gbInfo, gbDetalles;
        private Label lblNumero, lblCliente, lblVendedor, lblFecha, lblTotal;
        private ComboBox cbEstado;
        private TextBox txtObservaciones;
        private DataGridView dgvDetalles;
        private Button btnGuardar, btnCerrar, btnImprimir;

        public DetalleVentaForm(Venta venta, VentaRepository ventaRepo)
        {
            _venta = venta;
            _ventaRepo = ventaRepo;
            
            InitializeComponent();
            CargarDatos();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form
            this.Text = $"Detalle de {_venta.Tipo} - {_venta.NumeroVenta}";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(700, 500);

            // Root Layout
            tlRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(16)
            };
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 200)); // Info general
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Detalles
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // Botones

            // === INFORMACIÓN GENERAL ===
            gbInfo = new GroupBox
            {
                Text = "Información General",
                Dock = DockStyle.Fill,
                Padding = new Padding(12)
            };

            var tlInfo = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 4
            };

            // Configurar columnas
            for (int i = 0; i < 4; i++)
                tlInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            // Fila 1
            tlInfo.Controls.Add(new Label { Text = "Número:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Anchor = AnchorStyles.Left }, 0, 0);
            lblNumero = new Label { Text = _venta.NumeroVenta, Anchor = AnchorStyles.Left };
            tlInfo.Controls.Add(lblNumero, 1, 0);

            tlInfo.Controls.Add(new Label { Text = "Fecha:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Anchor = AnchorStyles.Left }, 2, 0);
            lblFecha = new Label { Text = _venta.FechaVenta.ToString("dd/MM/yyyy"), Anchor = AnchorStyles.Left };
            tlInfo.Controls.Add(lblFecha, 3, 0);

            // Fila 2
            tlInfo.Controls.Add(new Label { Text = "Cliente:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Anchor = AnchorStyles.Left }, 0, 1);
            lblCliente = new Label { Text = _venta.NombreCliente, Anchor = AnchorStyles.Left };
            tlInfo.SetColumnSpan(lblCliente, 3);
            tlInfo.Controls.Add(lblCliente, 1, 1);

            // Fila 3
            tlInfo.Controls.Add(new Label { Text = "Vendedor:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Anchor = AnchorStyles.Left }, 0, 2);
            lblVendedor = new Label { Text = _venta.NombreVendedor, Anchor = AnchorStyles.Left };
            tlInfo.Controls.Add(lblVendedor, 1, 2);

            tlInfo.Controls.Add(new Label { Text = "Estado:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Anchor = AnchorStyles.Left }, 2, 2);
            cbEstado = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbEstado.Items.AddRange(new[] { "Pendiente", "Completada", "Cancelada" });
            cbEstado.SelectedItem = _venta.Estado;
            cbEstado.SelectedIndexChanged += CbEstado_SelectedIndexChanged;
            tlInfo.Controls.Add(cbEstado, 3, 2);

            // Fila 4 - Observaciones
            tlInfo.Controls.Add(new Label { Text = "Observaciones:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Anchor = AnchorStyles.Left }, 0, 3);
            txtObservaciones = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                Text = _venta.Observaciones,
                ScrollBars = ScrollBars.Vertical
            };
            txtObservaciones.TextChanged += TxtObservaciones_TextChanged;
            tlInfo.SetColumnSpan(txtObservaciones, 3);
            tlInfo.Controls.Add(txtObservaciones, 1, 3);

            // Configurar filas
            tlInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            tlInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            tlInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            tlInfo.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            gbInfo.Controls.Add(tlInfo);

            // === DETALLES ===
            gbDetalles = new GroupBox
            {
                Text = "Detalles",
                Dock = DockStyle.Fill,
                Padding = new Padding(12)
            };

            var pnlDetalles = new Panel { Dock = DockStyle.Fill };

            // Grid de detalles
            dgvDetalles = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 35,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D
            };

            // Columnas
            dgvDetalles.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Producto", HeaderText = "Producto", Width = 200 },
                new DataGridViewTextBoxColumn { Name = "SKU", HeaderText = "SKU", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Cantidad", HeaderText = "Cantidad", Width = 80 },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "PrecioUnitario", 
                    HeaderText = "Precio Unit.", 
                    Width = 100,
                    DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight }
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Subtotal", 
                    HeaderText = "Subtotal", 
                    Width = 100,
                    DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight }
                }
            });

            // Panel de total
            var pnlTotal = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(0, 12, 0, 0)
            };

            lblTotal = new Label
            {
                Text = $"TOTAL: {_venta.Total:C2}",
                Dock = DockStyle.Right,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                AutoSize = true,
                ForeColor = Color.FromArgb(34, 139, 34)
            };

            pnlTotal.Controls.Add(lblTotal);

            pnlDetalles.Controls.Add(dgvDetalles);
            pnlDetalles.Controls.Add(pnlTotal);

            gbDetalles.Controls.Add(pnlDetalles);

            // === BOTONES ===
            var flAcciones = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 8, 0, 0)
            };

            btnCerrar = new Button
            {
                Text = "Cerrar",
                Height = 34,
                Width = 100,
                DialogResult = DialogResult.Cancel
            };

            btnImprimir = new Button
            {
                Text = "Imprimir",
                Height = 34,
                Width = 100,
                Margin = new Padding(8, 0, 0, 0)
            };

            btnGuardar = new Button
            {
                Text = "Guardar Cambios",
                Height = 34,
                Width = 120,
                Margin = new Padding(8, 0, 0, 0),
                Enabled = false,
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnGuardar.FlatAppearance.BorderSize = 0;

            flAcciones.Controls.Add(btnCerrar);
            flAcciones.Controls.Add(btnImprimir);
            flAcciones.Controls.Add(btnGuardar);

            // Eventos
            btnGuardar.Click += BtnGuardar_Click;
            btnImprimir.Click += BtnImprimir_Click;

            // Agregar al layout principal
            tlRoot.Controls.Add(gbInfo, 0, 0);
            tlRoot.Controls.Add(gbDetalles, 0, 1);
            tlRoot.Controls.Add(flAcciones, 0, 2);

            this.Controls.Add(tlRoot);
            this.ResumeLayout(false);
        }

        private void CargarDatos()
        {
            try
            {
                // Simular carga de detalles (en implementación real vendría de la BD)
                var detalles = new List<object>
                {
                    new { Producto = "Producto Ejemplo 1", SKU = "PROD001", Cantidad = 2, PrecioUnitario = 150.00m, Subtotal = 300.00m },
                    new { Producto = "Producto Ejemplo 2", SKU = "PROD002", Cantidad = 1, PrecioUnitario = (_venta.Total - 300.00m), Subtotal = (_venta.Total - 300.00m) }
                };

                dgvDetalles.DataSource = detalles;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar detalles: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CbEstado_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbEstado.SelectedItem.ToString() != _venta.Estado)
            {
                _modificado = true;
                btnGuardar.Enabled = true;
                this.Text = $"Detalle de {_venta.Tipo} - {_venta.NumeroVenta} *";
            }
        }

        private void TxtObservaciones_TextChanged(object sender, EventArgs e)
        {
            if (txtObservaciones.Text != _venta.Observaciones)
            {
                _modificado = true;
                btnGuardar.Enabled = true;
                this.Text = $"Detalle de {_venta.Tipo} - {_venta.NumeroVenta} *";
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_modificado)
                    return;

                _venta.Estado = cbEstado.SelectedItem.ToString();
                _venta.Observaciones = txtObservaciones.Text.Trim();

                if (_ventaRepo.Actualizar(_venta))
                {
                    _modificado = false;
                    btnGuardar.Enabled = false;
                    this.Text = $"Detalle de {_venta.Tipo} - {_venta.NumeroVenta}";
                    this.DialogResult = DialogResult.OK;

                    MessageBox.Show("Cambios guardados exitosamente.", "Éxito", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("No se pudieron guardar los cambios.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnImprimir_Click(object sender, EventArgs e)
        {
            try
            {
                MessageBox.Show("Funcionalidad de impresión en desarrollo.", "Información", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                // TODO: Implementar impresión
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_modificado)
            {
                var result = MessageBox.Show("Hay cambios sin guardar. ¿Desea guardarlos antes de cerrar?", 
                    "Cambios Sin Guardar", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    BtnGuardar_Click(null, null);
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

            base.OnFormClosing(e);
        }
    }
}