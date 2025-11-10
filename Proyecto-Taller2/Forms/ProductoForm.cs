using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Proyecto_Taller_2.Domain.Models;
using Proyecto_Taller_2.Data.Repositories;
using Proyecto_Taller_2.Data;
using System.Globalization;
using System.Linq;

namespace Proyecto_Taller_2.Forms
{
    public partial class ProductoForm : Form
    {
        public Producto Resultado { get; private set; }

        // AGREGADO: txtCosto
        private TextBox txtSku, txtNombre, txtDescripcion, txtCosto, txtPrecio, txtStock, txtMinimo, txtUbicacion, txtProveedor;
        private ComboBox cmbCategoria;
        private CheckBox chkActivo;
        private Button btnGuardar, btnCancelar, btnNuevaCategoria, btnEditarCategoria;
        private Label lblError;
        private List<Categoria> _categorias;
        private CategoriaRepository _categoriaRepo;

        public ProductoForm(Producto producto = null)
        {
            _categoriaRepo = new CategoriaRepository(BDGeneral.ConnectionString);
            InitializeComponent(producto);
        }

        private void ProductoForm_Load(object sender, EventArgs e)
        {
            // Puedes cargar cosas aquí si necesitas
        }

        private void CargarCategorias()
        {
            try
            {
                _categorias = _categoriaRepo.ObtenerTodas(true);
                cmbCategoria.DataSource = _categorias;
                cmbCategoria.DisplayMember = "Nombre";
                cmbCategoria.ValueMember = "IdCategoria";
                if (_categorias.Count > 0) cmbCategoria.SelectedIndex = 0;
            }
            catch
            {
                _categorias = new List<Categoria>();
            }
        }

        private void InitializeComponent(Producto producto)
        {
            this.SuspendLayout();
            this.Text = producto == null ? "Nuevo Producto" : "Editar Producto";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new System.Drawing.Size(910, 500); // Aumenté un poco la altura
            this.Name = "ProductoForm";

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 7, // Aumenté filas para el Costo
                Padding = new Padding(12)
            };

            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            for (int i = 0; i < 6; i++) mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            this.Controls.Add(mainPanel);

            int row = 0;

            // SKU y Nombre
            mainPanel.Controls.Add(new Label { Text = "SKU:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, row);
            txtSku = new TextBox { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(txtSku, 1, row);
            mainPanel.Controls.Add(new Label { Text = "Nombre:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 2, row);
            txtNombre = new TextBox { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(txtNombre, 3, row++);

            // Descripción
            mainPanel.Controls.Add(new Label { Text = "Descripción:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, row);
            txtDescripcion = new TextBox { Dock = DockStyle.Fill };
            mainPanel.SetColumnSpan(txtDescripcion, 3);
            mainPanel.Controls.Add(txtDescripcion, 1, row++);

            // Categoría
            mainPanel.Controls.Add(new Label { Text = "Categoría:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, row);
            cmbCategoria = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            mainPanel.Controls.Add(cmbCategoria, 1, row);
            var catBtns = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Margin = new Padding(0) };
            btnNuevaCategoria = new Button { Text = "Nueva", Width = 60, Height = 23 };
            btnEditarCategoria = new Button { Text = "Editar", Width = 60, Height = 23 };
            catBtns.Controls.AddRange(new Control[] { btnNuevaCategoria, btnEditarCategoria });
            mainPanel.SetColumnSpan(catBtns, 2);
            mainPanel.Controls.Add(catBtns, 2, row++);

            // --- NUEVO: COSTO Y PRECIO ---
            mainPanel.Controls.Add(new Label { Text = "Costo:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, row);
            txtCosto = new TextBox { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(txtCosto, 1, row);

            mainPanel.Controls.Add(new Label { Text = "Precio Venta:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 2, row);
            txtPrecio = new TextBox { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(txtPrecio, 3, row++);
            // -----------------------------

            // Stock y Mínimo
            mainPanel.Controls.Add(new Label { Text = "Stock Actual:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, row);
            txtStock = new TextBox { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(txtStock, 1, row);
            mainPanel.Controls.Add(new Label { Text = "Stock Mínimo:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 2, row);
            txtMinimo = new TextBox { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(txtMinimo, 3, row++);

            // Ubicación y Proveedor
            mainPanel.Controls.Add(new Label { Text = "Ubicación:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, row);
            txtUbicacion = new TextBox { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(txtUbicacion, 1, row);
            mainPanel.Controls.Add(new Label { Text = "Proveedor:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 2, row);
            txtProveedor = new TextBox { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(txtProveedor, 3, row++);

            // Activo
            mainPanel.Controls.Add(new Label { Text = "Activo:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, row);
            chkActivo = new CheckBox { Dock = DockStyle.Fill, Checked = true };
            mainPanel.Controls.Add(chkActivo, 1, row++);

            // Botones
            var bottomPanel = new Panel { Dock = DockStyle.Fill };
            lblError = new Label { Dock = DockStyle.Top, ForeColor = Color.Red, Height = 20, TextAlign = ContentAlignment.MiddleLeft };
            var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Height = 40 };
            btnCancelar = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, Width = 80, Height = 30 };
            btnGuardar = new Button { Text = "Guardar", DialogResult = DialogResult.None, Width = 80, Height = 30 };
            buttonPanel.Controls.AddRange(new Control[] { btnCancelar, btnGuardar });
            bottomPanel.Controls.AddRange(new Control[] { buttonPanel, lblError });
            mainPanel.SetColumnSpan(bottomPanel, 4);
            mainPanel.Controls.Add(bottomPanel, 0, row);

            this.AcceptButton = btnGuardar;
            this.CancelButton = btnCancelar;
            btnGuardar.Click += BtnGuardar_Click;
            btnNuevaCategoria.Click += BtnNuevaCategoria_Click;
            // btnEditarCategoria.Click += BtnEditarCategoria_Click; // Si tienes este método, descomenta
            this.Load += ProductoForm_Load;
            this.ResumeLayout(false);

            CargarCategorias();
            if (producto != null) CargarDatosProducto(producto);
        }

        private void CargarDatosProducto(Producto producto)
        {
            txtSku.Text = producto.Sku;
            txtNombre.Text = producto.Nombre;
            txtDescripcion.Text = producto.Descripcion;
            txtCosto.Text = Convert.ToDecimal(producto.Costo).ToString("F2", CultureInfo.InvariantCulture);
            txtPrecio.Text = producto.Precio.ToString("F2", CultureInfo.InvariantCulture);
            txtStock.Text = producto.Stock.ToString();
            txtMinimo.Text = producto.Minimo.ToString();
            txtUbicacion.Text = producto.Ubicacion;
            txtProveedor.Text = producto.Proveedor;
            chkActivo.Checked = producto.Activo;

            if (producto.IdCategoria > 0 && _categorias != null)
            {
                cmbCategoria.SelectedValue = producto.IdCategoria;
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            if (string.IsNullOrWhiteSpace(txtNombre.Text)) { lblError.Text = "El nombre es requerido"; txtNombre.Focus(); return; }
            if (cmbCategoria.SelectedValue == null) { lblError.Text = "Seleccione una categoría"; cmbCategoria.Focus(); return; }

            // Validar Costo
            if (!decimal.TryParse(txtCosto.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal costo) || costo < 0)
            {
                lblError.Text = "El costo debe ser un número válido >= 0"; txtCosto.Focus(); return;
            }
            // Validar Precio
            if (!decimal.TryParse(txtPrecio.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal precio) || precio < 0)
            {
                lblError.Text = "El precio debe ser un número válido >= 0"; txtPrecio.Focus(); return;
            }
            if (!int.TryParse(txtStock.Text, out int stock) || stock < 0) { lblError.Text = "Stock inválido"; txtStock.Focus(); return; }
            if (!int.TryParse(txtMinimo.Text, out int minimo) || minimo < 0) { lblError.Text = "Mínimo inválido"; txtMinimo.Focus(); return; }

            var cat = (Categoria)cmbCategoria.SelectedItem;
            Resultado = new Producto
            {
                Sku = txtSku.Text.Trim(),
                Nombre = txtNombre.Text.Trim(),
                Descripcion = txtDescripcion.Text.Trim(),
                IdCategoria = cat.IdCategoria,
                CategoriaNombre = cat.Nombre,
                Costo = costo,   // GUARDAR COSTO
                Precio = precio,
                Stock = stock,
                Minimo = minimo,
                Ubicacion = txtUbicacion.Text.Trim(),
                Proveedor = txtProveedor.Text.Trim(),
                Activo = chkActivo.Checked,
                FechaAlta = DateTime.Now,
                Actualizado = DateTime.Now
            };
            DialogResult = DialogResult.OK;
        }

        // ... (Resto de métodos auxiliares como BtnNuevaCategoria_Click, etc.) ...
        private void BtnNuevaCategoria_Click(object sender, EventArgs e) { /* Tu código existente */ }
        private void BtnEditarCategoria_Click(object sender, EventArgs e) { /* Tu código existente */ }
    }
}