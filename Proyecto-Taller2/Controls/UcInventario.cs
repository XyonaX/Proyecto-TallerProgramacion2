using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Proyecto_Taller_2.UI.Helpers;
using Proyecto_Taller_2.Data.Repositories;
using Proyecto_Taller_2.Domain.Models;

namespace Proyecto_Taller_2
{
    public partial class UcInventario : UserControl
    {
        private const int STOCK_SALUDABLE_MARGEN = 5;
        private static readonly string[] CATEGORIAS = new[] { "remera", "campera" };

        private readonly InventarioRepository _repo = new InventarioRepository();
        private BindingList<InventarioItem> _data = new BindingList<InventarioItem>();
        private readonly BindingSource _bs = new BindingSource();

        public TextBox txtBuscar;
        public ComboBox cbCategoria;
        public ComboBox cbEstado;
        public CheckBox chkSoloBajoStock;
        public DataGridView dgv;
        public Button btnNuevo, btnEntrada, btnSalida, btnAjuste, btnImportar, btnExportar, btnEditar;
        public Label kpiTotalVal, kpiBajoVal, kpiValVal, lblDetNombre, lblDetSku, lblDetCat, lblDetUbic, lblDetStock, lblDetPrecio, lblDetActualizado;

        public UcInventario()
        {
            InitializeComponent();

            if (!IsDesigner())
            {
                Load += UcInventario_Load;

                txtBuscar.TextChanged += delegate { ApplyFilters(); };
                cbCategoria.SelectedIndexChanged += delegate { ApplyFilters(); };
                cbEstado.SelectedIndexChanged += delegate { ApplyFilters(); };
                chkSoloBajoStock.CheckedChanged += delegate { ApplyFilters(); };

                dgv.CellFormatting += Dgv_CellFormatting;
                dgv.SelectionChanged += delegate { UpdateDetalle(); };

                btnNuevo.Click += delegate { NuevoProducto(); };
                btnEditar.Click += delegate { EditarProductoSeleccionado(); };
                btnEntrada.Click += delegate { RegistrarMovimiento('E'); };
                btnSalida.Click += delegate { RegistrarMovimiento('S'); };
                btnAjuste.Click += delegate { RegistrarMovimiento('A'); };
                btnImportar.Click += delegate { ImportarCsv(); };
                btnExportar.Click += delegate { ExportarCsv(); };
            }
        }

        private static bool IsDesigner()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime
                   || Process.GetCurrentProcess().ProcessName.Equals("devenv", StringComparison.OrdinalIgnoreCase);
        }

        private void UcInventario_Load(object sender, EventArgs e)
        {
            cbEstado.DataSource = new List<string> { "Todos", "Activo", "Inactivo" };
            cbEstado.SelectedIndex = 0;

            cbCategoria.DataSource = new List<string> { "Todas las categorías" }.Concat(CATEGORIAS).ToList();
            cbCategoria.SelectedIndex = 0;

            ConfigurarGrid();
            RefrescarDesdeDb();

            _bs.DataSource = _data;
            dgv.DataSource = _bs;

            RefreshKpis();

            if (dgv.Rows.Count > 0) dgv.Rows[0].Selected = true;
            UpdateDetalle();
        }

        private void RefrescarDesdeDb()
        {
            bool? activo = null;
            if (cbEstado != null && cbEstado.SelectedItem != null)
            {
                string sel = cbEstado.SelectedItem.ToString();
                if (sel == "Activo") activo = true;
                else if (sel == "Inactivo") activo = false;
            }

            List<InventarioItem> lista = _repo.Listar(false, null, activo);
            _data = new BindingList<InventarioItem>(lista);
            _bs.DataSource = _data;
            dgv.DataSource = _bs;
        }

        private void ConfigurarGrid()
        {
            dgv.RowHeadersVisible = false;
            dgv.EnableHeadersVisualStyles = false;

            Color header = Color.FromArgb(220, 232, 220);
            Color seleccion = Color.FromArgb(201, 222, 201);
            Color texto = Color.FromArgb(34, 47, 34);

            dgv.ColumnHeadersDefaultCellStyle.BackColor = header;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = texto;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 34;

            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10.5F);
            dgv.DefaultCellStyle.SelectionBackColor = seleccion;
            dgv.DefaultCellStyle.SelectionForeColor = texto;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 251, 248);
            dgv.BackgroundColor = Color.White;

            dgv.RowTemplate.Height = 28;
            dgv.AutoGenerateColumns = false;
            dgv.Columns.Clear();

            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Sku", DataPropertyName = "Sku", Width = 90 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nombre", DataPropertyName = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Categoría", DataPropertyName = "Categoria", Width = 140 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ubicación", DataPropertyName = "Ubicacion", Width = 110 });

            DataGridViewTextBoxColumn colStock = new DataGridViewTextBoxColumn { HeaderText = "Stock", DataPropertyName = "Stock", Width = 80 };
            colStock.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgv.Columns.Add(colStock);

            DataGridViewTextBoxColumn colMin = new DataGridViewTextBoxColumn { HeaderText = "Mínimo", DataPropertyName = "Minimo", Width = 80 };
            colMin.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgv.Columns.Add(colMin);

            DataGridViewTextBoxColumn colPrecio = new DataGridViewTextBoxColumn { HeaderText = "Precio", DataPropertyName = "Precio", Width = 110 };
            colPrecio.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colPrecio.DefaultCellStyle.Format = "C";
            colPrecio.DefaultCellStyle.FormatProvider = new CultureInfo("es-AR");
            dgv.Columns.Add(colPrecio);

            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Proveedor", DataPropertyName = "Proveedor", Width = 140 });
            DataGridViewTextBoxColumn colEstado = new DataGridViewTextBoxColumn { HeaderText = "Estado", DataPropertyName = "Estado", Width = 100 };
            dgv.Columns.Add(colEstado);

            DataGridViewTextBoxColumn colAct = new DataGridViewTextBoxColumn { HeaderText = "Actualizado", DataPropertyName = "Actualizado", Width = 150 };
            colAct.DefaultCellStyle.Format = "g";
            dgv.Columns.Add(colAct);
        }

        private void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            InventarioItem item = dgv.Rows[e.RowIndex].DataBoundItem as InventarioItem;
            if (item == null) return;

            if (dgv.Columns[e.ColumnIndex].DataPropertyName == "Stock")
            {
                if (item.Stock < item.Minimo)
                {
                    e.CellStyle.BackColor = Color.MistyRose;
                    e.CellStyle.ForeColor = Color.Maroon;
                }
                else if (item.Stock <= item.Minimo + STOCK_SALUDABLE_MARGEN)
                {
                    e.CellStyle.BackColor = Color.Honeydew;
                    e.CellStyle.ForeColor = Color.DarkGreen;
                }
            }
        }

        private void ApplyFilters()
        {
            string q = (txtBuscar != null && txtBuscar.Text != null) ? txtBuscar.Text.Trim().ToLower() : "";
            string cat = (cbCategoria != null && cbCategoria.SelectedItem != null) ? cbCategoria.SelectedItem.ToString() : "Todas las categorías";
            string est = (cbEstado != null && cbEstado.SelectedItem != null) ? cbEstado.SelectedItem.ToString() : "Todos";
            bool soloBajo = (chkSoloBajoStock != null && chkSoloBajoStock.Checked);

            IEnumerable<InventarioItem> filtered = _data;

            if (q.Length > 0)
                filtered = filtered.Where(p =>
                    ((p.Nombre ?? "").ToLower().Contains(q)) ||
                    ((p.Sku ?? "").ToLower().Contains(q)) ||
                    ((p.Proveedor ?? "").ToLower().Contains(q)));

            if (!string.IsNullOrEmpty(cat) && cat != "Todas las categorías")
                filtered = filtered.Where(p => string.Equals(p.Categoria ?? "", cat, StringComparison.OrdinalIgnoreCase));

            if (est != "Todos")
                filtered = filtered.Where(p => (p.Estado ?? "") == est);

            if (soloBajo)
                filtered = filtered.Where(p => p.Stock < p.Minimo);

            _bs.DataSource = new BindingList<InventarioItem>(filtered.OrderBy(p => p.Nombre).ToList());
            dgv.DataSource = _bs;

            RefreshKpis();
            UpdateDetalle();
        }

        private void RefreshKpis()
        {
            List<InventarioItem> lista = _bs.Cast<InventarioItem>().ToList();
            int total = lista.Count;
            int bajo = lista.Count(p => p.Stock < p.Minimo);
            decimal valorizado = 0m;
            foreach (InventarioItem p in lista)
            {
                int s = (p.Stock > 0) ? p.Stock : 0;
                valorizado += s * p.Precio;
            }

            kpiTotalVal.Text = total.ToString("N0");
            kpiBajoVal.Text = bajo.ToString("N0");
            kpiValVal.Text = valorizado.ToString("C", new CultureInfo("es-AR"));
        }

        private void UpdateDetalle()
        {
            InventarioItem p = (dgv.CurrentRow != null) ? dgv.CurrentRow.DataBoundItem as InventarioItem : null;
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
            lblDetPrecio.Text = "Precio: " + p.Precio.ToString("C", new CultureInfo("es-AR"));
            lblDetActualizado.Text = "Actualizado: " + p.Actualizado.ToString("g");
        }

        // ==================== CRUD ====================

        private static string ElegirCategoriaPorNumero(string entrada)
        {
            if (entrada == null) return "remera";
            string t = entrada.Trim().ToLower();
            if (t == "2" || t == "campera") return "campera";
            return "remera";
        }

        private void NuevoProducto()
        {
            string sku = FrmInput.Show("Nuevo producto", "SKU:", "");
            if (string.IsNullOrWhiteSpace(sku)) return;

            string nombre = FrmInput.Show("Nuevo producto", "Nombre:", "");
            if (string.IsNullOrWhiteSpace(nombre)) return;

            string catSel = FrmInput.Show("Nuevo producto", "Categoría (1=Remera, 2=Campera):", "1");
            string categoria = ElegirCategoriaPorNumero(catSel);

            string ubic = FrmInput.Show("Nuevo producto", "Ubicación:", "");
            string minimoStr = FrmInput.Show("Nuevo producto", "Mínimo:", "5");
            string precioStr = FrmInput.Show("Nuevo producto", "Precio:", "0");
            string prov = FrmInput.Show("Nuevo producto", "Proveedor:", "");
            string actStr = FrmInput.Show("Nuevo producto", "¿Activo? (s/n):", "s");
            if (actStr == null) return;

            int minimo = 5; int.TryParse(minimoStr, out minimo);
            decimal precio = 0m; decimal.TryParse(precioStr, out precio);
            string al = actStr.ToLower();
            bool activo = (al == "s" || al == "si" || al == "sí");

            Producto p = new Producto();
            p.Sku = sku;
            p.Nombre = nombre;
            p.SetCategoriaByName(categoria); // Usar helper method
            p.Ubicacion = ubic;
            p.Minimo = minimo;
            p.Precio = precio;
            p.Proveedor = prov;
            p.Activo = activo;

            int id = new InventarioRepository().CrearProducto(p);
            if (id > 0)
            {
                RefrescarDesdeDb();
                cbCategoria.DataSource = new List<string> { "Todas las categorías", "remera", "campera" };
                cbCategoria.SelectedItem = categoria;
                ApplyFilters();
                MessageBox.Show("Producto creado.");
            }
        }

        private void EditarProductoSeleccionado()
        {
            InventarioItem it = (dgv.CurrentRow != null) ? dgv.CurrentRow.DataBoundItem as InventarioItem : null;
            if (it == null) { MessageBox.Show("Seleccioná un producto."); return; }

            string sku = FrmInput.Show("Editar producto", "SKU:", it.Sku);
            if (string.IsNullOrWhiteSpace(sku)) return;

            string nombre = FrmInput.Show("Editar producto", "Nombre:", it.Nombre);
            if (string.IsNullOrWhiteSpace(nombre)) return;

            string catSel = FrmInput.Show("Editar producto", "Categoría (1=Remera, 2=Campera):", it.Categoria == "campera" ? "2" : "1");
            string categoria = ElegirCategoriaPorNumero(catSel);

            string ubic = FrmInput.Show("Editar producto", "Ubicación:", it.Ubicacion);
            string minimoStr = FrmInput.Show("Editar producto", "Mínimo:", it.Minimo.ToString());
            string precioStr = FrmInput.Show("Editar producto", "Precio:", it.Precio.ToString());
            string prov = FrmInput.Show("Editar producto", "Proveedor:", it.Proveedor);
            string actStr = FrmInput.Show("Editar producto", "¿Activo? (s/n):", it.Estado == "Activo" ? "s" : "n");
            if (actStr == null) return;

            int minimo = it.Minimo; int.TryParse(minimoStr, out minimo);
            decimal precio = it.Precio; decimal.TryParse(precioStr, out precio);
            string al = actStr.ToLower();
            bool activo = (al == "s" || al == "si" || al == "sí");

            Producto p = new Producto();
            p.IdProducto = it.IdProducto;
            p.Sku = sku;
            p.Nombre = nombre;
            p.SetCategoriaByName(categoria); // Usar helper method
            p.Ubicacion = ubic;
            p.Minimo = minimo;
            p.Precio = precio;
            p.Proveedor = prov;
            p.Activo = activo;

            new InventarioRepository().ActualizarProducto(p);
            RefrescarDesdeDb();
            cbCategoria.DataSource = new List<string> { "Todas las categorías", "remera", "campera" };
            cbCategoria.SelectedItem = categoria;
            ApplyFilters();
            MessageBox.Show("Producto actualizado.");
        }

        private void RegistrarMovimiento(char tipo)
        {
            InventarioItem it = (dgv.CurrentRow != null) ? dgv.CurrentRow.DataBoundItem as InventarioItem : null;
            if (it == null) { MessageBox.Show("Seleccioná un producto."); return; }

            string titulo = (tipo == 'E') ? "Entrada" : (tipo == 'S') ? "Salida" : "Ajuste (+/-)";
            string cantStr = FrmInput.Show(titulo, "Cantidad:", (tipo == 'A') ? "0" : "1");
            if (string.IsNullOrWhiteSpace(cantStr)) return;

            int cant;
            if (!int.TryParse(cantStr, out cant))
            {
                MessageBox.Show("Valor inválido. Ingresá un número entero (podés usar negativo en Ajuste).");
                return;
            }

            if ((tipo == 'E' || tipo == 'S') && cant <= 0)
            {
                MessageBox.Show("Para Entradas/Salidas la cantidad debe ser mayor que cero.");
                return;
            }
            if (tipo == 'A' && cant == 0)
            {
                MessageBox.Show("El Ajuste no puede ser cero (usá +N o -N).");
                return;
            }

            string obs = FrmInput.Show(titulo, "Observación:", "");
            new InventarioRepository().Movimiento(it.IdProducto, tipo, cant, obs, null, null);

            RefrescarDesdeDb();
            ApplyFilters();
            MessageBox.Show(titulo + " registrada.");
        }

        private void ImportarCsv()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV (*.csv)|*.csv";
            if (ofd.ShowDialog() != DialogResult.OK) return;

            int ok = 0, err = 0;
            foreach (string line in System.IO.File.ReadLines(ofd.FileName).Skip(1))
            {
                try
                {
                    string[] c = line.Split(';');
                    string catCsv = (c.Length > 2 ? c[2].Trim().ToLower() : "remera");
                    string categoria = (catCsv == "campera") ? "campera" : "remera";

                    Producto p = new Producto();
                    p.Sku = (c.Length > 0) ? c[0].Trim() : "";
                    p.Nombre = (c.Length > 1) ? c[1].Trim() : "";
                    p.SetCategoriaByName(categoria); // Usar helper method
                    p.Ubicacion = (c.Length > 3) ? c[3].Trim() : "";
                    int m; p.Minimo = (c.Length > 4 && int.TryParse(c[4], out m)) ? m : 5;
                    decimal pr; p.Precio = (c.Length > 5 && decimal.TryParse(c[5], out pr)) ? pr : 0m;
                    p.Proveedor = (c.Length > 6) ? c[6].Trim() : "";
                    p.Activo = true;

                    if (string.IsNullOrEmpty(p.Sku) || string.IsNullOrEmpty(p.Nombre))
                        throw new Exception("Datos incompletos");

                    new InventarioRepository().CrearProducto(p);
                    ok++;
                }
                catch { err++; }
            }

            RefrescarDesdeDb();
            ApplyFilters();
            MessageBox.Show("Importación finalizada. OK: " + ok + " | Errores: " + err);
        }

        private void ExportarCsv()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV (*.csv)|*.csv";
            sfd.FileName = "inventario.csv";
            if (sfd.ShowDialog() != DialogResult.OK) return;

            List<InventarioItem> items = _bs.Cast<InventarioItem>().ToList();
            List<string> lines = new List<string>();
            lines.Add("Sku;Nombre;Categoria;Ubicacion;Stock;Minimo;Precio;Proveedor;Estado;Actualizado");
            foreach (InventarioItem i in items)
            {
                lines.Add(string.Join(";", new string[] {
                    i.Sku, i.Nombre, i.Categoria, i.Ubicacion,
                    i.Stock.ToString(), i.Minimo.ToString(),
                    i.Precio.ToString("0.00"), i.Proveedor,
                    i.Estado, i.Actualizado.ToString("yyyy-MM-dd HH:mm")
                }));
            }

            System.IO.File.WriteAllLines(sfd.FileName, lines.ToArray(), System.Text.Encoding.UTF8);
            MessageBox.Show("Exportado correctamente.");
        }
    }
}
