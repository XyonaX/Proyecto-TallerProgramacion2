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
        
        private TextBox txtSku, txtNombre, txtDescripcion, txtPrecio, txtStock, txtMinimo, txtUbicacion, txtProveedor;
        private ComboBox cmbCategoria;
        private CheckBox chkActivo;
        private Button btnGuardar, btnCancelar, btnNuevaCategoria, btnEditarCategoria;

        private void ProductoForm_Load(object sender, EventArgs e)
        {

        }

        private Label lblError;
        private List<Categoria> _categorias;
        private CategoriaRepository _categoriaRepo;
        
        public ProductoForm(Producto producto = null)
        {
            _categoriaRepo = new CategoriaRepository(BDGeneral.ConnectionString);
            InitializeComponent(producto);
        }
        
        private void CargarCategorias()
        {
            try
            {
                _categorias = _categoriaRepo.ObtenerTodas(true); // Solo activas
                
                cmbCategoria.DataSource = _categorias;
                cmbCategoria.DisplayMember = "Nombre";
                cmbCategoria.ValueMember = "IdCategoria";
                
                if (_categorias.Count > 0)
                    cmbCategoria.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar categorías: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                
                // Fallback: crear categorías básicas
                _categorias = new List<Categoria>
                {
                    new Categoria { IdCategoria = 1, Nombre = "Remera" },
                    new Categoria { IdCategoria = 2, Nombre = "Campera" }
                };
                cmbCategoria.DataSource = _categorias;
                cmbCategoria.DisplayMember = "Nombre";
                cmbCategoria.ValueMember = "IdCategoria";
                cmbCategoria.SelectedIndex = 0;
            }
        }
        
        private void InitializeComponent(Producto producto)
        {
            this.SuspendLayout();
            
            // Form properties
            this.Text = producto == null ? "Nuevo Producto" : "Editar Producto";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new System.Drawing.Size(910, 471);
            this.Name = "ProductoForm";
            
            // Main container
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 6,
                Padding = new Padding(12)
            };
            
            // Column styles
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            
            // Row styles
            for (int i = 0; i < 5; i++)
                mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            
            this.Controls.Add(mainPanel);
            
            int row = 0;
            
            // SKU
            mainPanel.Controls.Add(new Label { Text = "SKU:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, row);
            txtSku = new TextBox { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(txtSku, 1, row);
            
            // Nombre
            mainPanel.Controls.Add(new Label { Text = "Nombre:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 2, row);
            txtNombre = new TextBox { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(txtNombre, 3, row++);
            
            // Descripción
            mainPanel.Controls.Add(new Label { Text = "Descripción:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, row);
            txtDescripcion = new TextBox { Dock = DockStyle.Fill };
            mainPanel.SetColumnSpan(txtDescripcion, 3);
            mainPanel.Controls.Add(txtDescripcion, 1, row++);
            
            // Categoría y botones de categoría
            mainPanel.Controls.Add(new Label { Text = "Categoría:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, row);
            cmbCategoria = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            mainPanel.Controls.Add(cmbCategoria, 1, row);
            
            var categoryButtonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0)
            };
            
            btnNuevaCategoria = new Button { Text = "Nueva", Width = 60, Height = 23 };
            btnEditarCategoria = new Button { Text = "Editar", Width = 60, Height = 23 };
            
            categoryButtonPanel.Controls.Add(btnNuevaCategoria);
            categoryButtonPanel.Controls.Add(btnEditarCategoria);
            
            mainPanel.SetColumnSpan(categoryButtonPanel, 2);
            mainPanel.Controls.Add(categoryButtonPanel, 2, row++);
            
            // Precio y Stock
            mainPanel.Controls.Add(new Label { Text = "Precio:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, row);
            txtPrecio = new TextBox { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(txtPrecio, 1, row);
            
            mainPanel.Controls.Add(new Label { Text = "Stock:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 2, row);
            txtStock = new TextBox { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(txtStock, 3, row++);
            
            // Mínimo y Ubicación
            mainPanel.Controls.Add(new Label { Text = "Mínimo:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, row);
            txtMinimo = new TextBox { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(txtMinimo, 1, row);
            
            mainPanel.Controls.Add(new Label { Text = "Ubicación:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 2, row);
            txtUbicacion = new TextBox { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(txtUbicacion, 3, row++);
            
            // Proveedor y Activo
            mainPanel.Controls.Add(new Label { Text = "Proveedor:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, row);
            txtProveedor = new TextBox { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(txtProveedor, 1, row);
            
            mainPanel.Controls.Add(new Label { Text = "Activo:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 2, row);
            chkActivo = new CheckBox { Dock = DockStyle.Fill, Checked = true };
            mainPanel.Controls.Add(chkActivo, 3, row++);
            
            // Error y botones en la última fila
            var bottomPanel = new Panel { Dock = DockStyle.Fill };
            
            lblError = new Label 
            { 
                Dock = DockStyle.Top, 
                ForeColor = Color.Red, 
                Height = 20,
                TextAlign = ContentAlignment.MiddleLeft
            };
            
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 40
            };
            
            btnCancelar = new Button 
            { 
                Text = "Cancelar", 
                DialogResult = DialogResult.Cancel,
                Width = 80,
                Height = 30
            };
            btnGuardar = new Button 
            { 
                Text = "Guardar", 
                DialogResult = DialogResult.None,
                Width = 80,
                Height = 30
            };
            
            buttonPanel.Controls.Add(btnCancelar);
            buttonPanel.Controls.Add(btnGuardar);
            
            bottomPanel.Controls.Add(buttonPanel);
            bottomPanel.Controls.Add(lblError);
            
            mainPanel.SetColumnSpan(bottomPanel, 4);
            mainPanel.Controls.Add(bottomPanel, 0, row);
            
            // Set form properties
            this.AcceptButton = btnGuardar;
            this.CancelButton = btnCancelar;
            
            // Wire up event handlers
            btnGuardar.Click += BtnGuardar_Click;
            btnNuevaCategoria.Click += BtnNuevaCategoria_Click;
            btnEditarCategoria.Click += BtnEditarCategoria_Click;
            this.Load += ProductoForm_Load;
            
            this.ResumeLayout(false);
            
            // Load categories after controls are created
            CargarCategorias();
            
            // Load product data if editing
            if (producto != null)
            {
                CargarDatosProducto(producto);
            }
        }
        
        private void CargarDatosProducto(Producto producto)
        {
            txtSku.Text = producto.Sku;
            txtNombre.Text = producto.Nombre;
            txtDescripcion.Text = producto.Descripcion;
            txtPrecio.Text = producto.Precio.ToString("F2", CultureInfo.InvariantCulture);
            txtStock.Text = producto.Stock.ToString();
            txtMinimo.Text = producto.Minimo.ToString();
            txtUbicacion.Text = producto.Ubicacion;
            txtProveedor.Text = producto.Proveedor;
            chkActivo.Checked = producto.Activo;
            
            // Seleccionar categoría
            if (producto.IdCategoria > 0 && _categorias != null)
            {
                var categoria = _categorias.FirstOrDefault(c => c.IdCategoria == producto.IdCategoria);
                if (categoria != null)
                {
                    cmbCategoria.SelectedValue = categoria.IdCategoria;
                }
            }
        }
        
        private void BtnNuevaCategoria_Click(object sender, EventArgs e)
        {
            using (var form = new CategoriaForm())
            {
                if (form.ShowDialog() == DialogResult.OK && form.Resultado != null)
                {
                    try
                    {
                        int nuevoId = _categoriaRepo.Agregar(form.Resultado);
                        CargarCategorias(); // Recargar categorías
                        
                        // Seleccionar la nueva categoría
                        cmbCategoria.SelectedValue = nuevoId;
                        
                        MessageBox.Show("Categoría creada correctamente.", "Éxito", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al crear categoría: {ex.Message}", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        
        private void BtnEditarCategoria_Click(object sender, EventArgs e)
        {
            if (cmbCategoria.SelectedValue == null) 
            {
                MessageBox.Show("Seleccione una categoría para editar.", "Información", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            int categoriaId = (int)cmbCategoria.SelectedValue;
            var categoria = _categoriaRepo.ObtenerPorId(categoriaId);
            
            if (categoria == null)
            {
                MessageBox.Show("No se pudo cargar la categoría seleccionada.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            using (var form = new CategoriaForm(categoria))
            {
                if (form.ShowDialog() == DialogResult.OK && form.Resultado != null)
                {
                    try
                    {
                        form.Resultado.IdCategoria = categoriaId;
                        _categoriaRepo.Actualizar(form.Resultado);
                        CargarCategorias(); // Recargar categorías
                        
                        // Mantener la categoría seleccionada
                        cmbCategoria.SelectedValue = categoriaId;
                        
                        MessageBox.Show("Categoría actualizada correctamente.", "Éxito", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al actualizar categoría: {ex.Message}", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        
        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                lblError.Text = "El nombre es requerido";
                txtNombre.Focus();
                return;
            }
            
            if (cmbCategoria.SelectedValue == null)
            {
                lblError.Text = "Debe seleccionar una categoría";
                cmbCategoria.Focus();
                return;
            }
            
            if (!decimal.TryParse(txtPrecio.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal precio) || precio < 0)
            {
                lblError.Text = "El precio debe ser un número válido mayor o igual a 0";
                txtPrecio.Focus();
                return;
            }
            
            if (!int.TryParse(txtStock.Text, out int stock) || stock < 0)
            {
                lblError.Text = "El stock debe ser un número entero válido mayor o igual a 0";
                txtStock.Focus();
                return;
            }
            
            if (!int.TryParse(txtMinimo.Text, out int minimo) || minimo < 0)
            {
                lblError.Text = "El mínimo debe ser un número entero válido mayor o igual a 0";
                txtMinimo.Focus();
                return;
            }
            
            var categoriaSeleccionada = (Categoria)cmbCategoria.SelectedItem;
            
            Resultado = new Producto
            {
                Sku = txtSku.Text.Trim(),
                Nombre = txtNombre.Text.Trim(),
                Descripcion = txtDescripcion.Text.Trim(),
                IdCategoria = categoriaSeleccionada.IdCategoria,
                CategoriaNombre = categoriaSeleccionada.Nombre,
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
    }
    
    // Formulario para gestionar categorías
    public partial class CategoriaForm : Form
    {
        public Categoria Resultado { get; private set; }
        
        private TextBox txtNombre, txtDescripcion;
        private CheckBox chkActivo;
        private Button btnGuardar, btnCancelar;
        private Label lblError;
        
        public CategoriaForm(Categoria categoria = null)
        {
            InitializeComponent(categoria);
        }
        
        private void InitializeComponent(Categoria categoria)
        {
            Text = categoria == null ? "Nueva Categoría" : "Editar Categoría";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Size = new Size(400, 220);
            
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(12)
            };
            
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            
            for (int i = 0; i < 4; i++)
                mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            
            Controls.Add(mainPanel);
            
            int row = 0;
            
            // Nombre
            mainPanel.Controls.Add(new Label { Text = "Nombre:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, row);
            txtNombre = new TextBox { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(txtNombre, 1, row++);
            
            // Descripción
            mainPanel.Controls.Add(new Label { Text = "Descripción:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, row);
            txtDescripcion = new TextBox { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(txtDescripcion, 1, row++);
            
            // Estado
            mainPanel.Controls.Add(new Label { Text = "Activo:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, row);
            chkActivo = new CheckBox { Dock = DockStyle.Fill, Checked = true };
            mainPanel.Controls.Add(chkActivo, 1, row++);
            
            // Error y botones
            mainPanel.Controls.Add(new Label { Text = "", Dock = DockStyle.Fill }, 0, row);
            var bottomPanel = new Panel { Dock = DockStyle.Fill };
            
            lblError = new Label 
            { 
                Dock = DockStyle.Top, 
                ForeColor = Color.Red, 
                Height = 20,
                TextAlign = ContentAlignment.MiddleLeft
            };
            
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 40
            };
            
            btnCancelar = new Button 
            { 
                Text = "Cancelar", 
                DialogResult = DialogResult.Cancel,
                Width = 80,
                Height = 30
            };
            btnGuardar = new Button 
            { 
                Text = "Guardar", 
                DialogResult = DialogResult.None,
                Width = 80,
                Height = 30
            };
            
            buttonPanel.Controls.Add(btnCancelar);
            buttonPanel.Controls.Add(btnGuardar);
            
            bottomPanel.Controls.Add(buttonPanel);
            bottomPanel.Controls.Add(lblError);
            
            mainPanel.Controls.Add(bottomPanel, 1, row);
            
            AcceptButton = btnGuardar;
            CancelButton = btnCancelar;
            
            btnGuardar.Click += BtnGuardar_Click;
            
            // Cargar datos si es edición
            if (categoria != null)
            {
                txtNombre.Text = categoria.Nombre;
                txtDescripcion.Text = categoria.Descripcion;
                chkActivo.Checked = categoria.Activo;
            }
        }
        
        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                lblError.Text = "El nombre es requerido";
                txtNombre.Focus();
                return;
            }
            
            Resultado = new Categoria
            {
                Nombre = txtNombre.Text.Trim(),
                Descripcion = txtDescripcion.Text.Trim(),
                Activo = chkActivo.Checked,
                FechaCreacion = DateTime.Now
            };
            
            DialogResult = DialogResult.OK;
        }
    }
}