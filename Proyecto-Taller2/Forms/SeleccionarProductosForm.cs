using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Proyecto_Taller_2.Data.Repositories;
using Proyecto_Taller_2.Domain.Models;

namespace Proyecto_Taller_2.Forms
{
    public partial class SeleccionarProductosForm : Form
    {
        private readonly ProductoRepository _productoRepo;
        private readonly List<DetalleVenta> _detallesExistentes;
        private readonly List<Producto> _todosLosProductos;
        private readonly List<Producto> _productosFiltrados;
        private readonly BindingList<ProductoVenta> _productosParaVenta;

        // Controles
        private TableLayoutPanel tlRoot;
        private GroupBox gbBuscar, gbProductos, gbSeleccionados;
        private TextBox txtBuscar;
        private ComboBox cbCategoria, cbEstadoStock;
        private Button btnLimpiarFiltros, btnCancelar, btnAceptar;
        private DataGridView dgvProductos, dgvSeleccionados;
        private Label lblResultados, lblTotalSeleccionados;

        public List<DetalleVenta> ProductosSeleccionados { get; private set; }

        public SeleccionarProductosForm(ProductoRepository productoRepo, List<DetalleVenta> detallesExistentes)
        {
            _productoRepo = productoRepo;
            _detallesExistentes = detallesExistentes;
            _todosLosProductos = new List<Producto>();
            _productosFiltrados = new List<Producto>();
            _productosParaVenta = new BindingList<ProductoVenta>();
            ProductosSeleccionados = new List<DetalleVenta>();

            InitializeComponent();
            CargarDatos();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form
            this.Text = "Seleccionar Productos para la Venta";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(1000, 700);

            // Root Layout (3 columnas)
            tlRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(16)
            };
            
            // Columnas: 60% productos disponibles, 40% productos seleccionados
            tlRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            tlRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            
            // Filas: Filtros (más altura), Productos, Botones
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 140)); // Aumentar de 120 a 140
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Contenido
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));  // Botones

            // === PANEL DE FILTROS ===
            gbBuscar = new GroupBox
            {
                Text = "Buscar y Filtrar Productos",
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            var tlFiltros = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3
            };

            // Fila 1: Búsqueda por texto
            tlFiltros.Controls.Add(new Label { Text = "Buscar:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Anchor = AnchorStyles.Left }, 0, 0);
            txtBuscar = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9),
                Margin = new Padding(0, 4, 8, 0)
            };
            tlFiltros.SetColumnSpan(txtBuscar, 3);
            tlFiltros.Controls.Add(txtBuscar, 1, 0);

            // Fila 2: Filtros por categoría y stock
            tlFiltros.Controls.Add(new Label { Text = "Categoria:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Anchor = AnchorStyles.Left }, 0, 1);
            cbCategoria = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9),
                Margin = new Padding(0, 4, 8, 0)
            };
            tlFiltros.Controls.Add(cbCategoria, 1, 1);

            tlFiltros.Controls.Add(new Label { Text = "Stock:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Anchor = AnchorStyles.Left }, 2, 1);
            cbEstadoStock = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9),
                Margin = new Padding(0, 4, 0, 0)
            };
            cbEstadoStock.Items.AddRange(new[] { "Todo Stock", "Solo Disponible", "Stock Bajo" });
            cbEstadoStock.SelectedIndex = 1; // "Solo Disponible" por defecto
            tlFiltros.Controls.Add(cbEstadoStock, 3, 1);

            // Fila 3: Botón limpiar filtros y resultados
            btnLimpiarFiltros = new Button
            {
                Text = "Limpiar Filtros",
                Height = 35, // Aumentar altura
                Width = 130, // Aumentar ancho
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 123, 255), // Cambiar a azul para mejor visibilidad
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Margin = new Padding(0, 5, 0, 0) // Agregar margen superior
            };
            btnLimpiarFiltros.FlatAppearance.BorderSize = 0;
            tlFiltros.Controls.Add(btnLimpiarFiltros, 0, 2);

            lblResultados = new Label
            {
                Text = "0 productos encontrados",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.FromArgb(108, 117, 125)
            };
            tlFiltros.SetColumnSpan(lblResultados, 3);
            tlFiltros.Controls.Add(lblResultados, 1, 2);

            // Configurar estilos de columnas y filas
            tlFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));  
            tlFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));   
            tlFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));  
            tlFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));   

            tlFiltros.RowStyles.Add(new RowStyle(SizeType.Absolute, 35)); // Aumentar altura
            tlFiltros.RowStyles.Add(new RowStyle(SizeType.Absolute, 35)); // Aumentar altura
            tlFiltros.RowStyles.Add(new RowStyle(SizeType.Absolute, 45)); // Aumentar altura para el botón

            gbBuscar.Controls.Add(tlFiltros);
            tlRoot.SetColumnSpan(gbBuscar, 2);
            tlRoot.Controls.Add(gbBuscar, 0, 0);

            // === PANEL DE PRODUCTOS DISPONIBLES ===
            gbProductos = new GroupBox
            {
                Text = "Productos Disponibles",
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            var pnlProductos = new Panel { Dock = DockStyle.Fill };

            // Grid de productos disponibles
            dgvProductos = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoGenerateColumns = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 40,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                GridColor = Color.FromArgb(230, 230, 230),
                RowTemplate = { Height = 32 },
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // Estilo de encabezados
            dgvProductos.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 248, 255);
            dgvProductos.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(0, 51, 102);
            dgvProductos.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);

            // Estilo de filas
            dgvProductos.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 235, 250);
            dgvProductos.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvProductos.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            dgvProductos.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);

            // Columnas para productos disponibles
            dgvProductos.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Nombre", 
                    HeaderText = "Producto", 
                    DataPropertyName = "Nombre",
                    FillWeight = 35,
                    DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleLeft, Padding = new Padding(8, 0, 0, 0) }
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Sku", 
                    HeaderText = "SKU", 
                    DataPropertyName = "Sku",
                    FillWeight = 15,
                    DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Consolas", 8) }
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "CategoriaNombre", 
                    HeaderText = "Categoria", 
                    DataPropertyName = "CategoriaNombre",
                    FillWeight = 15,
                    DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Stock", 
                    HeaderText = "Stock", 
                    DataPropertyName = "Stock",
                    FillWeight = 10,
                    DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9, FontStyle.Bold) }
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Precio", 
                    HeaderText = "Precio", 
                    DataPropertyName = "Precio",
                    FillWeight = 15,
                    DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight, Padding = new Padding(0, 0, 8, 0) }
                },
                new DataGridViewButtonColumn
                {
                    Name = "Agregar",
                    HeaderText = "Accion",
                    Text = "Agregar",
                    UseColumnTextForButtonValue = true,
                    FillWeight = 10,
                    DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
                }
            });

            pnlProductos.Controls.Add(dgvProductos);
            gbProductos.Controls.Add(pnlProductos);
            tlRoot.Controls.Add(gbProductos, 0, 1);

            // === PANEL DE PRODUCTOS SELECCIONADOS ===
            gbSeleccionados = new GroupBox
            {
                Text = "Productos Seleccionados",
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            var pnlSeleccionados = new Panel { Dock = DockStyle.Fill };

            // Grid de productos seleccionados
            dgvSeleccionados = new DataGridView
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
                RowTemplate = { Height = 32 },
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // Estilo similar al anterior
            dgvSeleccionados.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 255, 240);
            dgvSeleccionados.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(0, 102, 51);
            dgvSeleccionados.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);

            dgvSeleccionados.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 255, 220);
            dgvSeleccionados.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvSeleccionados.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            dgvSeleccionados.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 255, 248);

            // Columnas para productos seleccionados
            dgvSeleccionados.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Producto", 
                    HeaderText = "Producto", 
                    DataPropertyName = "Producto",
                    FillWeight = 40,
                    ReadOnly = true,
                    DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleLeft, Padding = new Padding(8, 0, 0, 0) }
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Cantidad", 
                    HeaderText = "Cantidad", 
                    DataPropertyName = "Cantidad",
                    FillWeight = 20,
                    DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Precio", 
                    HeaderText = "Precio", 
                    DataPropertyName = "Precio",
                    FillWeight = 20,
                    ReadOnly = true,
                    DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight }
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Subtotal", 
                    HeaderText = "Subtotal", 
                    DataPropertyName = "Subtotal",
                    FillWeight = 20,
                    ReadOnly = true,
                    DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9, FontStyle.Bold) }
                },
                new DataGridViewButtonColumn
                {
                    Name = "Quitar",
                    HeaderText = "",
                    Text = "X",
                    UseColumnTextForButtonValue = true,
                    FillWeight = 10,
                    DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
                }
            });

            dgvSeleccionados.DataSource = _productosParaVenta;

            // Panel de total seleccionados
            var pnlTotalSeleccionados = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(0, 8, 0, 0),
                BackColor = Color.FromArgb(248, 255, 248)
            };

            lblTotalSeleccionados = new Label
            {
                Text = "Total: $0.00",
                Dock = DockStyle.Right,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = true,
                ForeColor = Color.FromArgb(0, 102, 51),
                Padding = new Padding(16, 8, 16, 8)
            };

            pnlTotalSeleccionados.Controls.Add(lblTotalSeleccionados);

            pnlSeleccionados.Controls.Add(dgvSeleccionados);
            pnlSeleccionados.Controls.Add(pnlTotalSeleccionados);

            gbSeleccionados.Controls.Add(pnlSeleccionados);
            tlRoot.Controls.Add(gbSeleccionados, 1, 1);

            // === BOTONES ===
            var flAcciones = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 12, 0, 0)
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

            btnAceptar = new Button
            {
                Text = "Agregar a la Venta",
                Height = 38,
                Width = 160,
                Margin = new Padding(12, 0, 0, 0),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnAceptar.FlatAppearance.BorderSize = 0;

            flAcciones.Controls.Add(btnCancelar);
            flAcciones.Controls.Add(btnAceptar);

            tlRoot.SetColumnSpan(flAcciones, 2);
            tlRoot.Controls.Add(flAcciones, 0, 2);

            this.Controls.Add(tlRoot);
            this.ResumeLayout(false);

            // Eventos
            txtBuscar.TextChanged += TxtBuscar_TextChanged;
            cbCategoria.SelectedIndexChanged += CbCategoria_SelectedIndexChanged;
            cbEstadoStock.SelectedIndexChanged += CbEstadoStock_SelectedIndexChanged;
            btnLimpiarFiltros.Click += BtnLimpiarFiltros_Click;
            dgvProductos.CellClick += DgvProductos_CellClick;
            dgvSeleccionados.CellClick += DgvSeleccionados_CellClick;
            dgvSeleccionados.CellValueChanged += DgvSeleccionados_CellValueChanged;
            btnAceptar.Click += BtnAceptar_Click;
        }

        private void CargarDatos()
        {
            try
            {
                // Cargar productos
                _todosLosProductos.Clear();
                _todosLosProductos.AddRange(_productoRepo.Listar(activo: true));

                // Cargar categorías
                var categorias = _todosLosProductos.Select(p => p.CategoriaNombre).Distinct().OrderBy(c => c).ToList();
                cbCategoria.Items.Clear();
                cbCategoria.Items.Add("Todas las categorias");
                cbCategoria.Items.AddRange(categorias.ToArray());
                cbCategoria.SelectedIndex = 0;

                // Aplicar filtros iniciales
                AplicarFiltros();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar productos: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AplicarFiltros()
        {
            try
            {
                var productos = _todosLosProductos.AsEnumerable();

                // Filtro por texto
                var busqueda = txtBuscar.Text?.Trim() ?? "";
                if (!string.IsNullOrEmpty(busqueda))
                {
                    productos = productos.Where(p =>
                        p.Nombre.ToLowerInvariant().Contains(busqueda.ToLowerInvariant()) ||
                        p.Sku.ToLowerInvariant().Contains(busqueda.ToLowerInvariant()) ||
                        p.Descripcion.ToLowerInvariant().Contains(busqueda.ToLowerInvariant()));
                }

                // Filtro por categoría
                if (cbCategoria.SelectedIndex > 0)
                {
                    var categoria = cbCategoria.SelectedItem.ToString();
                    productos = productos.Where(p => p.CategoriaNombre == categoria);
                }

                // Filtro por stock
                switch (cbEstadoStock.SelectedIndex)
                {
                    case 1: // Solo Disponible
                        productos = productos.Where(p => p.Stock > 0);
                        break;
                    case 2: // Stock Bajo
                        productos = productos.Where(p => p.Stock <= p.Minimo);
                        break;
                    // case 0: Todo Stock - no filtrar
                }

                _productosFiltrados.Clear();
                _productosFiltrados.AddRange(productos.OrderBy(p => p.Nombre));

                dgvProductos.DataSource = null;
                dgvProductos.DataSource = _productosFiltrados;

                lblResultados.Text = $"{_productosFiltrados.Count} productos encontrados";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al filtrar productos: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtBuscar_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltros();
        }

        private void CbCategoria_SelectedIndexChanged(object sender, EventArgs e)
        {
            AplicarFiltros();
        }

        private void CbEstadoStock_SelectedIndexChanged(object sender, EventArgs e)
        {
            AplicarFiltros();
        }

        private void BtnLimpiarFiltros_Click(object sender, EventArgs e)
        {
            txtBuscar.Clear();
            cbCategoria.SelectedIndex = 0;
            cbEstadoStock.SelectedIndex = 1; // "Solo Disponible"
            AplicarFiltros();
        }

        private void DgvProductos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (e.ColumnIndex != dgvProductos.Columns["Agregar"].Index) return;

            try
            {
                var producto = _productosFiltrados[e.RowIndex];
                
                // Verificar si ya está en la lista
                var existente = _productosParaVenta.FirstOrDefault(p => p.IdProducto == producto.IdProducto);
                
                if (existente != null)
                {
                    existente.Cantidad++;
                }
                else
                {
                    _productosParaVenta.Add(new ProductoVenta(producto));
                }
                
                ActualizarTotalSeleccionados();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al agregar producto: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvSeleccionados_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (e.ColumnIndex != dgvSeleccionados.Columns["Quitar"].Index) return;

            try
            {
                if (e.RowIndex < _productosParaVenta.Count)
                {
                    var producto = _productosParaVenta[e.RowIndex];
                    var resultado = MessageBox.Show(
                        $"¿Quitar '{producto.Producto}' de la selección?",
                        "Confirmar",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                        
                    if (resultado == DialogResult.Yes)
                    {
                        _productosParaVenta.RemoveAt(e.RowIndex);
                        ActualizarTotalSeleccionados();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al quitar producto: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvSeleccionados_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (e.ColumnIndex != dgvSeleccionados.Columns["Cantidad"].Index) return;

            try
            {
                var cell = dgvSeleccionados.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (int.TryParse(cell.Value?.ToString(), out int cantidad) && cantidad > 0)
                {
                    if (e.RowIndex < _productosParaVenta.Count)
                    {
                        _productosParaVenta[e.RowIndex].Cantidad = cantidad;
                        ActualizarTotalSeleccionados();
                    }
                }
                else
                {
                    // Restaurar valor anterior si es inválido
                    cell.Value = 1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar cantidad: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ActualizarTotalSeleccionados()
        {
            var total = _productosParaVenta.Sum(p => p.Subtotal);
            lblTotalSeleccionados.Text = $"Total: {total:C2}";
            btnAceptar.Enabled = _productosParaVenta.Count > 0;
        }

        private void BtnAceptar_Click(object sender, EventArgs e)
        {
            try
            {
                if (_productosParaVenta.Count == 0)
                {
                    MessageBox.Show("Debe seleccionar al menos un producto.", "Validación", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validar cantidades
                foreach (var producto in _productosParaVenta)
                {
                    if (producto.Cantidad <= 0)
                    {
                        MessageBox.Show($"La cantidad para '{producto.Producto}' debe ser mayor a 0.", 
                            "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    
                    // Verificar stock disponible
                    var productoOriginal = _todosLosProductos.FirstOrDefault(p => p.IdProducto == producto.IdProducto);
                    if (productoOriginal != null && producto.Cantidad > productoOriginal.Stock)
                    {
                        MessageBox.Show($"La cantidad solicitada para '{producto.Producto}' ({producto.Cantidad}) " +
                            $"excede el stock disponible ({productoOriginal.Stock}).", 
                            "Stock Insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Convertir a DetalleVenta
                ProductosSeleccionados.Clear();
                foreach (var producto in _productosParaVenta)
                {
                    ProductosSeleccionados.Add(new DetalleVenta
                    {
                        IdProducto = producto.IdProducto,
                        NombreProducto = producto.Producto,
                        SkuProducto = producto.Sku,
                        Cantidad = producto.Cantidad,
                        PrecioUnitario = producto.Precio
                    });
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al confirmar selección: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // Clase auxiliar para manejar productos en el grid de seleccionados
    public class ProductoVenta : INotifyPropertyChanged
    {
        private int _cantidad;
        
        public int IdProducto { get; set; }
        public string Producto { get; set; }
        public string Sku { get; set; }
        public decimal Precio { get; set; }
        
        public int Cantidad 
        { 
            get => _cantidad;
            set
            {
                if (_cantidad != value)
                {
                    _cantidad = value;
                    OnPropertyChanged(nameof(Cantidad));
                    OnPropertyChanged(nameof(Subtotal));
                }
            }
        }
        
        public decimal Subtotal => Cantidad * Precio;

        public ProductoVenta(Producto producto)
        {
            IdProducto = producto.IdProducto;
            Producto = producto.Nombre;
            Sku = producto.Sku;
            Precio = producto.Precio;
            Cantidad = 1;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}