using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Proyecto_Taller_2.Data;
using Proyecto_Taller_2.Domain.Models; // Cambiar a Models en lugar de Entities
using Proyecto_Taller_2.Data.Repositories;
using Proyecto_Taller_2.Forms;

namespace Proyecto_Taller_2.Controls
{
    public partial class UcProductos : UserControl
    {
        // Paleta de colores (consistente con el diseño actual)
        private readonly Color ColBg = Color.White;
        private readonly Color ColSoft = Color.FromArgb(246, 250, 246);
        private readonly Color ColSoftAlt = Color.FromArgb(236, 243, 236);
        private readonly Color ColText = Color.FromArgb(34, 47, 34);
        private readonly Color ColAccent = Color.FromArgb(34, 139, 34);
        private readonly Color ColBorder = Color.FromArgb(210, 220, 210);

        // Repositorios y datos - usando la nueva estructura
        private readonly ProductoRepository _repo;
        private readonly CategoriaRepository _categoriaRepo;
        private readonly BindingList<Producto> _productos = new BindingList<Producto>();
        private List<Producto> _allProductos = new List<Producto>();
        private List<Categoria> _categorias = new List<Categoria>();

        private DataGridView dgv;
        private Panel pnlDetails;
        private TextBox txtBuscar;
        private ComboBox cbCategoria, cbStock, cbEstado;
        private Button btnNuevo, btnImportar, btnExportar;

        public UcProductos()
        {
            try
            {
                // Inicializar repositorios con la nueva estructura
                _repo = new ProductoRepository(BDGeneral.ConnectionString);
                _categoriaRepo = new CategoriaRepository(BDGeneral.ConnectionString);
                
                this.AutoScaleMode = AutoScaleMode.Dpi;
                this.DoubleBuffered = true;
                this.Dock = DockStyle.Fill;
                this.BackColor = ColBg;
                
                BuildUI();
                WireEvents();
                
                // Cargar datos
                if (!DesignMode)
                {
                    this.Load += (s, e) => 
                    {
                        CargarCategorias();
                        RefrescarDatos();
                    };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inicializando control de productos: {ex.Message}", "Error de Inicialización", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // Crear interfaz mínima de error
                this.Controls.Clear();
                var errorLabel = new Label
                {
                    Text = $"Error: {ex.Message}",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.Red
                };
                this.Controls.Add(errorLabel);
            }
        }

        private void CargarCategorias()
        {
            try
            {
                _categorias = _categoriaRepo.ObtenerTodas(true); // Solo activas
                
                // Asegurar que el ComboBox existe antes de accederlo
                if (cbCategoria != null)
                {
                    cbCategoria.Items.Clear();
                    cbCategoria.Items.Add("Todas las categorías");
                    
                    foreach (var categoria in _categorias)
                    {
                        cbCategoria.Items.Add(categoria.Nombre);
                    }
                    
                    cbCategoria.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar categorías: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                
                // Fallback: crear lista vacía
                _categorias = new List<Categoria>();
                if (cbCategoria != null)
                {
                    cbCategoria.Items.Clear();
                    cbCategoria.Items.Add("Todas las categorías");
                    cbCategoria.SelectedIndex = 0;
                }
            }
        }

        private void BuildUI()
        {
            // Root con padding general
            var rootPad = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16) };
            Controls.Add(rootPad);

            var tlRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = ColBg
            };
            // CORREGIR: Ajustar las alturas para evitar superposiciones
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));   // Top bar - más espacio para título y botones
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));   // Filtros - altura fija apropiada
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100));    // Grid - resto del espacio
            rootPad.Controls.Add(tlRoot);

            // Panel superior para título y acciones - MEJORADO
            var topPanel = new Panel { Dock = DockStyle.Fill, BackColor = ColBg };
            
            // CORREGIR: Título con posición fija para evitar superposición
            var lblTitulo = new Label
            {
                Text = "Gestión de Productos",
                Font = new Font("Segoe UI", 18, FontStyle.Bold), // Reducir un poco el tamaño
                Location = new Point(8, 15), // Posición fija
                Size = new Size(400, 35),    // Tamaño fijo
                ForeColor = ColText,
                BackColor = ColBg
            };
            topPanel.Controls.Add(lblTitulo);

            // CORREGIR: Panel de botones con mejor posicionamiento
            var panelAcciones = new Panel
            {
                Location = new Point(600, 10), // Posición fija para evitar superposición
                Size = new Size(400, 45),      // Tamaño fijo
                BackColor = ColBg
            };
            
            // Crear botones con posiciones fijas
            btnNuevo = new Button 
            { 
                Text = "+ Nuevo Producto", 
                BackColor = Color.FromArgb(201, 222, 201), 
                ForeColor = Color.Black, 
                FlatStyle = FlatStyle.Flat, 
                Location = new Point(250, 5),
                Size = new Size(140, 35)
            };
            btnExportar = new Button 
            { 
                Text = "Exportar", 
                BackColor = Color.White, 
                ForeColor = Color.Black, 
                FlatStyle = FlatStyle.Flat,
                Location = new Point(140, 5),
                Size = new Size(100, 35)
            };
            btnImportar = new Button 
            { 
                Text = "Importar", 
                BackColor = Color.White, 
                ForeColor = Color.Black, 
                FlatStyle = FlatStyle.Flat,
                Location = new Point(30, 5),
                Size = new Size(100, 35)
            };
            
            btnNuevo.FlatAppearance.BorderSize = 0;
            btnExportar.FlatAppearance.BorderSize = 0;
            btnImportar.FlatAppearance.BorderSize = 0;
            
            panelAcciones.Controls.Add(btnNuevo);
            panelAcciones.Controls.Add(btnExportar);
            panelAcciones.Controls.Add(btnImportar);
            topPanel.Controls.Add(panelAcciones);
            
            // Agregar el panel superior al tlRoot
            tlRoot.Controls.Add(topPanel, 0, 0);

            // CORREGIR: Panel de filtros con altura adecuada
            var gbBuscar = new GroupBox
            {
                Text = "Buscar y Filtrar",
                Dock = DockStyle.Fill,
                Padding = new Padding(15, 30, 15, 15), // Más padding para el título
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = ColText,
                BackColor = ColBg
            };
            
            // TextBox de búsqueda
            txtBuscar = new TextBox 
            { 
                Text = "Buscar por SKU, nombre, descripción...", 
                Dock = DockStyle.Top, 
                Height = 25,
                Margin = new Padding(0, 0, 0, 10),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray
            };
            
            // Panel para los filtros (ComboBoxes)
            var panelFiltros = new Panel 
            { 
                Dock = DockStyle.Top, 
                Height = 40,
                BackColor = ColBg
            };
            
            // CORREGIR: ComboBoxes con posiciones y tamaños fijos
            cbCategoria = new ComboBox 
            { 
                Location = new Point(0, 8),
                Size = new Size(180, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            
            cbEstado = new ComboBox 
            { 
                Location = new Point(190, 8),
                Size = new Size(140, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cbEstado.Items.AddRange(new[] { "Todos", "Activo", "Inactivo" });
            cbEstado.SelectedIndex = 0;
            
            cbStock = new ComboBox 
            { 
                Location = new Point(340, 8),
                Size = new Size(140, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cbStock.Items.AddRange(new[] { "Todo Stock", "Disponible", "Stock Bajo", "Sin Stock" });
            cbStock.SelectedIndex = 0;
            
            panelFiltros.Controls.Add(cbCategoria);
            panelFiltros.Controls.Add(cbEstado);
            panelFiltros.Controls.Add(cbStock);
            
            gbBuscar.Controls.Add(panelFiltros);
            gbBuscar.Controls.Add(txtBuscar);
            
            // Agregar filtros al layout principal
            tlRoot.Controls.Add(gbBuscar, 0, 1);

            // CORREGIR: Grilla con padding apropiado
            var gbLista = new GroupBox
            {
                Text = "Lista de Productos",
                Dock = DockStyle.Fill,
                Padding = new Padding(15, 35, 15, 15),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = ColText,
                BackColor = ColBg
            };
            
            // DataGridView con configuración mejorada
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                ColumnHeadersHeight = 50,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                GridColor = ColBorder,
                EnableHeadersVisualStyles = false,
                AutoGenerateColumns = false,
                ScrollBars = ScrollBars.Both,
                ColumnHeadersVisible = true,
                CellBorderStyle = DataGridViewCellBorderStyle.Single,
                RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single
            };

            // Aplicar estilos del grid
            dgv.ColumnHeadersDefaultCellStyle.BackColor = ColSoftAlt;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = ColText;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);
            
            dgv.DefaultCellStyle.Padding = new Padding(6, 8, 6, 8);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 232, 220);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            dgv.RowTemplate.Height = 55;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 248);

            // Crear columnas con anchos apropiados
            CrearColumnasDataGridView();

            // Eventos del DataGridView
            dgv.CellPainting += Dgv_CellPainting;
            dgv.CellClick += Dgv_CellClick;
            dgv.CellFormatting += Dgv_CellFormatting;

            gbLista.Controls.Add(dgv);
            
            // Agregar grilla al layout principal
            tlRoot.Controls.Add(gbLista, 0, 2);
        }
        
        private void CrearColumnasDataGridView()
        {
            dgv.Columns.Clear();
            
            // Definir columnas con anchos optimizados
            var cImagen = new DataGridViewImageColumn 
            { 
                Name = "Imagen", 
                HeaderText = "", 
                Width = 60,
                MinimumWidth = 60,
                Resizable = DataGridViewTriState.False,
                ImageLayout = DataGridViewImageCellLayout.Zoom 
            };
            var cSku = new DataGridViewTextBoxColumn 
            { 
                Name = "Sku", 
                HeaderText = "SKU", 
                Width = 90,
                MinimumWidth = 80,
                DataPropertyName = "Sku"
            };
            var cNombre = new DataGridViewTextBoxColumn 
            { 
                Name = "Nombre", 
                HeaderText = "Nombre", 
                Width = 220,
                MinimumWidth = 180,
                DataPropertyName = "Nombre"
            };
            var cDescripcion = new DataGridViewTextBoxColumn
            {
                Name = "Descripcion",
                HeaderText = "Descripcion",
                Width = 220,
                MinimumWidth = 180,
                DataPropertyName = "Descripcion"
            };
            var cCategoria = new DataGridViewTextBoxColumn 
            { 
                Name = "Categoria", 
                HeaderText = "Categoría", 
                Width = 110,
                MinimumWidth = 90,
                DataPropertyName = "CategoriaNombre"
            };
            var cStock = new DataGridViewTextBoxColumn 
            { 
                Name = "Stock", 
                HeaderText = "Stock", 
                Width = 70,
                MinimumWidth = 60,
                DefaultCellStyle = new DataGridViewCellStyle { 
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                },
                DataPropertyName = "Stock"
            };
            var cEstadoStock = new DataGridViewTextBoxColumn 
            { 
                Name = "EstadoStock", 
                HeaderText = "Estado Stock", 
                Width = 120,
                MinimumWidth = 100,
                DataPropertyName = "EstadoStock"
            };
            var cPrecio = new DataGridViewTextBoxColumn 
            { 
                Name = "Precio", 
                HeaderText = "Precio", 
                Width = 100,
                MinimumWidth = 80,
                DefaultCellStyle = new DataGridViewCellStyle { 
                    Alignment = DataGridViewContentAlignment.MiddleRight, 
                    Format = "C2",
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                },
                DataPropertyName = "Precio"
            };
            var cEstado = new DataGridViewTextBoxColumn 
            { 
                Name = "Estado", 
                HeaderText = "Estado", 
                Width = 85,
                MinimumWidth = 75,
                DataPropertyName = "Activo"
            };
            var cAcciones = new DataGridViewButtonColumn 
            { 
                Name = "Acciones", 
                HeaderText = "Acciones", 
                Width = 85,
                MinimumWidth = 75,
                Text = "Editar",
                UseColumnTextForButtonValue = true,
                FlatStyle = FlatStyle.Flat
            };

            dgv.Columns.AddRange(new DataGridViewColumn[] 
            { 
                cImagen, cSku, cNombre,cDescripcion, cCategoria, cStock, cEstadoStock, cPrecio, cEstado, cAcciones 
            });
        }

        private void WireEvents()
        {
            // MEJORAR: Agregar debouncing a la búsqueda para mejor rendimiento
            Timer searchTimer = new Timer { Interval = 500 };
            searchTimer.Tick += (s, e) => 
            {
                searchTimer.Stop();
                AplicarFiltros();
            };
            
            txtBuscar.TextChanged += (s, e) => 
            {
                searchTimer.Stop();
                searchTimer.Start();
            };
            
            cbCategoria.SelectedIndexChanged += (s, e) => AplicarFiltros();
            cbEstado.SelectedIndexChanged += (s, e) => AplicarFiltros();
            cbStock.SelectedIndexChanged += (s, e) => AplicarFiltros();
            
            btnNuevo.Click += BtnNuevo_Click;
            btnImportar.Click += BtnImportar_Click;
            btnExportar.Click += BtnExportar_Click;
            
            txtBuscar.Enter += (s, e) =>
            {
                if (txtBuscar.Text == "Buscar por SKU, nombre, descripción...")
                {
                    txtBuscar.Text = "";
                    txtBuscar.ForeColor = Color.Black;
                }
            };
            
            txtBuscar.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtBuscar.Text))
                {
                    txtBuscar.Text = "Buscar por SKU, nombre, descripción...";
                    txtBuscar.ForeColor = Color.Gray;
                }
            };
        }

        private void RefrescarDatos()
        {
            try
            {
                // MEJORAR: Mostrar indicador de carga
                this.Cursor = Cursors.WaitCursor;
                
                _allProductos = _repo.Listar();
                AplicarFiltros();
                
                // Asegurar configuración correcta del DataGridView después de cargar datos
                if (dgv.Rows.Count > 0)
                {
                    dgv.ClearSelection(); // Limpiar selección inicial
                    AjustarAnchoColumnas(); // Aplicar anchos correctos
                    AplicarEstilosPersonalizados(); // Aplicar estilos adicionales
                }
                
                this.Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show($"Error al cargar productos: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AplicarEstilosPersonalizados()
        {
            // Configurar estilos adicionales para mejor apariencia
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = dgv.ColumnHeadersDefaultCellStyle.BackColor;
            dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = dgv.ColumnHeadersDefaultCellStyle.ForeColor;
            
            // Asegurar que las filas alternadas tengan el color correcto
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 248);
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 232, 220);
            
            // Mejorar la apariencia de la selección
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 225, 200);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            
            // Forzar un refresh para aplicar los cambios
            dgv.Invalidate();
        }

        private void AplicarFiltros()
        {
            try
            {
                var filtrados = _allProductos.AsEnumerable();
                
                // Filtro de búsqueda
                var buscar = txtBuscar?.Text?.Trim() ?? "";
                if (!string.IsNullOrEmpty(buscar) && buscar != "Buscar por SKU, nombre, descripción...")
                {
                    filtrados = filtrados.Where(p =>
                        (!string.IsNullOrEmpty(p.Sku) && p.Sku.ToLowerInvariant().Contains(buscar.ToLowerInvariant())) ||
                        (!string.IsNullOrEmpty(p.Nombre) && p.Nombre.ToLowerInvariant().Contains(buscar.ToLowerInvariant())) ||
                        (!string.IsNullOrEmpty(p.Descripcion) && p.Descripcion.ToLowerInvariant().Contains(buscar.ToLowerInvariant())) ||
                        (!string.IsNullOrEmpty(p.Proveedor) && p.Proveedor.ToLowerInvariant().Contains(buscar.ToLowerInvariant())));
                }
                
                // Filtro de categoría
                if (cbCategoria != null && cbCategoria.SelectedIndex > 0 && cbCategoria.SelectedItem != null)
                {
                    var categoriaNombre = cbCategoria.SelectedItem.ToString();
                    filtrados = filtrados.Where(p => 
                        !string.IsNullOrEmpty(p.CategoriaNombre) && 
                        p.CategoriaNombre.Equals(categoriaNombre, StringComparison.OrdinalIgnoreCase));
                }
                
                // Filtro de estado
                if (cbEstado != null)
                {
                    switch (cbEstado.SelectedIndex)
                    {
                        case 1: // Activo
                            filtrados = filtrados.Where(p => p.Activo);
                            break;
                        case 2: // Inactivo
                            filtrados = filtrados.Where(p => !p.Activo);
                            break;
                    }
                }
                
                // Filtro de stock
                if (cbStock != null)
                {
                    switch (cbStock.SelectedIndex)
                    {
                        case 1: // Disponible
                            filtrados = filtrados.Where(p => !p.TieneBajoStock && !p.SinStock);
                            break;
                        case 2: // Stock Bajo
                            filtrados = filtrados.Where(p => p.TieneBajoStock);
                            break;
                        case 3: // Sin Stock
                            filtrados = filtrados.Where(p => p.SinStock);
                            break;
                    }
                }
                
                // MEJORAR: Actualizar datos de forma más eficiente
                var productosOrdenados = filtrados.OrderBy(p => p.Nombre ?? "").ToList();
                
                // Pausar eventos temporalmente
                dgv.SuspendLayout();
                
                // Actualizar binding de forma más segura
                _productos.RaiseListChangedEvents = false;
                _productos.Clear();
                
                foreach (var producto in productosOrdenados)
                {
                    _productos.Add(producto);
                }
                
                _productos.RaiseListChangedEvents = true;
                
                // Configurar DataSource si no está configurado
                if (dgv.DataSource != _productos)
                {
                    dgv.DataSource = null; // Limpiar primero
                    dgv.DataSource = _productos;
                }
                
                _productos.ResetBindings();
                
                // Reanudar layout
                dgv.ResumeLayout();
                
                // Asegurar que las columnas mantengan su ancho después del refresh
                this.BeginInvoke((Action)(() =>
                {
                    AjustarAnchoColumnas();
                }));
            }
            catch (Exception ex)
            {
                // Mostrar error pero no romper la aplicación
                MessageBox.Show($"Error al aplicar filtros: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                
                // Restaurar a lista completa en caso de error
                try
                {
                    _productos.Clear();
                    foreach (var producto in _allProductos)
                    {
                        _productos.Add(producto);
                    }
                }
                catch
                {
                    // Ignorar errores de recuperación
                }
            }
        }

        private void AjustarAnchoColumnas()
        {
            try
            {
                if (dgv.Columns.Count > 0)
                {
                    // Forzar el ancho específico de cada columna después de cambios de datos
                    if (dgv.Columns.Contains("Imagen")) dgv.Columns["Imagen"].Width = 60;
                    if (dgv.Columns.Contains("Sku")) dgv.Columns["Sku"].Width = 90;
                    if (dgv.Columns.Contains("Nombre")) dgv.Columns["Nombre"].Width = 220;
                    if (dgv.Columns.Contains("Descripcion")) dgv.Columns["Descripcion"].Width = 220;
                    if (dgv.Columns.Contains("Categoria")) dgv.Columns["Categoria"].Width = 110;
                    if (dgv.Columns.Contains("Stock")) dgv.Columns["Stock"].Width = 70;
                    if (dgv.Columns.Contains("EstadoStock")) dgv.Columns["EstadoStock"].Width = 120;
                    if (dgv.Columns.Contains("Precio")) dgv.Columns["Precio"].Width = 100;
                    if (dgv.Columns.Contains("Estado")) dgv.Columns["Estado"].Width = 85;
                    if (dgv.Columns.Contains("Acciones")) dgv.Columns["Acciones"].Width = 85;
                }
            }
            catch (Exception ex)
            {
                // Log del error para debugging (opcional)
                System.Diagnostics.Debug.WriteLine($"Error al ajustar anchos de columnas: {ex.Message}");
                // No mostrar error al usuario ya que es un detalle visual
            }
        }

        private void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Validaciones más robustas para evitar IndexOutOfRangeException
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (e.RowIndex >= _productos.Count) return;
            if (e.ColumnIndex >= dgv.Columns.Count) return;
            
            try
            {
                var producto = _productos[e.RowIndex];
                if (producto == null) return;

                // Imagen placeholder
                if (dgv.Columns[e.ColumnIndex].Name == "Imagen")
                {
                    e.Value = MakePlaceholderImage(producto.Nombre ?? "?");
                    e.FormattingApplied = true;
                    return;
                }
                
                // Estado
                if (dgv.Columns[e.ColumnIndex].Name == "Estado")
                {
                    if (e.Value is bool activo)
                    {
                        e.Value = activo ? "Activo" : "Inactivo";
                        e.FormattingApplied = true;
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                // Log del error para debugging (opcional)
                System.Diagnostics.Debug.WriteLine($"Error en CellFormatting: {ex.Message}");
                // No re-lanzar la excepción para evitar que se rompa la UI
            }
        }

        private void Dgv_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // Validaciones más robustas
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (e.RowIndex >= _productos.Count) return;
            if (e.ColumnIndex >= dgv.Columns.Count) return;

            try
            {
                var columnName = dgv.Columns[e.ColumnIndex].Name;

                // Estado (pintado especial)
                if (columnName == "Estado")
                {
                    e.Handled = true;
                    e.PaintBackground(e.ClipBounds, true);
                    string text = Convert.ToString(e.FormattedValue ?? "");
                    Color bg = text.Equals("Activo", StringComparison.OrdinalIgnoreCase) ? 
                              Color.FromArgb(34, 139, 34) : Color.FromArgb(200, 180, 80);
                    DrawChip(e.Graphics, e.CellBounds, text, bg, Color.White, 12);
                    return;
                }
                
                // Estado Stock (pintado especial)
                if (columnName == "EstadoStock")
                {
                    e.Handled = true;
                    e.PaintBackground(e.ClipBounds, true);
                    string text = Convert.ToString(e.FormattedValue ?? "");
                    Color bg;
                    switch (text)
                    {
                        case "Disponible":
                            bg = Color.FromArgb(34, 139, 34);
                            break;
                        case "Stock Bajo":
                            bg = Color.FromArgb(255, 165, 0);
                            break;
                        case "Sin Stock":
                            bg = Color.FromArgb(220, 53, 69);
                            break;
                        default:
                            bg = Color.Gray;
                            break;
                    }
                    DrawChip(e.Graphics, e.CellBounds, text, bg, Color.White, 12);
                    return;
                }
            }
            catch (Exception ex)
            {
                // Log del error para debugging (opcional)
                System.Diagnostics.Debug.WriteLine($"Error en CellPainting: {ex.Message}");
                // No re-lanzar la excepción para evitar que se rompa la UI
            }
        }

        private void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Validaciones más robustas
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (e.RowIndex >= _productos.Count) return;
            if (e.ColumnIndex >= dgv.Columns.Count) return;

            try
            {
                if (dgv.Columns[e.ColumnIndex].Name == "Acciones")
                {
                    var producto = _productos[e.RowIndex];
                    if (producto != null)
                    {
                        EditarProducto(producto);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al procesar la acción: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnNuevo_Click(object sender, EventArgs e)
        {
            try
            {
                using (var form = new ProductoForm())
                {
                    if (form.ShowDialog() == DialogResult.OK && form.Resultado != null)
                    {
                        try
                        {
                            _repo.Agregar(form.Resultado);
                            MessageBox.Show("Producto agregado correctamente.", "Éxito", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            
                            // MEJORAR: Refrescar datos y mantener filtros actuales
                            RefrescarDatos();
                            
                            // Opcional: Seleccionar el nuevo producto en la lista
                            var nuevoProducto = _productos.FirstOrDefault(p => 
                                p.Sku == form.Resultado.Sku || 
                                p.Nombre == form.Resultado.Nombre);
                            

                            if (nuevoProducto != null)
                            {
                                var index = _productos.IndexOf(nuevoProducto);
                                if (index >= 0 && index < dgv.Rows.Count)
                                {
                                    dgv.ClearSelection();
                                    dgv.Rows[index].Selected = true;
                                    dgv.FirstDisplayedScrollingRowIndex = index;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error al agregar producto: {ex.Message}", "Error", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir formulario de producto: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditarProducto(Producto producto)
        {
            try
            {
                using (var form = new ProductoForm(producto))
                {
                    if (form.ShowDialog() == DialogResult.OK && form.Resultado != null)
                    {
                        try
                        {
                            form.Resultado.IdProducto = producto.IdProducto;
                            _repo.Actualizar(form.Resultado);
                            MessageBox.Show("Producto actualizado correctamente.", "Éxito", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            
                            // MEJORAR: Refrescar datos y mantener selección
                            var selectedIndex = dgv.SelectedRows.Count > 0 ? dgv.SelectedRows[0].Index : -1;
                            RefrescarDatos();
                            
                            // Restaurar selección si es posible
                            if (selectedIndex >= 0 && selectedIndex < dgv.Rows.Count)
                            {
                                dgv.ClearSelection();
                                dgv.Rows[selectedIndex].Selected = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error al actualizar producto: {ex.Message}", "Error", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir formulario de edición: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnImportar_Click(object sender, EventArgs e)
        {
            try
            {
                using (var openDialog = new OpenFileDialog())
                {
                    openDialog.Filter = "Archivos CSV (*.csv)|*.csv";
                    openDialog.Title = "Seleccionar archivo CSV para importar";

                    if (openDialog.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            ImportarCSV(openDialog.FileName);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error durante la importación: {ex.Message}", "Error", 
                                          MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir selector de archivos: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ImportarCSV(string archivo)
        {
            var lineas = File.ReadAllLines(archivo, Encoding.UTF8);
            if (lineas.Length <= 1)
            {
                MessageBox.Show("El archivo está vacío o no tiene datos.", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var importados = 0;
            var errores = new List<string>();

            // Saltar header
            for (int i = 1; i < lineas.Length; i++)
            {
                try
                {
                    var campos = lineas[i].Split(',');
                    if (campos.Length < 5) continue;

                    // Buscar categoría con búsqueda más flexible
                    var categoriaNombre = campos[3].Trim('"');
                    var categoria = BuscarCategoria(categoriaNombre);
                    if (categoria == null)
                    {
                        errores.Add($"Línea {i + 1}: Categoría '{categoriaNombre}' no encontrada");
                        continue;
                    }

                    var producto = new Producto
                    {
                        Sku = campos[0].Trim('"'),
                        Nombre = campos[1].Trim('"'),
                        Descripcion = campos[2].Trim('"'),
                        IdCategoria = categoria.IdCategoria,
                        CategoriaNombre = categoria.Nombre,
                        Precio = decimal.Parse(campos[4].Trim('"'), CultureInfo.InvariantCulture),
                        Stock = campos.Length > 5 ? int.Parse(campos[5].Trim('"')) : 0,
                        Minimo = campos.Length > 6 ? int.Parse(campos[6].Trim('"')) : 5,
                        Ubicacion = campos.Length > 7 ? campos[7].Trim('"') : "",
                        Proveedor = campos.Length > 8 ? campos[8].Trim('"') : "",
                        Activo = campos.Length > 9 ? campos[9].Trim('"').Equals("Activo", StringComparison.OrdinalIgnoreCase) : true,
                        FechaAlta = DateTime.Now,
                        Actualizado = DateTime.Now
                    };

                    // Validar SKU único
                    if (!string.IsNullOrEmpty(producto.Sku) && _repo.ExisteSku(producto.Sku))
                    {
                        errores.Add($"Línea {i + 1}: SKU '{producto.Sku}' ya existe");
                        continue;
                    }

                    _repo.Agregar(producto);
                    importados++;
                }
                catch (Exception ex)
                {
                    errores.Add($"Línea {i + 1}: {ex.Message}");
                }
            }

            // Mostrar resultado con información detallada sobre categorías
            MostrarResultadoImportacion(importados, errores);
            
            // MEJORAR: Refrescar datos después de importar
            RefrescarDatos();
        }

        private void MostrarResultadoImportacion(int importados, List<string> errores)
        {
            var form = new Form
            {
                Text = "Resultado de Importación",
                Size = new Size(700, 600),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16) };
            form.Controls.Add(panel);

            var yPos = 10;

            // Resultado general
            var lblResultado = new Label
            {
                Text = $"Productos importados: {importados}\nErrores: {errores.Count}",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, yPos),
                ForeColor = importados > 0 ? Color.Green : Color.Red
            };
            panel.Controls.Add(lblResultado);
            yPos += 60;

            if (errores.Any())
            {
                // Información sobre categorías disponibles
                var lblCategorias = new Label
                {
                    Text = "Categorías disponibles en el sistema:",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(0, yPos),
                    ForeColor = Color.Blue
                };
                panel.Controls.Add(lblCategorias);
                yPos += 25;

                var categoriasTexto = string.Join(", ", _categorias.Select(c => $"'{c.Nombre}'"));
                var lblCategoriasList = new Label
                {
                    Text = categoriasTexto,
                    AutoSize = true,
                    MaximumSize = new Size(650, 0),
                    Location = new Point(0, yPos),
                    ForeColor = Color.DarkBlue
                };
                panel.Controls.Add(lblCategoriasList);
                yPos += lblCategoriasList.PreferredHeight + 20;

                // Sugerencias de mapeo automático
                var categoriasCSV = ExtraerCategoriasDelCSV(errores);
                if (categoriasCSV.Any())
                {
                    var lblSugerencias = new Label
                    {
                        Text = "Sugerencias de mapeo automático:",
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        AutoSize = true,
                        Location = new Point(0, yPos),
                        ForeColor = Color.DarkGreen
                    };
                    panel.Controls.Add(lblSugerencias);
                    yPos += 25;

                    var sugerenciasTexto = GenerarSugerenciasMapeo(categoriasCSV);
                    var lblSugerenciasList = new Label
                    {
                        Text = sugerenciasTexto,
                        AutoSize = true,
                        MaximumSize = new Size(650, 0),
                        Location = new Point(0, yPos),
                        ForeColor = Color.DarkGreen
                    };
                    panel.Controls.Add(lblSugerenciasList);
                    yPos += lblSugerenciasList.PreferredHeight + 20;
                }

                // Lista de errores
                var lblErrores = new Label
                {
                    Text = "Errores encontrados:",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(0, yPos),
                    ForeColor = Color.Red
                };
                panel.Controls.Add(lblErrores);
                yPos += 25;

                var txtErrores = new TextBox
                {
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    Location = new Point(0, yPos),
                    Size = new Size(650, 200),
                    Text = string.Join(Environment.NewLine, errores.Take(20))
                };
                
                if (errores.Count > 20)
                {
                    txtErrores.Text += Environment.NewLine + $"... y {errores.Count - 20} errores más.";
                }
                
                panel.Controls.Add(txtErrores);
                yPos += 220;
            }

            // Botones
            var btnAceptar = new Button
            {
                Text = "Aceptar",
                DialogResult = DialogResult.OK,
                Size = new Size(100, 30),
                Location = new Point(570, yPos)
            };
            panel.Controls.Add(btnAceptar);

            form.AcceptButton = btnAceptar;
            form.ShowDialog(this);
        }

        private List<string> ExtraerCategoriasDelCSV(List<string> errores)
        {
            var categorias = new HashSet<string>();
            foreach (var error in errores)
            {
                if (error.Contains("Categoría") && error.Contains("no encontrada"))
                {
                    // Extraer el nombre de la categoría del mensaje de error
                    var inicio = error.IndexOf("'") + 1;
                    var fin = error.LastIndexOf("'");
                    if (inicio > 0 && fin > inicio)
                    {
                        var categoria = error.Substring(inicio, fin - inicio);
                        categorias.Add(categoria);
                    }
                }
            }
            return categorias.ToList();
        }

        private string GenerarSugerenciasMapeo(List<string> categoriasCSV)
        {
            var sugerencias = new List<string>();
            
            foreach (var categoriaCSV in categoriasCSV)
            {
                var mejorCoincidencia = _categorias
                    .OrderBy(c => CalcularDistanciaLevenshtein(c.Nombre.ToLowerInvariant(), categoriaCSV.ToLowerInvariant()))
                    .FirstOrDefault();
                
                if (mejorCoincidencia != null)
                {
                    sugerencias.Add($"'{categoriaCSV}' → '{mejorCoincidencia.Nombre}'");
                }
            }
            
            return string.Join(Environment.NewLine, sugerencias);
        }

        private int CalcularDistanciaLevenshtein(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0) return m;
            if (m == 0) return n;

            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }
            
            return d[n, m];
        }
        
        private Categoria BuscarCategoria(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre) || _categorias == null || !_categorias.Any())
                return null;

            // Búsqueda exacta (case insensitive)
            var categoria = _categorias.FirstOrDefault(c => 
                string.Equals(c.Nombre, nombre, StringComparison.OrdinalIgnoreCase));
            
            if (categoria != null)
                return categoria;

            // Búsqueda por similitud (quitar acentos, espacios, etc.)
            var nombreNormalizado = NormalizarTexto(nombre);
            categoria = _categorias.FirstOrDefault(c => 
                string.Equals(NormalizarTexto(c.Nombre), nombreNormalizado, StringComparison.OrdinalIgnoreCase));
            
            if (categoria != null)
                return categoria;

            // Búsqueda parcial (contiene)
            categoria = _categorias.FirstOrDefault(c => 
                c.Nombre.IndexOf(nombre, StringComparison.OrdinalIgnoreCase) >= 0 ||
                nombre.IndexOf(c.Nombre, StringComparison.OrdinalIgnoreCase) >= 0);

            return categoria;
        }

        private string NormalizarTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            // Convertir a minúsculas y quitar espacios
            return texto.ToLowerInvariant()
                       .Trim()
                       .Replace(" ", "")
                       .Replace("á", "a")
                       .Replace("é", "e")
                       .Replace("í", "i")
                       .Replace("ó", "o")
                       .Replace("ú", "u")
                       .Replace("ñ", "n");
        }

        private void BtnExportar_Click(object sender, EventArgs e)
        {
            try
            {
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "Archivos CSV (*.csv)|*.csv";
                    saveDialog.Title = "Guardar archivo de exportación";
                    saveDialog.FileName = $"productos_{DateTime.Now:yyyyMMdd}.csv";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            ExportarCSV(saveDialog.FileName);
                            MessageBox.Show("Productos exportados correctamente.", "Éxito", 
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error durante la exportación: {ex.Message}", "Error", 
                                          MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir selector de archivos: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UcProductos_Load(object sender, EventArgs e)
        {

        }

        private void ExportarCSV(string archivo)
        {
            var lineas = new List<string>
            {
                "SKU,Nombre,Descripcion,Categoria,Precio,Stock,Minimo,Ubicacion,Proveedor,Estado"
            };
            
            foreach (var producto in _productos)
            {
                lineas.Add($"\"{producto.Sku}\",\"{producto.Nombre}\",\"{producto.Descripcion}\"," +
                          $"\"{producto.CategoriaNombre}\",\"{producto.Precio}\"," +
                          $"\"{producto.Stock}\",\"{producto.Minimo}\"," +
                          $"\"{producto.Ubicacion}\",\"{producto.Proveedor}\"," +
                          $"\"{(producto.Activo ? "Activo" : "Inactivo")}\"");
            }
            
            File.WriteAllLines(archivo, lineas, Encoding.UTF8);
        }

        // Helpers para UI
        private Image MakePlaceholderImage(string nombre)
        {
            int size = 48;
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.FromArgb(236, 243, 236));
                
                using (var p = new Pen(Color.FromArgb(201, 222, 201)))
                    g.DrawRectangle(p, 0, 0, size - 1, size - 1);

                var initial = nombre?.Substring(0, 1).ToUpper() ?? "?";
                using (var f = new Font("Segoe UI", 16, FontStyle.Bold))
                using (var b = new SolidBrush(Color.FromArgb(34, 139, 34)))
                {
                    var sz = g.MeasureString(initial, f);
                    g.DrawString(initial, f, b, 
                        (size - sz.Width) / 2, 
                        (size - sz.Height) / 2);
                }
            }
            return bmp;
        }

        private void DrawChip(Graphics g, Rectangle cell, string text, Color bg, Color fg, int radius)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var font = new Font("Segoe UI", 9f, FontStyle.Bold);
            var sz = TextRenderer.MeasureText(text, font);
            int padX = 12, padY = 6;
            int w = Math.Min(cell.Width - 12, sz.Width + padX * 2);
            int h = Math.Min(cell.Height - 12, sz.Height + padY * 2);

            int x = cell.X + (cell.Width - w) / 2;
            int y = cell.Y + (cell.Height - h) / 2;

            using (var path = RoundedRect(new Rectangle(x, y, w, h), radius))
            using (var sb = new SolidBrush(bg))
            {
                g.FillPath(sb, path);
            }

            var textRect = new Rectangle(x, y, w, h);
            TextRenderer.DrawText(g, text, font, textRect, fg,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}