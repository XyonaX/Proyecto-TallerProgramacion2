using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Proyecto_Taller_2.Data.Repositories;
using Proyecto_Taller_2.Domain.Models;
using Proyecto_Taller_2.Data;

namespace Proyecto_Taller_2.Forms
{
    public partial class NuevaVentaForm : Form
    {
        private readonly VentaRepository _ventaRepo;
        private readonly ProductoRepository _productoRepo;
        private readonly Usuario _currentUser;
        private readonly List<Cliente> _clientes;
        private readonly List<DetalleVenta> _detalles;

        // Controles
        private TableLayoutPanel tlRoot;
        private GroupBox gbDatos, gbDetalles;
        private ComboBox cbCliente, cbTipo;
        private DateTimePicker dtpFecha;
        private TextBox txtObservaciones;
        private DataGridView dgvDetalles;
        private Label lblTotal;
        private Button btnAgregar, btnQuitar, btnGuardar, btnCancelar;

        public Venta VentaCreada { get; private set; }

        public NuevaVentaForm(Usuario currentUser, string tipo = "Venta")
        {
            _currentUser = currentUser;
            _ventaRepo = new VentaRepository(BDGeneral.ConnectionString);
            _productoRepo = new ProductoRepository(BDGeneral.ConnectionString);
            _clientes = new List<Cliente>();
            _detalles = new List<DetalleVenta>();

            InitializeComponent();
            ConfigurarFormulario(tipo);
            CargarDatos();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form
            this.Text = "Nueva Venta";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(800, 600);

            // Root Layout
            tlRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(16)
            };
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 180)); // Datos generales
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Detalles
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // Botones

            // === DATOS GENERALES ===
            gbDatos = new GroupBox
            {
                Text = "Datos Generales",
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            var tlDatos = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 4
            };
            
            // Fila 1
            tlDatos.Controls.Add(new Label { Text = "Cliente:", Anchor = AnchorStyles.Left | AnchorStyles.Bottom, Font = new Font("Segoe UI", 9, FontStyle.Bold) }, 0, 0);
            tlDatos.Controls.Add(new Label { Text = "Tipo:", Anchor = AnchorStyles.Left | AnchorStyles.Bottom, Font = new Font("Segoe UI", 9, FontStyle.Bold) }, 2, 0);

            cbCliente = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 4, 8, 0),
                Font = new Font("Segoe UI", 9)
            };

            cbTipo = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 4, 0, 0),
                Font = new Font("Segoe UI", 9)
            };
            cbTipo.Items.AddRange(new[] { "Venta", "Cotización" });

            tlDatos.SetColumnSpan(cbCliente, 2);
            tlDatos.Controls.Add(cbCliente, 0, 1);
            tlDatos.Controls.Add(cbTipo, 2, 1);

            // Fila 2
            tlDatos.Controls.Add(new Label { Text = "Fecha:", Anchor = AnchorStyles.Left | AnchorStyles.Bottom, Font = new Font("Segoe UI", 9, FontStyle.Bold) }, 0, 2);
            
            dtpFecha = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now,
                Margin = new Padding(0, 4, 8, 0),
                Font = new Font("Segoe UI", 9)
            };
            tlDatos.Controls.Add(dtpFecha, 0, 3);

            // Observaciones
            tlDatos.Controls.Add(new Label { Text = "Observaciones:", Anchor = AnchorStyles.Left | AnchorStyles.Bottom, Font = new Font("Segoe UI", 9, FontStyle.Bold) }, 1, 2);
            txtObservaciones = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                Margin = new Padding(0, 4, 0, 0),
                Font = new Font("Segoe UI", 9),
                ScrollBars = ScrollBars.Vertical
            };
            tlDatos.SetColumnSpan(txtObservaciones, 3);
            tlDatos.Controls.Add(txtObservaciones, 1, 3);

            // Configurar columnas
            for (int i = 0; i < 4; i++)
                tlDatos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            tlDatos.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            tlDatos.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            tlDatos.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            tlDatos.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            gbDatos.Controls.Add(tlDatos);

            // === DETALLES ===
            gbDetalles = new GroupBox
            {
                Text = "Detalles de la Venta",
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            var pnlDetalles = new Panel { Dock = DockStyle.Fill };

            // Botones de detalle
            var flBotones = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 50,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 0, 0, 12)
            };

            btnAgregar = new Button
            {
                Text = "[+] Seleccionar Productos",
                Height = 36,
                AutoSize = true,
                Margin = new Padding(0, 0, 12, 0),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnAgregar.FlatAppearance.BorderSize = 0;

            btnQuitar = new Button
            {
                Text = "[-] Quitar Seleccionado",
                Height = 36,
                AutoSize = true,
                Enabled = false,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9)
            };
            btnQuitar.FlatAppearance.BorderSize = 0;

            flBotones.Controls.Add(btnAgregar);
            flBotones.Controls.Add(btnQuitar);

            // DataGridView
            dgvDetalles = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 40,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                GridColor = Color.FromArgb(230, 230, 230),
                RowTemplate = { Height = 35 },
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
            dgvDetalles.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            dgvDetalles.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);

            // Columnas del grid mejoradas
            dgvDetalles.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Producto", 
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
                    Name = "SKU", 
                    HeaderText = "SKU", 
                    DataPropertyName = "SkuProducto",
                    FillWeight = 15,
                    DefaultCellStyle = new DataGridViewCellStyle 
                    { 
                        Alignment = DataGridViewContentAlignment.MiddleCenter,
                        Font = new Font("Consolas", 9, FontStyle.Regular),
                        ForeColor = Color.FromArgb(0, 102, 153)
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
                        Font = new Font("Segoe UI", 9, FontStyle.Bold),
                        ForeColor = Color.FromArgb(0, 102, 51)
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

            dgvDetalles.DataSource = _detalles;

            // Total
            var pnlTotal = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(0, 12, 0, 0),
                BackColor = Color.FromArgb(245, 250, 255)
            };

            lblTotal = new Label
            {
                Text = "TOTAL: $0.00",
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
            pnlDetalles.Controls.Add(flBotones);

            gbDetalles.Controls.Add(pnlDetalles);

            // === BOTONES ===
            var flAcciones = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 8, 0, 0)
            };

            btnCancelar = new Button
            {
                Text = "Cancelar",
                Height = 38,
                Width = 110,
                DialogResult = DialogResult.Cancel,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9)
            };
            btnCancelar.FlatAppearance.BorderSize = 0;

            btnGuardar = new Button
            {
                Text = "[SAVE] Guardar",
                Height = 38,
                Width = 130,
                Margin = new Padding(12, 0, 0, 0),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnGuardar.FlatAppearance.BorderSize = 0;

            flAcciones.Controls.Add(btnCancelar);
            flAcciones.Controls.Add(btnGuardar);

            // Agregar al layout principal
            tlRoot.Controls.Add(gbDatos, 0, 0);
            tlRoot.Controls.Add(gbDetalles, 0, 1);
            tlRoot.Controls.Add(flAcciones, 0, 2);

            this.Controls.Add(tlRoot);
            this.ResumeLayout(false);

            // Eventos
            btnAgregar.Click += BtnAgregar_Click;
            btnQuitar.Click += BtnQuitar_Click;
            btnGuardar.Click += BtnGuardar_Click;
            dgvDetalles.SelectionChanged += DgvDetalles_SelectionChanged;
        }

        private void ConfigurarFormulario(string tipo)
        {
            if (tipo == "Cotización")
            {
                this.Text = "Nueva Cotización";
                gbDetalles.Text = "Detalles de la Cotización";
                cbTipo.SelectedItem = "Cotización";
            }
            else
            {
                cbTipo.SelectedItem = "Venta";
            }
        }

        private void CargarDatos()
        {
            try
            {
                // Cargar clientes
                var clientes = _ventaRepo.ObtenerClientes(true);
                _clientes.Clear();
                _clientes.AddRange(clientes);

                cbCliente.DisplayMember = "NombreCompleto";
                cbCliente.ValueMember = "IdCliente";
                cbCliente.DataSource = _clientes;

                // Asegurar que hay una selección válida
                if (cbCliente.Items.Count > 0)
                {
                    cbCliente.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("No hay clientes disponibles. Por favor, agregue al menos un cliente antes de crear una venta.", 
                        "Sin clientes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            // Nueva funcionalidad mejorada para seleccionar productos
            using (var form = new SeleccionarProductosForm(_productoRepo, _detalles))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    var productosSeleccionados = form.ProductosSeleccionados;
                    
                    foreach (var producto in productosSeleccionados)
                    {
                        // Verificar si el producto ya existe en la lista
                        var existente = _detalles.FirstOrDefault(d => d.IdProducto == producto.IdProducto);
                        
                        if (existente != null)
                        {
                            // Si existe, actualizar cantidad
                            existente.Cantidad += producto.Cantidad;
                        }
                        else
                        {
                            // Si no existe, agregar nuevo
                            _detalles.Add(producto);
                        }
                    }
                    
                    ActualizarTotal();
                    RefrescarGrid();
                }
            }
        }

        private void BtnQuitar_Click(object sender, EventArgs e)
        {
            if (dgvDetalles.SelectedRows.Count > 0)
            {
                var index = dgvDetalles.SelectedRows[0].Index;
                if (index >= 0 && index < _detalles.Count)
                {
                    var detalle = _detalles[index];
                    var resultado = MessageBox.Show(
                        $"¿Está seguro de quitar '{detalle.NombreProducto}' de la venta?",
                        "Confirmar eliminación",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                        
                    if (resultado == DialogResult.Yes)
                    {
                        _detalles.RemoveAt(index);
                        ActualizarTotal();
                        RefrescarGrid();
                    }
                }
            }
        }

        private void DgvDetalles_SelectionChanged(object sender, EventArgs e)
        {
            btnQuitar.Enabled = dgvDetalles.SelectedRows.Count > 0 && _detalles.Count > 0;
        }

        private void RefrescarGrid()
        {
            dgvDetalles.DataSource = null;
            dgvDetalles.DataSource = _detalles;
            dgvDetalles.Refresh();
            
            // Ajustar altura de filas si es necesario
            foreach (DataGridViewRow row in dgvDetalles.Rows)
            {
                row.Height = 35;
            }
        }

        private void ActualizarTotal()
        {
            var total = _detalles.Sum(d => d.Subtotal);
            lblTotal.Text = $"TOTAL: {total:C2}";
            
            // Cambiar color según el monto
            if (total > 1000)
                lblTotal.ForeColor = Color.FromArgb(0, 102, 51); // Verde
            else if (total > 500)
                lblTotal.ForeColor = Color.FromArgb(255, 165, 0); // Naranja
            else
                lblTotal.ForeColor = Color.FromArgb(108, 117, 125); // Gris
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidarFormulario())
                    return;

                // Mostrar confirmación antes de guardar
                var tipoOperacion = cbTipo.SelectedItem.ToString();
                var mensaje = tipoOperacion == "Venta" 
                    ? "¿Confirma la creación de esta VENTA? Esto actualizará el stock de los productos."
                    : "¿Confirma la creación de esta COTIZACIÓN?";

                var confirmacion = MessageBox.Show(mensaje, "Confirmar " + tipoOperacion, 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirmacion != DialogResult.Yes)
                    return;

                var venta = new Venta
                {
                    IdUsuario = _currentUser.IdUsuario,
                    IdCliente = Convert.ToInt32(cbCliente.SelectedValue),
                    FechaVenta = dtpFecha.Value,
                    Tipo = tipoOperacion,
                    Estado = tipoOperacion == "Cotización" ? "Pendiente" : "Completada",
                    Total = _detalles.Sum(d => d.Subtotal),
                    Observaciones = txtObservaciones.Text.Trim(),
                    Detalles = _detalles.ToList()
                };

                var idVenta = _ventaRepo.Agregar(venta);
                venta.IdVenta = idVenta;

                VentaCreada = venta;

                var mensajeExito = $"{venta.Tipo} creada exitosamente.\nNúmero: {venta.NumeroVenta}";
                if (venta.Tipo == "Venta")
                {
                    mensajeExito += "\n\nEl stock de los productos ha sido actualizado automáticamente.";
                }

                MessageBox.Show(mensajeExito, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (InvalidOperationException ex)
            {
                // Error de stock insuficiente
                MessageBox.Show($"No se pudo completar la venta:\n\n{ex.Message}", "Stock Insuficiente", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidarFormulario()
        {
            // Validar que hay un cliente seleccionado
            if (cbCliente.SelectedIndex == -1 || cbCliente.SelectedValue == null || !(cbCliente.SelectedValue is int clienteId) || clienteId <= 0)
            {
                MessageBox.Show("Debe seleccionar un cliente válido.", "Validación", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cbCliente.Focus();
                return false;
            }

            if (_detalles.Count == 0)
            {
                MessageBox.Show("Debe agregar al menos un producto a la venta.", "Validación", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                btnAgregar.Focus();
                return false;
            }

            // Validar que todos los productos tienen cantidad válida
            foreach (var detalle in _detalles)
            {
                if (detalle.Cantidad <= 0)
                {
                    MessageBox.Show($"La cantidad para '{detalle.NombreProducto}' debe ser mayor a 0.", 
                        "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                
                if (detalle.PrecioUnitario <= 0)
                {
                    MessageBox.Show($"El precio para '{detalle.NombreProducto}' debe ser mayor a 0.", 
                        "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            return true;
        }
    }
}