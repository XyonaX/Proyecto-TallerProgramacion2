using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
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
        private List<DetalleVenta> _detalles;
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
            this.Size = new Size(900, 700);
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
                BorderStyle = BorderStyle.Fixed3D,
                GridColor = Color.FromArgb(230, 230, 230),
                RowTemplate = new DataGridViewRow { Height = 30 },
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // Estilo de encabezados
            dgvDetalles.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 248, 255);
            dgvDetalles.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(0, 51, 102);
            dgvDetalles.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvDetalles.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Estilo de filas
            dgvDetalles.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 235, 250);
            dgvDetalles.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvDetalles.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);

            // Columnas
            dgvDetalles.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn 
                { 
                    Name = "NombreProducto", 
                    HeaderText = "Producto", 
                    DataPropertyName = "NombreProducto",
                    FillWeight = 40,
                    DefaultCellStyle = new DataGridViewCellStyle 
                    { 
                        Alignment = DataGridViewContentAlignment.MiddleLeft,
                        Padding = new Padding(8, 0, 0, 0)
                    }
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "SkuProducto", 
                    HeaderText = "SKU", 
                    DataPropertyName = "SkuProducto",
                    FillWeight = 15,
                    DefaultCellStyle = new DataGridViewCellStyle 
                    { 
                        Alignment = DataGridViewContentAlignment.MiddleCenter,
                        Font = new Font("Consolas", 9, FontStyle.Regular)
                    }
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Cantidad", 
                    HeaderText = "Cantidad", 
                    DataPropertyName = "Cantidad",
                    FillWeight = 10,
                    DefaultCellStyle = new DataGridViewCellStyle 
                    { 
                        Alignment = DataGridViewContentAlignment.MiddleCenter,
                        Font = new Font("Segoe UI", 9, FontStyle.Bold)
                    }
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "PrecioUnitario", 
                    HeaderText = "Precio Unit.", 
                    DataPropertyName = "PrecioUnitario",
                    FillWeight = 15,
                    DefaultCellStyle = new DataGridViewCellStyle 
                    { 
                        Format = "C2", 
                        Alignment = DataGridViewContentAlignment.MiddleRight,
                        Padding = new Padding(0, 0, 8, 0)
                    }
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Subtotal", 
                    HeaderText = "Subtotal", 
                    DataPropertyName = "Subtotal",
                    FillWeight = 20,
                    DefaultCellStyle = new DataGridViewCellStyle 
                    { 
                        Format = "C2", 
                        Alignment = DataGridViewContentAlignment.MiddleRight,
                        Padding = new Padding(0, 0, 8, 0),
                        Font = new Font("Segoe UI", 9, FontStyle.Bold),
                        ForeColor = Color.FromArgb(0, 102, 51)
                    }
                }
            });

            // Panel de total
            var pnlTotal = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(0, 12, 0, 0),
                BackColor = Color.FromArgb(245, 250, 255)
            };

            lblTotal = new Label
            {
                Text = $"TOTAL: {_venta.Total:C2}",
                Dock = DockStyle.Right,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = true,
                ForeColor = Color.FromArgb(0, 102, 51),
                Padding = new Padding(20, 10, 20, 10)
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
                DialogResult = DialogResult.Cancel,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White
            };
            btnCerrar.FlatAppearance.BorderSize = 0;

            btnImprimir = new Button
            {
                Text = "?? Imprimir",
                Height = 34,
                Width = 120,
                Margin = new Padding(8, 0, 0, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(23, 162, 184),
                ForeColor = Color.White
            };
            btnImprimir.FlatAppearance.BorderSize = 0;

            btnGuardar = new Button
            {
                Text = "Guardar Cambios",
                Height = 34,
                Width = 140,
                Margin = new Padding(8, 0, 0, 0),
                Enabled = false,
                BackColor = Color.FromArgb(40, 167, 69),
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
                this.Cursor = Cursors.WaitCursor;
                
                // Cargar detalles reales de la base de datos
                _detalles = _ventaRepo.ObtenerDetalles(_venta.IdVenta);
                
                if (_detalles.Count == 0)
                {
                    // Si no hay detalles en la BD, mostrar mensaje
                    var mensaje = new List<object>
                    {
                        new { 
                            NombreProducto = "No hay productos registrados para esta venta", 
                            SkuProducto = "-", 
                            Cantidad = 0, 
                            PrecioUnitario = 0.00m, 
                            Subtotal = 0.00m 
                        }
                    };
                    dgvDetalles.DataSource = mensaje;
                }
                else
                {
                    // Mostrar detalles reales
                    dgvDetalles.DataSource = _detalles;
                }

                this.Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show($"Error al cargar detalles: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                
                // En caso de error, mostrar mensaje
                var error = new List<object>
                {
                    new { 
                        NombreProducto = $"Error al cargar datos: {ex.Message}", 
                        SkuProducto = "ERROR", 
                        Cantidad = 0, 
                        PrecioUnitario = 0.00m, 
                        Subtotal = 0.00m 
                    }
                };
                dgvDetalles.DataSource = error;
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
                if (_detalles == null || _detalles.Count == 0)
                {
                    MessageBox.Show("No hay detalles para imprimir.", "Información", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Configurar impresión
                var printDocument = new PrintDocument();
                printDocument.PrintPage += PrintDocument_PrintPage;
                
                // Mostrar preview de impresión
                var printPreview = new PrintPreviewDialog
                {
                    Document = printDocument,
                    Width = 800,
                    Height = 600,
                    UseAntiAlias = true
                };

                if (printPreview.ShowDialog() == DialogResult.OK)
                {
                    printDocument.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            try
            {
                var graphics = e.Graphics;
                var font = new Font("Arial", 10);
                var fontBold = new Font("Arial", 10, FontStyle.Bold);
                var fontTitle = new Font("Arial", 14, FontStyle.Bold);
                var fontHeader = new Font("Arial", 12, FontStyle.Bold);

                var brush = Brushes.Black;
                var pen = new Pen(Color.Black, 1);

                int y = 50;
                int leftMargin = 50;
                int rightMargin = e.PageBounds.Width - 50;

                // TÍTULO
                var title = $"FACTURA - {_venta.NumeroVenta}";
                graphics.DrawString(title, fontTitle, brush, leftMargin, y);
                y += 30;

                // Línea separadora
                graphics.DrawLine(pen, leftMargin, y, rightMargin, y);
                y += 20;

                // INFORMACIÓN DE LA VENTA
                graphics.DrawString($"Fecha: {_venta.FechaVenta:dd/MM/yyyy}", font, brush, leftMargin, y);
                graphics.DrawString($"Estado: {_venta.Estado}", font, brush, rightMargin - 200, y);
                y += 20;

                graphics.DrawString($"Cliente: {_venta.NombreCliente}", font, brush, leftMargin, y);
                y += 20;

                graphics.DrawString($"Vendedor: {_venta.NombreVendedor}", font, brush, leftMargin, y);
                y += 30;

                // ENCABEZADOS DE TABLA
                graphics.DrawString("DETALLE DE PRODUCTOS", fontHeader, brush, leftMargin, y);
                y += 25;

                // Línea separadora
                graphics.DrawLine(pen, leftMargin, y, rightMargin, y);
                y += 15;

                // Encabezados de columnas
                graphics.DrawString("Producto", fontBold, brush, leftMargin, y);
                graphics.DrawString("SKU", fontBold, brush, leftMargin + 250, y);
                graphics.DrawString("Cant.", fontBold, brush, leftMargin + 350, y);
                graphics.DrawString("Precio Unit.", fontBold, brush, leftMargin + 420, y);
                graphics.DrawString("Subtotal", fontBold, brush, leftMargin + 520, y);
                y += 20;

                // Línea separadora
                graphics.DrawLine(pen, leftMargin, y, rightMargin, y);
                y += 15;

                // DETALLES DE PRODUCTOS
                decimal totalCalculado = 0;
                foreach (var detalle in _detalles)
                {
                    if (y > e.PageBounds.Height - 100) // Nueva página si es necesario
                        break;

                    graphics.DrawString(detalle.NombreProducto, font, brush, leftMargin, y);
                    graphics.DrawString(detalle.SkuProducto, font, brush, leftMargin + 250, y);
                    graphics.DrawString(detalle.Cantidad.ToString(), font, brush, leftMargin + 350, y);
                    graphics.DrawString(detalle.PrecioUnitario.ToString("C2"), font, brush, leftMargin + 420, y);
                    graphics.DrawString(detalle.Subtotal.ToString("C2"), font, brush, leftMargin + 520, y);

                    totalCalculado += detalle.Subtotal;
                    y += 18;
                }

                y += 10;
                
                // Línea separadora final
                graphics.DrawLine(pen, leftMargin, y, rightMargin, y);
                y += 20;

                // TOTAL
                var totalText = $"TOTAL: {_venta.Total:C2}";
                var totalSize = graphics.MeasureString(totalText, fontHeader);
                graphics.DrawString(totalText, fontHeader, brush, rightMargin - totalSize.Width, y);
                y += 30;

                // OBSERVACIONES
                if (!string.IsNullOrEmpty(_venta.Observaciones))
                {
                    y += 10;
                    graphics.DrawString("Observaciones:", fontBold, brush, leftMargin, y);
                    y += 20;
                    graphics.DrawString(_venta.Observaciones, font, brush, leftMargin, y);
                }

                // PIE DE PÁGINA
                y = e.PageBounds.Height - 80;
                graphics.DrawLine(pen, leftMargin, y, rightMargin, y);
                y += 15;
                graphics.DrawString($"Documento generado el {DateTime.Now:dd/MM/yyyy HH:mm}", 
                    new Font("Arial", 8), brush, leftMargin, y);
                graphics.DrawString("Sistema ERP - Gestión de Ventas", 
                    new Font("Arial", 8), brush, rightMargin - 200, y);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error durante la impresión: {ex.Message}", "Error de Impresión", 
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