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
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

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
                Padding = new Padding(12)
            };

            var tlDatos = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3
            };
            
            // Fila 1
            tlDatos.Controls.Add(new Label { Text = "Cliente:", Anchor = AnchorStyles.Left | AnchorStyles.Bottom }, 0, 0);
            tlDatos.Controls.Add(new Label { Text = "Tipo:", Anchor = AnchorStyles.Left | AnchorStyles.Bottom }, 2, 0);

            cbCliente = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 4, 8, 0)
            };

            cbTipo = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 4, 0, 0)
            };
            cbTipo.Items.AddRange(new[] { "Venta", "Cotización" });

            tlDatos.SetColumnSpan(cbCliente, 2);
            tlDatos.Controls.Add(cbCliente, 0, 1);
            tlDatos.Controls.Add(cbTipo, 2, 1);

            // Fila 2
            tlDatos.Controls.Add(new Label { Text = "Fecha:", Anchor = AnchorStyles.Left | AnchorStyles.Bottom }, 0, 2);
            
            dtpFecha = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now,
                Margin = new Padding(0, 4, 8, 0)
            };
            tlDatos.Controls.Add(dtpFecha, 0, 3);

            // Observaciones
            tlDatos.Controls.Add(new Label { Text = "Observaciones:", Anchor = AnchorStyles.Left | AnchorStyles.Bottom }, 1, 2);
            txtObservaciones = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                Margin = new Padding(0, 4, 0, 0)
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
                Padding = new Padding(12)
            };

            var pnlDetalles = new Panel { Dock = DockStyle.Fill };

            // Botones de detalle
            var flBotones = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 0, 0, 8)
            };

            btnAgregar = new Button
            {
                Text = "Agregar Item",
                Height = 28,
                AutoSize = true,
                Margin = new Padding(0, 0, 8, 0)
            };

            btnQuitar = new Button
            {
                Text = "Quitar Seleccionado",
                Height = 28,
                AutoSize = true,
                Enabled = false
            };

            flBotones.Controls.Add(btnAgregar);
            flBotones.Controls.Add(btnQuitar);

            // DataGridView
            dgvDetalles = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 35,
                RowHeadersVisible = false
            };

            // Columnas del grid
            dgvDetalles.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Producto", HeaderText = "Producto", Width = 200, DataPropertyName = "NombreProducto" },
                new DataGridViewTextBoxColumn { Name = "SKU", HeaderText = "SKU", Width = 100, DataPropertyName = "SkuProducto" },
                new DataGridViewTextBoxColumn { Name = "Cantidad", HeaderText = "Cantidad", Width = 80, DataPropertyName = "Cantidad" },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "PrecioUnitario", 
                    HeaderText = "Precio Unit.", 
                    Width = 100, 
                    DataPropertyName = "PrecioUnitario",
                    DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight }
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Subtotal", 
                    HeaderText = "Subtotal", 
                    Width = 100,
                    DataPropertyName = "Subtotal",
                    DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight }
                }
            });

            dgvDetalles.DataSource = _detalles;

            // Total
            var pnlTotal = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                Padding = new Padding(0, 8, 0, 0)
            };

            lblTotal = new Label
            {
                Text = "TOTAL: $0.00",
                Dock = DockStyle.Right,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                AutoSize = true
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
                Height = 34,
                Width = 100,
                DialogResult = DialogResult.Cancel
            };

            btnGuardar = new Button
            {
                Text = "Guardar",
                Height = 34,
                Width = 100,
                Margin = new Padding(8, 0, 0, 0)
            };

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

                if (cbCliente.Items.Count > 0)
                    cbCliente.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            // Simulación de agregar un producto (en una implementación real sería un formulario separado)
            using (var form = new AgregarItemForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    var detalle = form.DetalleCreado;
                    if (detalle != null)
                    {
                        _detalles.Add(detalle);
                        ActualizarTotal();
                        RefrescarGrid();
                    }
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
                    _detalles.RemoveAt(index);
                    ActualizarTotal();
                    RefrescarGrid();
                }
            }
        }

        private void DgvDetalles_SelectionChanged(object sender, EventArgs e)
        {
            btnQuitar.Enabled = dgvDetalles.SelectedRows.Count > 0;
        }

        private void RefrescarGrid()
        {
            dgvDetalles.DataSource = null;
            dgvDetalles.DataSource = _detalles;
        }

        private void ActualizarTotal()
        {
            var total = _detalles.Sum(d => d.Subtotal);
            lblTotal.Text = $"TOTAL: {total:C2}";
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidarFormulario())
                    return;

                var venta = new Venta
                {
                    IdUsuario = _currentUser.IdUsuario,
                    IdCliente = (int)cbCliente.SelectedValue,
                    FechaVenta = dtpFecha.Value,
                    Tipo = cbTipo.SelectedItem.ToString(),
                    Estado = cbTipo.SelectedItem.ToString() == "Cotización" ? "Pendiente" : "Completada",
                    Total = _detalles.Sum(d => d.Subtotal),
                    Observaciones = txtObservaciones.Text.Trim(),
                    Detalles = _detalles.ToList()
                };

                var idVenta = _ventaRepo.Agregar(venta);
                venta.IdVenta = idVenta;

                VentaCreada = venta;

                MessageBox.Show($"{venta.Tipo} creada exitosamente.\nNúmero: {venta.NumeroVenta}", 
                    "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidarFormulario()
        {
            if (cbCliente.SelectedValue == null)
            {
                MessageBox.Show("Debe seleccionar un cliente.", "Validación", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (_detalles.Count == 0)
            {
                MessageBox.Show("Debe agregar al menos un item.", "Validación", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }

    // Formulario auxiliar para agregar items
    public partial class AgregarItemForm : Form
    {
        private TextBox txtProducto, txtSku, txtCantidad, txtPrecio;
        private Button btnOk, btnCancelar;

        public DetalleVenta DetalleCreado { get; private set; }

        public AgregarItemForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Agregar Item";
            this.Size = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var tl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 6,
                Padding = new Padding(16)
            };

            // Producto
            tl.Controls.Add(new Label { Text = "Producto:", Anchor = AnchorStyles.Left }, 0, 0);
            txtProducto = new TextBox { Dock = DockStyle.Fill };
            tl.Controls.Add(txtProducto, 1, 0);

            // SKU
            tl.Controls.Add(new Label { Text = "SKU:", Anchor = AnchorStyles.Left }, 0, 1);
            txtSku = new TextBox { Dock = DockStyle.Fill };
            tl.Controls.Add(txtSku, 1, 1);

            // Cantidad
            tl.Controls.Add(new Label { Text = "Cantidad:", Anchor = AnchorStyles.Left }, 0, 2);
            txtCantidad = new TextBox { Dock = DockStyle.Fill, Text = "1" };
            tl.Controls.Add(txtCantidad, 1, 2);

            // Precio
            tl.Controls.Add(new Label { Text = "Precio Unitario:", Anchor = AnchorStyles.Left }, 0, 3);
            txtPrecio = new TextBox { Dock = DockStyle.Fill };
            tl.Controls.Add(txtPrecio, 1, 3);

            // Botones
            var flBotones = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft
            };

            btnCancelar = new Button
            {
                Text = "Cancelar",
                DialogResult = DialogResult.Cancel,
                Height = 28,
                Width = 80
            };

            btnOk = new Button
            {
                Text = "Agregar",
                Height = 28,
                Width = 80,
                Margin = new Padding(8, 0, 0, 0)
            };

            btnOk.Click += BtnOk_Click;

            flBotones.Controls.Add(btnCancelar);
            flBotones.Controls.Add(btnOk);

            tl.SetColumnSpan(flBotones, 2);
            tl.Controls.Add(flBotones, 0, 5);

            // Estilos de fila
            tl.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            tl.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            tl.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            tl.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            tl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tl.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            this.Controls.Add(tl);
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (Validar())
            {
                var cantidad = int.Parse(txtCantidad.Text);
                var precio = decimal.Parse(txtPrecio.Text);

                DetalleCreado = new DetalleVenta
                {
                    IdProducto = 1, // Simulado
                    NombreProducto = txtProducto.Text.Trim(),
                    SkuProducto = txtSku.Text.Trim(),
                    Cantidad = cantidad,
                    PrecioUnitario = precio
                    // Subtotal se calcula automáticamente
                };

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private bool Validar()
        {
            if (string.IsNullOrWhiteSpace(txtProducto.Text))
            {
                MessageBox.Show("Ingrese el nombre del producto.", "Validación");
                return false;
            }

            if (!int.TryParse(txtCantidad.Text, out int cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Ingrese una cantidad válida.", "Validación");
                return false;
            }

            if (!decimal.TryParse(txtPrecio.Text, out decimal precio) || precio <= 0)
            {
                MessageBox.Show("Ingrese un precio válido.", "Validación");
                return false;
            }

            return true;
        }
    }
}