using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;              // 👈 agregado
using System.Linq;
using System.Windows.Forms;

namespace Proyecto_Taller_2
{
    public partial class UcInventario : UserControl
    {
        private readonly BindingList<Producto> _data = new BindingList<Producto>();
        private readonly BindingSource _bs = new BindingSource();

        public TextBox txtBuscar;
        public ComboBox cbCategoria;
        public ComboBox cbEstado;
        public CheckBox chkSoloBajoStock;
        public DataGridView dgv;
        public Button btnNuevo;
        public Button btnEntrada;
        public Button btnSalida;
        public Button btnAjuste;
        public Button btnImportar;
        public Button btnExportar;
        public Button btnEditar;
        public Label kpiTotalVal;
        public Label kpiBajoVal;
        public Label kpiValVal;
        public Label lblDetNombre;
        public Label lblDetSku;
        public Label lblDetCat;
        public Label lblDetUbic;
        public Label lblDetStock;
        public Label lblDetPrecio;
        public Label lblDetActualizado;

        public UcInventario()
        {
            InitializeComponent();

            if (!IsDesigner())
            {
                Load += UcInventario_Load;

                txtBuscar.TextChanged += new EventHandler((sender, e) => ApplyFilters());
                cbCategoria.SelectedIndexChanged += new EventHandler((sender, e) => ApplyFilters());
                cbEstado.SelectedIndexChanged += new EventHandler((sender, e) => ApplyFilters());
                chkSoloBajoStock.CheckedChanged += new EventHandler((sender, e) => ApplyFilters());

                dgv.CellFormatting += new DataGridViewCellFormattingEventHandler(Dgv_CellFormatting);
                dgv.SelectionChanged += new EventHandler((sender, e) => UpdateDetalle());

                btnNuevo.Click += new EventHandler((sender, e) => MessageBox.Show("Nuevo producto (form/modal)…"));
                btnEntrada.Click += new EventHandler((sender, e) => MessageBox.Show("Registrar entrada de stock…"));
                btnSalida.Click += new EventHandler((sender, e) => MessageBox.Show("Registrar salida de stock…"));
                btnAjuste.Click += new EventHandler((sender, e) => MessageBox.Show("Ajuste de stock…"));
                btnImportar.Click += new EventHandler((sender, e) => MessageBox.Show("Importar CSV…"));
                btnExportar.Click += new EventHandler((sender, e) => MessageBox.Show("Exportar…"));
                btnEditar.Click += new EventHandler((sender, e) => MessageBox.Show("Editar producto seleccionado…"));
            }
        }

        private void InitializeComponent()
        {
            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
            Controls.Add(mainPanel);

            var filtrosPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 0, 0, 8)
            };
            mainPanel.Controls.Add(filtrosPanel);

            txtBuscar = new TextBox { Width = 180, Margin = new Padding(4) };
            cbCategoria = new ComboBox { Width = 140, Margin = new Padding(4) };
            cbEstado = new ComboBox { Width = 100, Margin = new Padding(4) };
            chkSoloBajoStock = new CheckBox { Text = "Solo bajo stock", Margin = new Padding(4, 8, 4, 4) };

            filtrosPanel.Controls.Add(txtBuscar);
            filtrosPanel.Controls.Add(cbCategoria);
            filtrosPanel.Controls.Add(cbEstado);
            filtrosPanel.Controls.Add(chkSoloBajoStock);

            var botonesPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 0, 0, 8)
            };
            mainPanel.Controls.Add(botonesPanel);

            btnNuevo = new Button { Text = "Nuevo", Width = 80, Margin = new Padding(4) };
            btnEntrada = new Button { Text = "Entrada", Width = 80, Margin = new Padding(4) };
            btnSalida = new Button { Text = "Salida", Width = 80, Margin = new Padding(4) };
            btnAjuste = new Button { Text = "Ajuste", Width = 80, Margin = new Padding(4) };
            btnImportar = new Button { Text = "Importar", Width = 80, Margin = new Padding(4) };
            btnExportar = new Button { Text = "Exportar", Width = 80, Margin = new Padding(4) };
            btnEditar = new Button { Text = "Editar", Width = 80, Margin = new Padding(4) };

            botonesPanel.Controls.Add(btnNuevo);
            botonesPanel.Controls.Add(btnEntrada);
            botonesPanel.Controls.Add(btnSalida);
            botonesPanel.Controls.Add(btnAjuste);
            botonesPanel.Controls.Add(btnImportar);
            botonesPanel.Controls.Add(btnExportar);
            botonesPanel.Controls.Add(btnEditar);

            var kpiPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 0, 0, 8)
            };
            mainPanel.Controls.Add(kpiPanel);

            kpiTotalVal = new Label { Text = "0", Width = 80, Margin = new Padding(4) };
            kpiBajoVal = new Label { Text = "0", Width = 80, Margin = new Padding(4) };
            kpiValVal = new Label { Text = "$0", Width = 80, Margin = new Padding(4) };

            kpiPanel.Controls.Add(new Label { Text = "Total:", Width = 50, Margin = new Padding(4) });
            kpiPanel.Controls.Add(kpiTotalVal);
            kpiPanel.Controls.Add(new Label { Text = "Bajo stock:", Width = 80, Margin = new Padding(4) });
            kpiPanel.Controls.Add(kpiBajoVal);
            kpiPanel.Controls.Add(new Label { Text = "Valorizado:", Width = 80, Margin = new Padding(4) });
            kpiPanel.Controls.Add(kpiValVal);

            dgv = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 220,
                Margin = new Padding(4),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            mainPanel.Controls.Add(dgv);

            var detallePanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(0, 8, 0, 0)
            };
            mainPanel.Controls.Add(detallePanel);

            lblDetNombre = new Label { Text = "Nombre: —", Width = 300, Margin = new Padding(4) };
            lblDetSku = new Label { Text = "SKU: —", Width = 300, Margin = new Padding(4) };
            lblDetCat = new Label { Text = "Categoría: —", Width = 300, Margin = new Padding(4) };
            lblDetUbic = new Label { Text = "Ubicación: —", Width = 300, Margin = new Padding(4) };
            lblDetStock = new Label { Text = "Stock / Mín.: —", Width = 300, Margin = new Padding(4) };
            lblDetPrecio = new Label { Text = "Precio: —", Width = 300, Margin = new Padding(4) };
            lblDetActualizado = new Label { Text = "Actualizado: —", Width = 300, Margin = new Padding(4) };

            detallePanel.Controls.Add(lblDetNombre);
            detallePanel.Controls.Add(lblDetSku);
            detallePanel.Controls.Add(lblDetCat);
            detallePanel.Controls.Add(lblDetUbic);
            detallePanel.Controls.Add(lblDetStock);
            detallePanel.Controls.Add(lblDetPrecio);
            detallePanel.Controls.Add(lblDetActualizado);
        }

        private static bool IsDesigner()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime
                   || Process.GetCurrentProcess().ProcessName.Equals("devenv", StringComparison.OrdinalIgnoreCase);
        }

        private void UcInventario_Load(object sender, EventArgs e)
        {
            var demo = Seed();
            foreach (var p in demo) _data.Add(p);

            var categorias = new List<string> { "Todas las categorías" };
            categorias.AddRange(_data.Select(p => p.Categoria).Distinct().OrderBy(s => s));
            cbCategoria.DataSource = categorias;

            cbEstado.DataSource = new List<string> { "Todos", "Activo", "Inactivo" };
            cbEstado.SelectedIndex = 0;

            _bs.DataSource = _data;
            dgv.DataSource = _bs;

            // 👇 Estilo del grid + columnas explícitas
            SetupGridStyle();
            SetupColumns();

            RefreshKpis();

            if (dgv.Rows.Count > 0) dgv.Rows[0].Selected = true;
            UpdateDetalle();
        }

        // ======= Estilo del DataGridView (quita el fondo gris) =======
        private void SetupGridStyle()
        {
            var hover = Color.FromArgb(220, 232, 220);
            var activo = Color.FromArgb(201, 222, 201);
            var texto = Color.FromArgb(34, 47, 34);

            dgv.RowHeadersVisible = false;
            dgv.EnableHeadersVisualStyles = false;

            // Encabezados
            dgv.ColumnHeadersDefaultCellStyle.BackColor = hover;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = texto;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

            // Celdas
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10F);
            dgv.DefaultCellStyle.SelectionBackColor = activo;
            dgv.DefaultCellStyle.SelectionForeColor = texto;

            // Alternado
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 251, 248);

            // 👇 Fondo blanco en todo el grid (incluyendo el área vacía)
            dgv.BackgroundColor = Color.White;
            dgv.DefaultCellStyle.BackColor = Color.White;
        }

        // ======= Columnas explícitas y formatos =======
        private void SetupColumns()
        {
            dgv.AutoGenerateColumns = false;
            dgv.Columns.Clear();

            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Sku", DataPropertyName = "Sku", Width = 90, MinimumWidth = 80 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nombre", DataPropertyName = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Categoría", DataPropertyName = "Categoria", Width = 120 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ubicación", DataPropertyName = "Ubicacion", Width = 110 });

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Stock",
                DataPropertyName = "Stock",
                Width = 70,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Mínimo",
                DataPropertyName = "Minimo",
                Width = 70,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Precio",
                DataPropertyName = "Precio",
                Width = 90,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "C2" }
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Proveedor", DataPropertyName = "Proveedor", Width = 120 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Estado", DataPropertyName = "Estado", Width = 90 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Actualizado",
                DataPropertyName = "Actualizado",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "g" }
            });
        }

        private void RefreshKpis()
        {
            var total = _bs.Cast<Producto>().Count();
            var bajo = _bs.Cast<Producto>().Count(p => p.Stock < p.Minimo);
            var valorizado = _bs.Cast<Producto>().Sum(p => p.Stock * p.Precio);

            kpiTotalVal.Text = total.ToString();
            kpiBajoVal.Text = bajo.ToString();
            kpiValVal.Text = valorizado.ToString("C2");
        }

        private void ApplyFilters()
        {
            var q = txtBuscar.Text != null ? txtBuscar.Text.Trim().ToLower() : "";
            var cat = cbCategoria.SelectedItem != null ? cbCategoria.SelectedItem.ToString() : "Todas las categorías";
            var est = cbEstado.SelectedItem != null ? cbEstado.SelectedItem.ToString() : "Todos";
            var soloBajo = chkSoloBajoStock.Checked;

            IEnumerable<Producto> filtered = _data;

            if (!string.IsNullOrEmpty(q))
                filtered = filtered.Where(p =>
                    (p.Nombre != null && p.Nombre.ToLower().Contains(q)) ||
                    (p.Sku != null && p.Sku.ToLower().Contains(q)) ||
                    (p.Proveedor != null && p.Proveedor.ToLower().Contains(q)));

            if (cat != "Todas las categorías")
                filtered = filtered.Where(p => p.Categoria == cat);

            if (est != "Todos")
                filtered = filtered.Where(p => p.Estado == est);

            if (soloBajo)
                filtered = filtered.Where(p => p.Stock < p.Minimo);

            _bs.DataSource = new BindingList<Producto>(filtered.OrderBy(p => p.Nombre).ToList());
            dgv.DataSource = _bs;

            RefreshKpis();
            UpdateDetalle();
        }

        private void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgv.Columns[e.ColumnIndex].DataPropertyName == "Stock")
            {
                var row = dgv.Rows[e.RowIndex].DataBoundItem as Producto;
                if (row == null) return;

                if (row.Stock < row.Minimo)
                {
                    e.CellStyle.BackColor = System.Drawing.Color.MistyRose;
                    e.CellStyle.ForeColor = System.Drawing.Color.Maroon;
                }
                else
                {
                    e.CellStyle.BackColor = System.Drawing.Color.Honeydew;
                    e.CellStyle.ForeColor = System.Drawing.Color.DarkGreen;
                }
            }

            if (dgv.Columns[e.ColumnIndex].DataPropertyName == "Estado")
            {
                if ((e.Value != null ? e.Value.ToString() : "") == "Inactivo")
                {
                    e.CellStyle.ForeColor = System.Drawing.Color.DimGray;
                }
            }
        }

        private void UpdateDetalle()
        {
            Producto p = null;
            if (dgv.CurrentRow != null && dgv.CurrentRow.DataBoundItem is Producto)
                p = (Producto)dgv.CurrentRow.DataBoundItem;

            if (p == null)
            {
                lblDetNombre.Text = "Nombre: —";
                lblDetSku.Text = "SKU: —";
                lblDetCat.Text = "Categoría: —";
                lblDetUbic.Text = "Ubicación: —";
                lblDetStock.Text = "Stock / Mín.: —";
                lblDetPrecio.Text = "Precio: —";
                lblDetActualizado.Text = "Actualizado: —";
                return;
            }

            lblDetNombre.Text = "Nombre: " + p.Nombre;
            lblDetSku.Text = "SKU: " + p.Sku;
            lblDetCat.Text = "Categoría: " + p.Categoria;
            lblDetUbic.Text = "Ubicación: " + p.Ubicacion;
            lblDetStock.Text = "Stock / Mín.: " + p.Stock + " / " + p.Minimo;
            lblDetPrecio.Text = "Precio: " + p.Precio.ToString("C2");
            lblDetActualizado.Text = "Actualizado: " + p.Actualizado.ToString("g");
        }

        private static List<Producto> Seed()
        {
            var list = new List<Producto>();
            list.Add(new Producto { Sku = "A-1001", Nombre = "Tornillo 3/8\" x 1\"", Categoria = "Ferretería", Ubicacion = "A1-01", Stock = 120, Minimo = 50, Precio = 120.00m, Proveedor = "ACME", Estado = "Activo", Actualizado = DateTime.Now.AddHours(-2) });
            list.Add(new Producto { Sku = "A-1002", Nombre = "Arandela 3/8\"", Categoria = "Ferretería", Ubicacion = "A1-02", Stock = 30, Minimo = 60, Precio = 15.50m, Proveedor = "ACME", Estado = "Activo", Actualizado = DateTime.Now.AddDays(-1) });
            list.Add(new Producto { Sku = "B-2001", Nombre = "Pintura Látex 4L", Categoria = "Pinturas", Ubicacion = "B3-12", Stock = 8, Minimo = 10, Precio = 9500m, Proveedor = "Colores", Estado = "Activo", Actualizado = DateTime.Now.AddMinutes(-30) });
            list.Add(new Producto { Sku = "B-2002", Nombre = "Pincel 2”", Categoria = "Pinturas", Ubicacion = "B1-05", Stock = 85, Minimo = 20, Precio = 1700m, Proveedor = "Colores", Estado = "Activo", Actualizado = DateTime.Now.AddDays(-3) });
            list.Add(new Producto { Sku = "C-3001", Nombre = "Guantes Nitrilo", Categoria = "Seguridad", Ubicacion = "C2-07", Stock = 0, Minimo = 25, Precio = 2500m, Proveedor = "SafeCorp", Estado = "Inactivo", Actualizado = DateTime.Now.AddDays(-8) });
            return list;
        }
    }

    public class Producto
    {
        public string Sku { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Categoria { get; set; } = "";
        public string Ubicacion { get; set; } = "";
        public int Stock { get; set; }
        public int Minimo { get; set; }
        public decimal Precio { get; set; }
        public string Proveedor { get; set; } = "";
        public string Estado { get; set; } = "Activo";
        public DateTime Actualizado { get; set; } = DateTime.Now;
    }
}
