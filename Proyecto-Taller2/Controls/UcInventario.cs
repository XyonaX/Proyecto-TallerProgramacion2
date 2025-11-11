using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Proyecto_Taller_2.Data.Repositories;
using Proyecto_Taller_2.Forms;
using Proyecto_Taller_2.Domain.Models; // Para InventarioItem y Categoria
using Proyecto_Taller_2.Domain.Entities; // Para Producto y MovimientoStock
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using ProductoEntity = Proyecto_Taller_2.Domain.Entities.Producto;
using ProductoModel = Proyecto_Taller_2.Domain.Models.Producto;


namespace Proyecto_Taller_2
{
    public partial class UcInventario : UserControl
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;
        private static void SetPlaceholder(TextBox tb, string text)
        {
            if (!IsDesigner() && tb != null && !tb.IsDisposed && tb.Handle != IntPtr.Zero)
                SendMessage(tb.Handle, EM_SETCUEBANNER, 1, text);
        }

        private const int STOCK_SALUDABLE_MARGEN = 5;

        private readonly InventarioRepository _repo = new InventarioRepository();
        private BindingList<InventarioItem> _data = new BindingList<InventarioItem>();
        private readonly BindingSource _bs = new BindingSource();
        private BindingList<MovimientoStock> _historialData = new BindingList<MovimientoStock>();
        private readonly BindingSource _bsHistorial = new BindingSource();
        private List<Categoria> _categoriasDisponibles = new List<Categoria>();

        public UcInventario()
        {
            InitializeComponent();

            if (btnNuevo != null) { btnNuevo.Visible = false; btnNuevo.Enabled = false; }
            if (btnEntrada != null) { btnEntrada.Visible = false; btnEntrada.Enabled = false; }
            if (btnSalida != null) { btnSalida.Visible = false; btnSalida.Enabled = false; }

            if (!IsDesigner())
            {
                Load += UcInventario_Load;
                if (txtBuscar != null) txtBuscar.TextChanged += (s, e) => ApplyFilters();
                if (cbCategoria != null) cbCategoria.SelectedIndexChanged += (s, e) => ApplyFilters();
                if (cbEstado != null) cbEstado.SelectedIndexChanged += (s, e) => ApplyFilters();
                if (chkSoloBajoStock != null) chkSoloBajoStock.CheckedChanged += (s, e) => ApplyFilters();
                if (dgv != null)
                {
                    dgv.CellFormatting += Dgv_CellFormatting;
                    dgv.SelectionChanged += Dgv_SelectionChanged;
                }

                if (dgvHistorial != null)
                {
                    dgvHistorial.SelectionChanged += DgvHistorial_SelectionChanged;
                    dgvHistorial.CellDoubleClick += (s, e) =>
                    {
                        if (e.RowIndex >= 0)
                            MessageBox.Show(
                                dgvHistorial.Rows[e.RowIndex].Cells["Observacion"].Value?.ToString() ?? "",
                                "Detalle Ajuste"
                            );
                    };
                }

                if (btnEditar != null) btnEditar.Click += BtnEditar_Click;
                if (btnAjuste != null) btnAjuste.Click += BtnAjuste_Click;
                if (btnImportar != null) btnImportar.Click += BtnImportar_Click;
                if (btnExportar != null) btnExportar.Click += BtnExportar_Click;
            }
        }

        private static bool IsDesigner()
        {
            try
            {
                return LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
                       Process.GetCurrentProcess().ProcessName.ToLower().Contains("devenv");
            }
            catch
            {
                return true;
            }
        }

        private async void UcInventario_Load(object sender, EventArgs e)
        {
            if (cbEstado != null)
            {
                cbEstado.DataSource = new List<string> { "Todos", "Activo", "Inactivo" };
                cbEstado.SelectedIndex = 0;
            }
            await CargarCategoriasAsync();
            ConfigurarGridPrincipal();
            ConfigurarGridHistorial();
            _bs.DataSource = _data;
            if (dgv != null) dgv.DataSource = _bs;
            _bsHistorial.DataSource = _historialData;
            if (dgvHistorial != null) dgvHistorial.DataSource = _bsHistorial;
            await RefrescarDesdeDbAsync();
            if (txtBuscar != null) SetPlaceholder(txtBuscar, "Buscar por Sku, Nombre, Proveedor...");
        }

        private async Task CargarCategoriasAsync()
        {
            try
            {
                _categoriasDisponibles = await _repo.ObtenerCategoriasAsync();
                var dataSource = new List<object> { new { Id = 0, Nombre = "Todas las categorías" } };
                dataSource.AddRange(_categoriasDisponibles.Select(c => new { Id = c.IdCategoria, Nombre = c.Nombre }).ToList<object>());
                if (cbCategoria != null)
                {
                    cbCategoria.DataSource = dataSource;
                    cbCategoria.DisplayMember = "Nombre";
                    cbCategoria.ValueMember = "Id";
                    cbCategoria.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar categorías: {ex.Message}");
                if (cbCategoria != null)
                {
                    cbCategoria.DataSource = new List<object> { new { Id = 0, Nombre = "Error" } };
                    cbCategoria.SelectedIndex = 0;
                }
            }
        }

        private async Task RefrescarDesdeDbAsync()
        {
            bool? activo = null;
            if (cbEstado?.SelectedItem != null)
            {
                string sel = cbEstado.SelectedItem.ToString();
                if (sel == "Activo") activo = true;
                else if (sel == "Inactivo") activo = false;
            }

            string q = txtBuscar?.Text.Trim().ToLower() ?? "";
            int? catId = (cbCategoria?.SelectedValue is int id && id > 0) ? id : (int?)null;
            bool soloBajo = chkSoloBajoStock?.Checked ?? false;
            int? selectedProductId = (_bs.Current as InventarioItem)?.IdProducto;

            if (gbLista != null) gbLista.Text = "Cargando...";

            try
            {
                List<InventarioItem> lista = await _repo.ListarAsync(soloBajo, q, activo, catId);
                _data = new BindingList<InventarioItem>(lista);
                _bs.DataSource = _data;
                RefreshKpis();

                if (selectedProductId.HasValue)
                {
                    int newIndex = _data.ToList().FindIndex(item => item.IdProducto == selectedProductId.Value);
                    if (newIndex >= 0)
                        _bs.Position = newIndex;
                    else
                        LimpiarHistorialYDetalle();
                }
                else
                    LimpiarHistorialYDetalle();

                Dgv_SelectionChanged(null, null);

                if (gbLista != null) gbLista.Text = "Lista de Productos";
            }
            catch (Exception ex)
            {
                if (gbLista != null) gbLista.Text = "Error al Cargar";
                MessageBox.Show($"Error al cargar inventario: {ex.Message}", "Error DB");
                _data = new BindingList<InventarioItem>();
                _bs.DataSource = _data;
                RefreshKpis();
                LimpiarHistorialYDetalle();
            }

            if (dgv != null) dgv.ClearSelection();
        }

        private void ConfigurarGridPrincipal()
        {
            if (dgv == null) return;

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
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nombre", DataPropertyName = "NombreProducto", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Categoría", DataPropertyName = "Categoria", Width = 140 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ubicación", DataPropertyName = "Ubicacion", Width = 110 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stock", DataPropertyName = "Stock", Width = 80, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mínimo", DataPropertyName = "Minimo", Width = 80, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Precio", DataPropertyName = "PrecioProducto", Width = 110, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "C", FormatProvider = new CultureInfo("es-AR") } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Proveedor", DataPropertyName = "Proveedor", Width = 140 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Estado", DataPropertyName = "Estado", Width = 100 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Actualizado", DataPropertyName = "Actualizado", Width = 150, DefaultCellStyle = new DataGridViewCellStyle { Format = "g" } });
        }

        private void ConfigurarGridHistorial()
        {
            if (dgvHistorial == null) return;

            dgvHistorial.RowHeadersVisible = false;

            Color header = Color.FromArgb(230, 235, 230);
            Color seleccion = Color.FromArgb(210, 225, 210);
            Color texto = Color.FromArgb(50, 60, 50);

            dgvHistorial.ColumnHeadersDefaultCellStyle.BackColor = header;
            dgvHistorial.ColumnHeadersDefaultCellStyle.ForeColor = texto;
            dgvHistorial.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvHistorial.ColumnHeadersHeight = 30;
            dgvHistorial.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvHistorial.DefaultCellStyle.SelectionBackColor = seleccion;
            dgvHistorial.DefaultCellStyle.SelectionForeColor = texto;
            dgvHistorial.BackgroundColor = Color.WhiteSmoke;
            dgvHistorial.RowTemplate.Height = 24;
            dgvHistorial.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvHistorial.MultiSelect = false;
            dgvHistorial.AllowUserToAddRows = false;
            dgvHistorial.AllowUserToDeleteRows = false;
            dgvHistorial.ReadOnly = true;

            dgvHistorial.AutoGenerateColumns = false;
            dgvHistorial.Columns.Clear();
            dgvHistorial.Columns.Add(new DataGridViewTextBoxColumn { Name = "Fecha", HeaderText = "Fecha", DataPropertyName = "Fecha", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "g" } });
            dgvHistorial.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cantidad", HeaderText = "Cantidad", DataPropertyName = "Cantidad", Width = 60, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvHistorial.Columns.Add(new DataGridViewTextBoxColumn { Name = "Observacion", HeaderText = "Observacion", DataPropertyName = "Observacion", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        }

        private void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || dgv == null) return;

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
                else
                {
                    e.CellStyle.BackColor = (e.RowIndex % 2 == 0) ? dgv.DefaultCellStyle.BackColor : dgv.AlternatingRowsDefaultCellStyle.BackColor;
                    e.CellStyle.ForeColor = dgv.DefaultCellStyle.ForeColor;
                }
            }

            if (dgv.Columns[e.ColumnIndex].DataPropertyName == "Estado")
            {
                e.CellStyle.ForeColor = item.Activo ? Color.DarkGreen : Color.OrangeRed;
            }
        }

        private async void ApplyFilters()
        {
            await RefrescarDesdeDbAsync();
        }

        private void RefreshKpis()
        {
            if (kpiTotalVal == null) return;

            List<InventarioItem> lista = _bs.Cast<InventarioItem>().ToList();
            kpiTotalVal.Text = lista.Count.ToString("N0");
            kpiBajoVal.Text = lista.Count(p => p.Stock < p.Minimo).ToString("N0");
            decimal valorizado = lista.Sum(p => (p.Stock > 0 ? p.Stock : 0) * p.PrecioProducto);
            kpiValVal.Text = valorizado.ToString("C", new CultureInfo("es-AR"));
        }

        private async void Dgv_SelectionChanged(object sender, EventArgs e)
        {
            InventarioItem p = _bs.Current as InventarioItem;

            if (p == null)
            {
                LimpiarHistorialYDetalle();
                return;
            }

            await CargarHistorialAsync(p.IdProducto);
        }

        private async Task CargarHistorialAsync(int idProducto)
        {
            if (lblHistorialTitulo != null)
                lblHistorialTitulo.Text = $"Historial (Prod ID: {idProducto})";

            LimpiarHistorialYDetalle();

            try
            {
                var historial = await _repo.GetHistorialMovimientosAsync(idProducto);
                _historialData = new BindingList<MovimientoStock>(historial);
                _bsHistorial.DataSource = _historialData;

                if (dgvHistorial != null && dgvHistorial.Rows.Count > 0)
                    dgvHistorial.Rows[0].Selected = true;
                else if (txtDetalleAjuste != null)
                    txtDetalleAjuste.Clear();

                if (dgvHistorial != null)
                {
                    dgvHistorial.DataSource = _bsHistorial;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar historial: {ex.Message}");
                _historialData = new BindingList<MovimientoStock>();
                _bsHistorial.DataSource = _historialData;
                LimpiarHistorialYDetalle();
            }
        }

        private void LimpiarHistorialYDetalle()
        {
            _historialData?.Clear();
            if (txtDetalleAjuste != null)
                txtDetalleAjuste.Clear();
            if (lblHistorialTitulo != null)
                lblHistorialTitulo.Text = "Historial de Movimientos";
        }

        private void DgvHistorial_SelectionChanged(object sender, EventArgs e)
        {
            MovimientoStock mov = _bsHistorial.Current as MovimientoStock;
            if (txtDetalleAjuste != null)
            {
                txtDetalleAjuste.Text = mov?.Observacion ?? "";
            }
        }

        private async void BtnEditar_Click(object sender, EventArgs e)
        {
            InventarioItem itemSeleccionado = _bs.Current as InventarioItem;
            if (itemSeleccionado == null)
            {
                MessageBox.Show("Seleccioná un producto para editar.");
                return;
            }

            using (var form = new Form())
            {
                form.Text = "Editar Producto";
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.Width = 420;
                form.Height = 400;

                TableLayoutPanel root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), RowCount = 2 };
                root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
                form.Controls.Add(root);

                TableLayoutPanel grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoSize = true, Padding = new Padding(5) };
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                root.Controls.Add(grid, 0, 0);

                var txtSku = new TextBox { Dock = DockStyle.Fill, Text = itemSeleccionado.Sku };
                var txtNombre = new TextBox { Dock = DockStyle.Fill, Text = itemSeleccionado.NombreProducto };
                var cmbCategoria = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
                var txtUbicacion = new TextBox { Dock = DockStyle.Fill, Text = itemSeleccionado.Ubicacion };
                var numMinimo = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 0, Maximum = 10000, Value = Math.Max(0, itemSeleccionado.Minimo) };
                var numPrecio = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 0, Maximum = 1000000, DecimalPlaces = 2, Value = Math.Max(0, itemSeleccionado.PrecioProducto) };
                var txtProveedor = new TextBox { Dock = DockStyle.Fill, Text = itemSeleccionado.Proveedor };
                var chkActivo = new CheckBox { Text = "Activo", Checked = itemSeleccionado.Activo, Dock = DockStyle.Left };

                var catDataSource = _categoriasDisponibles.Select(c => new { Id = c.IdCategoria, Nombre = c.Nombre }).ToList<object>();
                cmbCategoria.DataSource = catDataSource;
                cmbCategoria.DisplayMember = "Nombre";
                cmbCategoria.ValueMember = "Id";
                cmbCategoria.SelectedValue = itemSeleccionado.IdCategoria;

                int row = 0;
                grid.Controls.Add(new Label { Text = "SKU:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, row);
                grid.Controls.Add(txtSku, 1, row++);
                grid.Controls.Add(new Label { Text = "Nombre:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, row);
                grid.Controls.Add(txtNombre, 1, row++);
                grid.Controls.Add(new Label { Text = "Categoría:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, row);
                grid.Controls.Add(cmbCategoria, 1, row++);
                grid.Controls.Add(new Label { Text = "Ubicación:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, row);
                grid.Controls.Add(txtUbicacion, 1, row++);
                grid.Controls.Add(new Label { Text = "Mínimo:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, row);
                grid.Controls.Add(numMinimo, 1, row++);
                grid.Controls.Add(new Label { Text = "Precio:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, row);
                grid.Controls.Add(numPrecio, 1, row++);
                grid.Controls.Add(new Label { Text = "Proveedor:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, row);
                grid.Controls.Add(txtProveedor, 1, row++);
                grid.Controls.Add(new Label { Text = "Estado:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, row);
                grid.Controls.Add(chkActivo, 1, row++);

                FlowLayoutPanel panelBtns = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill };
                Button btnOk = new Button { Text = "Guardar", DialogResult = DialogResult.None, AutoSize = true };
                Button btnCancel = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, AutoSize = true };
                panelBtns.Controls.Add(btnOk);
                panelBtns.Controls.Add(btnCancel);
                root.Controls.Add(panelBtns, 0, 1);
                form.AcceptButton = btnOk;
                form.CancelButton = btnCancel;

                btnOk.Click += async (s, args) =>
                {
                    form.DialogResult = DialogResult.None;
                    if (string.IsNullOrWhiteSpace(txtNombre.Text))
                    {
                        MessageBox.Show("El nombre no puede estar vacío.");
                        return;
                    }

                    var p = new ProductoEntity

                    {
                        IdProducto = itemSeleccionado.IdProducto,
                        Nombre = txtNombre.Text.Trim(),
                        Descripcion = itemSeleccionado.DescripcionProducto,
                        Precio = numPrecio.Value,
                        Activo = chkActivo.Checked,
                        Sku = txtSku.Text.Trim(),
                        IdCategoria = (int)cmbCategoria.SelectedValue,
                        Ubicacion = txtUbicacion.Text.Trim(),
                        Minimo = (int)numMinimo.Value,
                        Proveedor = txtProveedor.Text.Trim()
                    };

                    try
                    {
                        int v = _repo.ActualizarProducto(p);
                        form.DialogResult = DialogResult.OK;
                        form.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al actualizar: {ex.Message}");
                    }
                };

                if (form.ShowDialog() == DialogResult.OK)
                {
                    await RefrescarDesdeDbAsync();
                }
            }
        }

        private async void BtnAjuste_Click(object sender, EventArgs e)
        {
            InventarioItem itemSeleccionado = _bs.Current as InventarioItem;
            if (itemSeleccionado == null)
            {
                MessageBox.Show("Seleccioná un producto.");
                return;
            }

            using (var form = new FrmAjusteModern(itemSeleccionado))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    await RefrescarDesdeDbAsync();
                }
            }
        }

        private async void BtnImportar_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV (*.csv)|*.csv";
            if (ofd.ShowDialog() != DialogResult.OK) return;

            int ok = 0, err = 0;
            var sbErrores = new StringBuilder();
            var repo = new InventarioRepository();

            foreach (string line in System.IO.File.ReadLines(ofd.FileName).Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] c = line.Split(';');

                try
                {
                    if (c.Length < 7) throw new Exception("Formato de línea incorrecto.");

                    var p = new ProductoEntity { };

                    string skuCsv = (c.Length > 0) ? c[0].Trim() : "";
                    string nombreCsv = (c.Length > 1) ? c[1].Trim() : "";
                    string catCsv = (c.Length > 2 ? c[2].Trim().ToLower() : "general");
                    string ubicacionCsv = (c.Length > 3) ? c[3].Trim() : "";
                    int m;
                    int minimoCsv = (c.Length > 4 && int.TryParse(c[4], out m)) ? m : 5;
                    decimal pr;
                    decimal precioCsv = (c.Length > 5 && decimal.TryParse(c[5], out pr)) ? pr : 0m;
                    string proveedorCsv = (c.Length > 6) ? c[6].Trim() : "";

                    var catEncontrada = _categoriasDisponibles.FirstOrDefault(cat => cat.Nombre.Equals(catCsv, StringComparison.OrdinalIgnoreCase));
                    int catIdCsv = catEncontrada?.IdCategoria ?? 0;

                    if (catIdCsv == 0)
                    {
                        throw new Exception($"Categoría '{catCsv}' no encontrada.");
                    }
                    if (string.IsNullOrEmpty(skuCsv) || string.IsNullOrEmpty(nombreCsv))
                        throw new Exception("SKU o Nombre vacíos.");

                    p.Nombre = nombreCsv;
                    p.Precio = precioCsv;
                    p.Activo = true;
                    p.Descripcion = "";
                    p.Sku = skuCsv;
                    p.IdCategoria = catIdCsv;
                    p.Ubicacion = ubicacionCsv;
                    p.Minimo = minimoCsv;
                    p.Proveedor = proveedorCsv;

                    repo.CrearProducto(p);
                    ok++;
                }
                catch (Exception ex)
                {
                    err++;
                    sbErrores.AppendLine($"Error en línea '{line.Substring(0, Math.Min(line.Length, 30))}...': {ex.Message}");
                }
            }

            await RefrescarDesdeDbAsync();
            MessageBox.Show($"Importación finalizada.\nOK: {ok}\nErrores: {err}\n\nDetalles:\n{sbErrores.ToString()}", "Importación");
        }

        private void BtnExportar_Click(object sender, EventArgs e)
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
                lines.Add(string.Join(";", new string[]
                {
                    i.Sku,
                    i.NombreProducto,
                    i.Categoria,
                    i.Ubicacion,
                    i.Stock.ToString(),
                    i.Minimo.ToString(),
                    i.PrecioProducto.ToString("0.00", CultureInfo.InvariantCulture),
                    i.Proveedor,
                    i.Estado,
                    i.Actualizado.ToString("yyyy-MM-dd HH:mm")
                }));
            }

            try
            {
                System.IO.File.WriteAllLines(sfd.FileName, lines.ToArray(), System.Text.Encoding.UTF8);
                MessageBox.Show("Exportado correctamente.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}");
            }
        }
    }
}