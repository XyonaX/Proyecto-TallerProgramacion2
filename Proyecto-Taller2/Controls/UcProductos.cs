using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Proyecto_Taller_2.Data;
using Proyecto_Taller_2.Domain.Models;
using Proyecto_Taller_2.Data.Repositories;
using Proyecto_Taller_2.Forms;

namespace Proyecto_Taller_2.Controls
{
    public partial class UcProductos : UserControl
    {
        private readonly Color ColBg = Color.White;
        private readonly Color ColSoft = Color.FromArgb(246, 250, 246);
        private readonly Color ColSoftAlt = Color.FromArgb(236, 243, 236);
        private readonly Color ColText = Color.FromArgb(34, 47, 34);
        private readonly Color ColBorder = Color.FromArgb(210, 220, 210);

        private readonly Color ColAccent = Color.FromArgb(34, 139, 34);
        private readonly ProductoRepository _repo;
        private readonly CategoriaRepository _categoriaRepo;
        private readonly BindingList<Producto> _productos = new BindingList<Producto>();
        private List<Producto> _allProductos = new List<Producto>();
        private List<Categoria> _categorias = new List<Categoria>();

        private DataGridView dgv;
        private TextBox txtBuscar;
        private ComboBox cbCategoria, cbStock, cbEstado;
        private Button btnNuevo, btnImportar, btnExportar;

        public UcProductos()
        {
            try
            {
                _repo = new ProductoRepository(BDGeneral.ConnectionString);
                _categoriaRepo = new CategoriaRepository(BDGeneral.ConnectionString);
                this.AutoScaleMode = AutoScaleMode.Dpi;
                this.DoubleBuffered = true;
                this.Dock = DockStyle.Fill;
                this.BackColor = ColBg;
                BuildUI();
                WireEvents();
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
                MessageBox.Show($"Error inicializando: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarCategorias()
        {
            try
            {
                _categorias = _categoriaRepo.ObtenerTodas(true);
                if (cbCategoria != null)
                {
                    cbCategoria.Items.Clear();
                    cbCategoria.Items.Add("Todas las categorías");
                    foreach (var categoria in _categorias) cbCategoria.Items.Add(categoria.Nombre);
                    cbCategoria.SelectedIndex = 0;
                }
            }
            catch
            {
                _categorias = new List<Categoria>();
            }
        }

        private void BuildUI()
        {
            var rootPad = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16) };
            Controls.Add(rootPad);
            var tlRoot = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = ColBg };
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            rootPad.Controls.Add(tlRoot);

            var topPanel = new Panel { Dock = DockStyle.Fill, BackColor = ColBg };
            topPanel.Controls.Add(new Label { Text = "Gestión de Productos", Font = new Font("Segoe UI", 18, FontStyle.Bold), Location = new Point(8, 15), Size = new Size(400, 35), ForeColor = ColText, BackColor = ColBg });
            var panelAcciones = new Panel { Location = new Point(600, 10), Size = new Size(400, 45), BackColor = ColBg };
            btnNuevo = new Button { Text = "+ Nuevo Producto", BackColor = Color.FromArgb(201, 222, 201), ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Location = new Point(250, 5), Size = new Size(140, 35) };
            btnExportar = new Button { Text = "Exportar", BackColor = Color.White, ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Location = new Point(140, 5), Size = new Size(100, 35) };
            btnImportar = new Button { Text = "Importar", BackColor = Color.White, ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Location = new Point(30, 5), Size = new Size(100, 35) };
            btnNuevo.FlatAppearance.BorderSize = 0; btnExportar.FlatAppearance.BorderSize = 0; btnImportar.FlatAppearance.BorderSize = 0;
            panelAcciones.Controls.AddRange(new Control[] { btnNuevo, btnExportar, btnImportar });
            topPanel.Controls.Add(panelAcciones);
            tlRoot.Controls.Add(topPanel, 0, 0);

            var gbBuscar = new GroupBox { Text = "Buscar y Filtrar", Dock = DockStyle.Fill, Padding = new Padding(15, 30, 15, 15), Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = ColText, BackColor = ColBg };
            txtBuscar = new TextBox { Text = "Buscar por SKU, nombre, descripción...", Dock = DockStyle.Top, Height = 25, Margin = new Padding(0, 0, 0, 10), Font = new Font("Segoe UI", 9), ForeColor = Color.Gray };
            var panelFiltros = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = ColBg };
            cbCategoria = new ComboBox { Location = new Point(0, 8), Size = new Size(180, 25), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            cbEstado = new ComboBox { Location = new Point(190, 8), Size = new Size(140, 25), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            cbEstado.Items.AddRange(new[] { "Todos", "Activo", "Inactivo" }); cbEstado.SelectedIndex = 0;
            cbStock = new ComboBox { Location = new Point(340, 8), Size = new Size(140, 25), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            cbStock.Items.AddRange(new[] { "Todo Stock", "Disponible", "Stock Bajo", "Sin Stock" }); cbStock.SelectedIndex = 0;
            panelFiltros.Controls.AddRange(new Control[] { cbCategoria, cbEstado, cbStock });
            gbBuscar.Controls.AddRange(new Control[] { panelFiltros, txtBuscar });
            tlRoot.Controls.Add(gbBuscar, 0, 1);

            var gbLista = new GroupBox { Text = "Lista de Productos", Dock = DockStyle.Fill, Padding = new Padding(15, 35, 15, 15), Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = ColText, BackColor = ColBg };
            dgv = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false, RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None, BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle, ColumnHeadersHeight = 50, ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing, GridColor = ColBorder, EnableHeadersVisualStyles = false, AutoGenerateColumns = false, ColumnHeadersVisible = true, CellBorderStyle = DataGridViewCellBorderStyle.Single, RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = ColSoftAlt; dgv.ColumnHeadersDefaultCellStyle.ForeColor = ColText; dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold); dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);
            dgv.DefaultCellStyle.Padding = new Padding(6, 8, 6, 8); dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 232, 220); dgv.DefaultCellStyle.SelectionForeColor = Color.Black; dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9); dgv.RowTemplate.Height = 55; dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 248);
            CrearColumnasDataGridView();
            dgv.CellPainting += Dgv_CellPainting; dgv.CellClick += Dgv_CellClick; dgv.CellFormatting += Dgv_CellFormatting;
            gbLista.Controls.Add(dgv);
            tlRoot.Controls.Add(gbLista, 0, 2);
        }

        private void CrearColumnasDataGridView()
        {
            dgv.Columns.Clear();
            var cImagen = new DataGridViewImageColumn { Name = "Imagen", HeaderText = "", Width = 60, MinimumWidth = 60, Resizable = DataGridViewTriState.False, ImageLayout = DataGridViewImageCellLayout.Zoom };
            var cSku = new DataGridViewTextBoxColumn { Name = "Sku", HeaderText = "SKU", Width = 90, MinimumWidth = 80, DataPropertyName = "Sku" };
            var cNombre = new DataGridViewTextBoxColumn { Name = "Nombre", HeaderText = "Nombre", Width = 200, MinimumWidth = 150, DataPropertyName = "Nombre" };

            // NUEVA COLUMNA: PROVEEDOR
            var cProveedor = new DataGridViewTextBoxColumn { Name = "Proveedor", HeaderText = "Proveedor", Width = 120, MinimumWidth = 100, DataPropertyName = "Proveedor" };

            var cCategoria = new DataGridViewTextBoxColumn { Name = "Categoria", HeaderText = "Categoría", Width = 110, MinimumWidth = 90, DataPropertyName = "CategoriaNombre" };
            var cStock = new DataGridViewTextBoxColumn { Name = "Stock", HeaderText = "Stock", Width = 70, MinimumWidth = 60, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9, FontStyle.Bold) }, DataPropertyName = "Stock" };
            var cEstadoStock = new DataGridViewTextBoxColumn { Name = "EstadoStock", HeaderText = "Estado Stock", Width = 120, MinimumWidth = 100, DataPropertyName = "EstadoStock" };

            // NUEVA COLUMNA: COSTO
            var cCosto = new DataGridViewTextBoxColumn { Name = "Costo", HeaderText = "Costo", Width = 90, MinimumWidth = 70, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "C2", Font = new Font("Segoe UI", 9, FontStyle.Regular) }, DataPropertyName = "Costo" };

            var cPrecio = new DataGridViewTextBoxColumn { Name = "Precio", HeaderText = "Precio", Width = 90, MinimumWidth = 70, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "C2", Font = new Font("Segoe UI", 9, FontStyle.Bold) }, DataPropertyName = "Precio" };
            var cEstado = new DataGridViewTextBoxColumn { Name = "Estado", HeaderText = "Estado", Width = 85, MinimumWidth = 75, DataPropertyName = "Activo" };
            var cAcciones = new DataGridViewButtonColumn { Name = "Acciones", HeaderText = "Acciones", Width = 80, MinimumWidth = 70, Text = "Editar", UseColumnTextForButtonValue = true, FlatStyle = FlatStyle.Flat };

            // Agregamos las nuevas columnas al rango
            dgv.Columns.AddRange(new DataGridViewColumn[] { cImagen, cSku, cNombre, cProveedor, cCategoria, cStock, cEstadoStock, cCosto, cPrecio, cEstado, cAcciones });
        }

        private void WireEvents()
        {
            Timer searchTimer = new Timer { Interval = 500 };
            searchTimer.Tick += (s, e) => { searchTimer.Stop(); AplicarFiltros(); };
            txtBuscar.TextChanged += (s, e) => { searchTimer.Stop(); searchTimer.Start(); };
            cbCategoria.SelectedIndexChanged += (s, e) => AplicarFiltros();
            cbEstado.SelectedIndexChanged += (s, e) => AplicarFiltros();
            cbStock.SelectedIndexChanged += (s, e) => AplicarFiltros();
            btnNuevo.Click += BtnNuevo_Click;
            btnImportar.Click += BtnImportar_Click;
            btnExportar.Click += BtnExportar_Click;
            txtBuscar.Enter += (s, e) => { if (txtBuscar.Text == "Buscar por SKU, nombre, descripción...") { txtBuscar.Text = ""; txtBuscar.ForeColor = Color.Black; } };
            txtBuscar.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(txtBuscar.Text)) { txtBuscar.Text = "Buscar por SKU, nombre, descripción..."; txtBuscar.ForeColor = Color.Gray; } };
        }

        private void RefrescarDatos()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                _allProductos = _repo.Listar();
                AplicarFiltros();
                if (dgv.Rows.Count > 0) { dgv.ClearSelection(); AjustarAnchoColumnas(); AplicarEstilosPersonalizados(); }
            }
            catch (Exception ex) { MessageBox.Show($"Error al cargar productos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { this.Cursor = Cursors.Default; }
        }

        private void AplicarEstilosPersonalizados()
        {
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = dgv.ColumnHeadersDefaultCellStyle.BackColor;
            dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = dgv.ColumnHeadersDefaultCellStyle.ForeColor;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 248);
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 232, 220);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 225, 200);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.Invalidate();
        }

        private void AplicarFiltros()
        {
            try
            {
                var filtrados = _allProductos.AsEnumerable();
                var buscar = txtBuscar?.Text?.Trim() ?? "";
                if (!string.IsNullOrEmpty(buscar) && buscar != "Buscar por SKU, nombre, descripción...")
                {
                    filtrados = filtrados.Where(p => (!string.IsNullOrEmpty(p.Sku) && p.Sku.ToLowerInvariant().Contains(buscar.ToLowerInvariant())) || (!string.IsNullOrEmpty(p.Nombre) && p.Nombre.ToLowerInvariant().Contains(buscar.ToLowerInvariant())) || (!string.IsNullOrEmpty(p.Descripcion) && p.Descripcion.ToLowerInvariant().Contains(buscar.ToLowerInvariant())) || (!string.IsNullOrEmpty(p.Proveedor) && p.Proveedor.ToLowerInvariant().Contains(buscar.ToLowerInvariant())));
                }
                if (cbCategoria != null && cbCategoria.SelectedIndex > 0 && cbCategoria.SelectedItem != null)
                {
                    filtrados = filtrados.Where(p => !string.IsNullOrEmpty(p.CategoriaNombre) && p.CategoriaNombre.Equals(cbCategoria.SelectedItem.ToString(), StringComparison.OrdinalIgnoreCase));
                }
                if (cbEstado != null)
                {
                    if (cbEstado.SelectedIndex == 1) filtrados = filtrados.Where(p => p.Activo);
                    else if (cbEstado.SelectedIndex == 2) filtrados = filtrados.Where(p => !p.Activo);
                }
                if (cbStock != null)
                {
                    if (cbStock.SelectedIndex == 1) filtrados = filtrados.Where(p => !p.TieneBajoStock && !p.SinStock);
                    else if (cbStock.SelectedIndex == 2) filtrados = filtrados.Where(p => p.TieneBajoStock);
                    else if (cbStock.SelectedIndex == 3) filtrados = filtrados.Where(p => p.SinStock);
                }
                var productosOrdenados = filtrados.OrderBy(p => p.Nombre ?? "").ToList();
                dgv.SuspendLayout();
                _productos.RaiseListChangedEvents = false;
                _productos.Clear();
                foreach (var producto in productosOrdenados) _productos.Add(producto);
                _productos.RaiseListChangedEvents = true;
                if (dgv.DataSource != _productos) dgv.DataSource = _productos;
                _productos.ResetBindings();
                dgv.ResumeLayout();
                this.BeginInvoke((Action)(() => AjustarAnchoColumnas()));
            }
            catch { try { _productos.Clear(); foreach (var p in _allProductos) _productos.Add(p); } catch { } }
        }

        private void AjustarAnchoColumnas()
        {
            try
            {
                if (dgv.Columns.Contains("Imagen")) dgv.Columns["Imagen"].Width = 60;
                if (dgv.Columns.Contains("Sku")) dgv.Columns["Sku"].Width = 90;
                if (dgv.Columns.Contains("Nombre")) dgv.Columns["Nombre"].Width = 200;
                // AJUSTAR ANCHO DE NUEVAS COLUMNAS
                if (dgv.Columns.Contains("Proveedor")) dgv.Columns["Proveedor"].Width = 120;
                if (dgv.Columns.Contains("Categoria")) dgv.Columns["Categoria"].Width = 110;
                if (dgv.Columns.Contains("Stock")) dgv.Columns["Stock"].Width = 70;
                if (dgv.Columns.Contains("EstadoStock")) dgv.Columns["EstadoStock"].Width = 120;
                if (dgv.Columns.Contains("Costo")) dgv.Columns["Costo"].Width = 90;
                if (dgv.Columns.Contains("Precio")) dgv.Columns["Precio"].Width = 90;
                if (dgv.Columns.Contains("Estado")) dgv.Columns["Estado"].Width = 85;
                if (dgv.Columns.Contains("Acciones")) dgv.Columns["Acciones"].Width = 80;
            }
            catch { }
        }

        private void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.RowIndex >= _productos.Count || e.ColumnIndex >= dgv.Columns.Count) return;
            try
            {
                if (dgv.Columns[e.ColumnIndex].Name == "Imagen") { e.Value = MakePlaceholderImage(_productos[e.RowIndex].Nombre ?? "?"); e.FormattingApplied = true; return; }
                if (dgv.Columns[e.ColumnIndex].Name == "Estado" && e.Value is bool activo) { e.Value = activo ? "Activo" : "Inactivo"; e.FormattingApplied = true; return; }
            }
            catch { }
        }

        private void Dgv_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.RowIndex >= _productos.Count || e.ColumnIndex >= dgv.Columns.Count) return;
            try
            {
                var colName = dgv.Columns[e.ColumnIndex].Name;
                if (colName == "Estado" || colName == "EstadoStock")
                {
                    e.Handled = true; e.PaintBackground(e.ClipBounds, true);
                    string text = Convert.ToString(e.FormattedValue ?? "");
                    Color bg = Color.Gray;
                    if (colName == "Estado") bg = text.Equals("Activo", StringComparison.OrdinalIgnoreCase) ? Color.FromArgb(34, 139, 34) : Color.FromArgb(200, 180, 80);
                    else if (text == "Disponible") bg = Color.FromArgb(34, 139, 34);
                    else if (text == "Stock Bajo") bg = Color.FromArgb(255, 165, 0);
                    else if (text == "Sin Stock") bg = Color.FromArgb(220, 53, 69);
                    DrawChip(e.Graphics, e.CellBounds, text, bg, Color.White, 12);
                }
            }
            catch { }
        }

        private void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgv.Columns[e.ColumnIndex].Name == "Acciones")
                EditarProducto(_productos[e.RowIndex]);
        }

        private void BtnNuevo_Click(object sender, EventArgs e)
        {
            using (var form = new ProductoForm()) { if (form.ShowDialog() == DialogResult.OK && form.Resultado != null) { _repo.Agregar(form.Resultado); RefrescarDatos(); } }
        }

        private void EditarProducto(Producto producto)
        {
            using (var form = new ProductoForm(producto)) { if (form.ShowDialog() == DialogResult.OK && form.Resultado != null) { form.Resultado.IdProducto = producto.IdProducto; _repo.Actualizar(form.Resultado); RefrescarDatos(); } }
        }

        private void BtnImportar_Click(object sender, EventArgs e)
        {
            using (var od = new OpenFileDialog { Filter = "CSV|*.csv" }) { if (od.ShowDialog() == DialogResult.OK) ImportarCSV(od.FileName); }
        }

        private void ImportarCSV(string archivo) { /* ... (Tu lógica de importación existente) ... */ }
        private void BtnExportar_Click(object sender, EventArgs e) { /* ... (Tu lógica de exportación existente) ... */ }
        private void ExportarCSV(string archivo) { /* ... */ }

        // Helpers Gráficos
        private Image MakePlaceholderImage(string nombre) { int s = 48; var b = new Bitmap(s, s); using (var g = Graphics.FromImage(b)) { g.SmoothingMode = SmoothingMode.AntiAlias; g.Clear(ColSoftAlt); var ini = nombre?.Substring(0, 1).ToUpper() ?? "?"; using (var f = new Font("Segoe UI", 16, FontStyle.Bold)) using (var br = new SolidBrush(ColAccent)) { var z = g.MeasureString(ini, f); g.DrawString(ini, f, br, (s - z.Width) / 2, (s - z.Height) / 2); } } return b; }
        private void DrawChip(Graphics g, Rectangle r, string t, Color b, Color f, int rad) { g.SmoothingMode = SmoothingMode.AntiAlias; var fn = new Font("Segoe UI", 9f, FontStyle.Bold); var s = TextRenderer.MeasureText(t, fn); int w = Math.Min(r.Width - 12, s.Width + 24), h = Math.Min(r.Height - 12, s.Height + 12), x = r.X + (r.Width - w) / 2, y = r.Y + (r.Height - h) / 2; using (var p = RoundedRect(new Rectangle(x, y, w, h), rad)) using (var sb = new SolidBrush(b)) { g.FillPath(sb, p); } TextRenderer.DrawText(g, t, fn, new Rectangle(x, y, w, h), f, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter); }
        private static GraphicsPath RoundedRect(Rectangle b, int r) { int d = r * 2; var p = new GraphicsPath(); p.AddArc(b.X, b.Y, d, d, 180, 90); p.AddArc(b.Right - d, b.Y, d, d, 270, 90); p.AddArc(b.Right - d, b.Bottom - d, d, d, 0, 90); p.AddArc(b.X, b.Bottom - d, d, d, 90, 90); p.CloseFigure(); return p; }
    }
}