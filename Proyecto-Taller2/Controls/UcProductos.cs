using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace Proyecto_Taller_2.Controls
{
    public class UcProductos : UserControl
    {
        // Paleta de colores (consistente con el diseño actual)
        private readonly Color ColBg = Color.White;
        private readonly Color ColSoft = Color.FromArgb(246, 250, 246);
        private readonly Color ColSoftAlt = Color.FromArgb(236, 243, 236);
        private readonly Color ColText = Color.FromArgb(34, 47, 34);
        private readonly Color ColAccent = Color.FromArgb(34, 139, 34);
        private readonly Color ColBorder = Color.FromArgb(210, 220, 210);

        private DataGridView dgv;
        private Panel pnlDetails;

        public UcProductos()
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.DoubleBuffered = true;
            this.Dock = DockStyle.Fill;
            this.BackColor = ColBg;
            BuildUI();
            CargarDatosPrueba();
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
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));  // Top bar
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Grid + details
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));  // Footer
            rootPad.Controls.Add(tlRoot);

            // Panel superior para título y acciones
            var topPanel = new Panel { Dock = DockStyle.Fill };
            
            // Título
            var lblTitulo = new Label
            {
                Text = "Gestión de Productos",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 48,
                ForeColor = ColText,
                Padding = new Padding(8, 8, 0, 0)
            };
            topPanel.Controls.Add(lblTitulo);

            // Acciones
            var panelAcciones = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 48,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 8, 8, 0),
                BackColor = ColBg
            };
            var btnNuevo = new Button { Text = "+ Nuevo Producto", BackColor = Color.FromArgb(201, 222, 201), ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Height = 32, Width = 140 };
            var btnExportar = new Button { Text = "Exportar", BackColor = Color.White, ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Height = 32, Width = 100 };
            var btnImportar = new Button { Text = "Importar", BackColor = Color.White, ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Height = 32, Width = 100 };
            btnNuevo.FlatAppearance.BorderSize = 0;
            btnExportar.FlatAppearance.BorderSize = 0;
            btnImportar.FlatAppearance.BorderSize = 0;
            panelAcciones.Controls.Add(btnNuevo);
            panelAcciones.Controls.Add(btnExportar);
            panelAcciones.Controls.Add(btnImportar);
            topPanel.Controls.Add(panelAcciones);
            
            // Agregar el panel superior al tlRoot
            tlRoot.Controls.Add(topPanel, 0, 0);

            // Panel central para filtros y grilla con padding
            var centerPanel = new Panel { 
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 8, 0, 0)  // Añade espacio arriba
            };
            
            // Filtros con márgenes ajustados
            var gbBuscar = new GroupBox
            {
                Text = "Buscar y Filtrar",
                Dock = DockStyle.Top,
                Height = 110,
                Padding = new Padding(12),         // Aumenta el padding interno
                Margin = new Padding(0, 0, 0, 8),  // Añade margen inferior
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = ColText
            };
            var txtBuscar = new TextBox { Text = "Buscar por nombre, código o descripción...", Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 8) };
            var panelFiltros = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 36, FlowDirection = FlowDirection.LeftToRight };
            var cbCategoria = new ComboBox { Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            cbCategoria.Items.AddRange(new[] { "Todas las categorías", "Electrónicos", "Accesorios", "Oficina" });
            cbCategoria.SelectedIndex = 0;
            var cbStock = new ComboBox { Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            cbStock.Items.AddRange(new[] { "Todos los estados", "Disponible", "Stock Bajo", "Sin Stock" });
            cbStock.SelectedIndex = 0;
            var cbEstado = new ComboBox { Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            cbEstado.Items.AddRange(new[] { "Activos/Inactivos", "Activo", "Inactivo" });
            cbEstado.SelectedIndex = 0;
            panelFiltros.Controls.Add(cbCategoria);
            panelFiltros.Controls.Add(cbStock);
            panelFiltros.Controls.Add(cbEstado);
            gbBuscar.Controls.Add(panelFiltros);
            gbBuscar.Controls.Add(txtBuscar);
            centerPanel.Controls.Add(gbBuscar);

            // Grilla con márgenes y padding ajustados
            var gbLista = new GroupBox
            {
                Text = "Lista de Productos",
                Dock = DockStyle.Fill,
                Padding = new Padding(50),         // Aumenta el padding interno
                Margin = new Padding(0),           // Reset márgenes
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = ColText
            };
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                ColumnHeadersHeight = 42,
                GridColor = ColBorder,
                EnableHeadersVisualStyles = false
            };

            // Estilos del grid
            dgv.ColumnHeadersDefaultCellStyle.BackColor = ColSoftAlt;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = ColText;
            dgv.DefaultCellStyle.Padding = new Padding(4, 4, 4, 4); // Reduce el padding de las celdas
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 232, 220);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.RowTemplate.Height = 48;           // Reduce un poco la altura de las filas

            // Definición de columnas
            var cImagen = new DataGridViewImageColumn 
            { 
                Name = "Imagen", 
                HeaderText = "", 
                FillWeight = 80,
                ImageLayout = DataGridViewImageCellLayout.Zoom 
            };
            var cNombre = new DataGridViewTextBoxColumn 
            { 
                Name = "Nombre", 
                HeaderText = "Nombre", 
                FillWeight = 180 
            };
            var cCodigo = new DataGridViewTextBoxColumn 
            { 
                Name = "Codigo", 
                HeaderText = "Código", 
                FillWeight = 100 
            };
            var cCategoria = new DataGridViewTextBoxColumn 
            { 
                Name = "Categoria", 
                HeaderText = "Categoría", 
                FillWeight = 120 
            };
            var cPrecio = new DataGridViewTextBoxColumn 
            { 
                Name = "Precio", 
                HeaderText = "Precio", 
                FillWeight = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight }
            };
            var cStock = new DataGridViewTextBoxColumn 
            { 
                Name = "Stock", 
                HeaderText = "Stock", 
                FillWeight = 80,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight }
            };
            var cEstado = new DataGridViewTextBoxColumn 
            { 
                Name = "Estado", 
                HeaderText = "Estado", 
                FillWeight = 100 
            };
            var cAcciones = new DataGridViewButtonColumn 
            { 
                Name = "Acciones", 
                HeaderText = "Acciones", 
                FillWeight = 100,
                Text = "Editar",
                UseColumnTextForButtonValue = true
            };

            dgv.Columns.AddRange(new DataGridViewColumn[] 
            { 
                cImagen, cNombre, cCodigo, cCategoria, cPrecio, cStock, cEstado, cAcciones 
            });

            // Eventos
            dgv.CellPainting += Dgv_CellPainting;
            dgv.CellClick += Dgv_CellClick;

            gbLista.Controls.Add(dgv);
            centerPanel.Controls.Add(gbLista);

            // Agregar el panel central al tlRoot
            tlRoot.Controls.Add(centerPanel, 0, 1);

            // Alternar color de filas
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 248);
        }

        private void Dgv_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Estado (pintado especial)
            if (dgv.Columns[e.ColumnIndex].Name == "Estado")
            {
                e.Handled = true;
                e.PaintBackground(e.ClipBounds, true);
                string text = Convert.ToString(e.FormattedValue ?? "");
                Color bg = text.Equals("Activo", StringComparison.OrdinalIgnoreCase) ? 
                          Color.FromArgb(34, 139, 34) : Color.FromArgb(200, 180, 80);
                DrawChip(e.Graphics, e.CellBounds, text, bg, Color.White, 12);
                return;
            }
        }

        private void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgv.Columns[e.ColumnIndex].Name == "Acciones")
            {
                var row = dgv.Rows[e.RowIndex];
                string codigo = row.Cells["Codigo"].Value.ToString();
                MessageBox.Show($"Editando producto {codigo}", "Editar Producto", 
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void CargarDatosPrueba()
        {
            // Datos de ejemplo
            var productos = new[]
            {
                new { Nombre = "Laptop Pro X1", Codigo = "PRD001", Categoria = "Electrónicos", 
                      Precio = "$999.99", Stock = "15", Estado = "Activo" },
                new { Nombre = "Mouse Inalámbrico", Codigo = "PRD002", Categoria = "Accesorios", 
                      Precio = "$29.99", Stock = "8", Estado = "Activo" },
                new { Nombre = "Teclado Mecánico", Codigo = "PRD003", Categoria = "Accesorios", 
                      Precio = "$89.99", Stock = "0", Estado = "Sin Stock" },
                new { Nombre = "Monitor 24'", Codigo = "PRD004", Categoria = "Electrónicos", 
                      Precio = "$199.99", Stock = "5", Estado = "Stock Bajo" },
                new { Nombre = "Disco SSD 1TB", Codigo = "PRD005", Categoria = "Electrónicos", 
                      Precio = "$129.99", Stock = "12", Estado = "Activo" }
            };

            foreach (var p in productos)
            {
                var img = MakePlaceholderImage(p.Nombre);
                dgv.Rows.Add(img, p.Nombre, p.Codigo, p.Categoria, p.Precio, p.Stock, p.Estado);
            }
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

                var initial = nombre.Substring(0, 1).ToUpper();
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