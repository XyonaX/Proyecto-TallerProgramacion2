namespace Proyecto_Taller_2
{
    partial class UcInventario
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador

        private void InitializeComponent()
        {
            this.SuspendLayout();

            var mainPanel = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                Padding = new System.Windows.Forms.Padding(12)
            };
            this.Controls.Add(mainPanel);

            // ====== Filtros ======
            var filtrosPanel = new System.Windows.Forms.FlowLayoutPanel
            {
                Dock = System.Windows.Forms.DockStyle.Top,
                Height = 44,
                FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight,
                Padding = new System.Windows.Forms.Padding(0, 0, 0, 8)
            };
            mainPanel.Controls.Add(filtrosPanel);

            this.txtBuscar = new System.Windows.Forms.TextBox { Width = 230, Margin = new System.Windows.Forms.Padding(4) };

            // 👇 Solo selección (remera/campera)
            this.cbCategoria = new System.Windows.Forms.ComboBox
            {
                Width = 180,
                Margin = new System.Windows.Forms.Padding(4),
                DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            };

            this.cbEstado = new System.Windows.Forms.ComboBox
            {
                Width = 110,
                Margin = new System.Windows.Forms.Padding(4),
                DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            };

            this.chkSoloBajoStock = new System.Windows.Forms.CheckBox
            {
                Text = "Solo bajo stock",
                Margin = new System.Windows.Forms.Padding(10, 10, 4, 4)
            };

            filtrosPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Buscar:", AutoSize = true, Margin = new System.Windows.Forms.Padding(4, 12, 4, 4) });
            filtrosPanel.Controls.Add(this.txtBuscar);
            filtrosPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Categoría:", AutoSize = true, Margin = new System.Windows.Forms.Padding(12, 12, 4, 4) });
            filtrosPanel.Controls.Add(this.cbCategoria);
            filtrosPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Estado:", AutoSize = true, Margin = new System.Windows.Forms.Padding(12, 12, 4, 4) });
            filtrosPanel.Controls.Add(this.cbEstado);
            filtrosPanel.Controls.Add(this.chkSoloBajoStock);

            // ====== Botones ======
            var botonesPanel = new System.Windows.Forms.FlowLayoutPanel
            {
                Dock = System.Windows.Forms.DockStyle.Top,
                Height = 44,
                FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight,
                Padding = new System.Windows.Forms.Padding(0, 0, 0, 8)
            };
            mainPanel.Controls.Add(botonesPanel);

            this.btnNuevo = new System.Windows.Forms.Button { Text = "Nuevo", Width = 90, Margin = new System.Windows.Forms.Padding(4) };
            this.btnEntrada = new System.Windows.Forms.Button { Text = "Entrada", Width = 90, Margin = new System.Windows.Forms.Padding(4) };
            this.btnSalida = new System.Windows.Forms.Button { Text = "Salida", Width = 90, Margin = new System.Windows.Forms.Padding(4) };
            this.btnAjuste = new System.Windows.Forms.Button { Text = "Ajuste", Width = 90, Margin = new System.Windows.Forms.Padding(4) };
            this.btnImportar = new System.Windows.Forms.Button { Text = "Importar", Width = 90, Margin = new System.Windows.Forms.Padding(4) };
            this.btnExportar = new System.Windows.Forms.Button { Text = "Exportar", Width = 90, Margin = new System.Windows.Forms.Padding(4) };
            this.btnEditar = new System.Windows.Forms.Button { Text = "Editar", Width = 90, Margin = new System.Windows.Forms.Padding(4) };

            botonesPanel.Controls.Add(this.btnNuevo);
            botonesPanel.Controls.Add(this.btnEntrada);
            botonesPanel.Controls.Add(this.btnSalida);
            botonesPanel.Controls.Add(this.btnAjuste);
            botonesPanel.Controls.Add(this.btnImportar);
            botonesPanel.Controls.Add(this.btnExportar);
            botonesPanel.Controls.Add(this.btnEditar);

            // ====== KPIs ======
            var kpiPanel = new System.Windows.Forms.FlowLayoutPanel
            {
                Dock = System.Windows.Forms.DockStyle.Top,
                Height = 48,
                FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight,
                Padding = new System.Windows.Forms.Padding(0, 0, 0, 8)
            };
            mainPanel.Controls.Add(kpiPanel);

            var fontKpiTitle = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            var fontKpiVal = new System.Drawing.Font("Segoe UI", 11.5F, System.Drawing.FontStyle.Bold);

            kpiPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Total:", AutoSize = true, Font = fontKpiTitle, Margin = new System.Windows.Forms.Padding(6, 14, 6, 0) });
            this.kpiTotalVal = new System.Windows.Forms.Label { Text = "0", AutoSize = true, Font = fontKpiVal, Margin = new System.Windows.Forms.Padding(0, 12, 16, 0) };
            kpiPanel.Controls.Add(this.kpiTotalVal);

            kpiPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Bajo stock:", AutoSize = true, Font = fontKpiTitle, Margin = new System.Windows.Forms.Padding(16, 14, 6, 0) });
            this.kpiBajoVal = new System.Windows.Forms.Label { Text = "0", AutoSize = true, Font = fontKpiVal, Margin = new System.Windows.Forms.Padding(0, 12, 16, 0) };
            kpiPanel.Controls.Add(this.kpiBajoVal);

            kpiPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Valorizado:", AutoSize = true, Font = fontKpiTitle, Margin = new System.Windows.Forms.Padding(16, 14, 6, 0) });
            this.kpiValVal = new System.Windows.Forms.Label { Text = "$ 0", AutoSize = true, Font = fontKpiVal, Margin = new System.Windows.Forms.Padding(0, 12, 6, 0) };
            kpiPanel.Controls.Add(this.kpiValVal);

            // ====== Grilla ======
            this.dgv = new System.Windows.Forms.DataGridView
            {
                Dock = System.Windows.Forms.DockStyle.Top,
                Height = 260,
                Margin = new System.Windows.Forms.Padding(4),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 28 }
            };
            mainPanel.Controls.Add(this.dgv);

            // ====== Detalle ======
            var detallePanel = new System.Windows.Forms.FlowLayoutPanel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                FlowDirection = System.Windows.Forms.FlowDirection.TopDown,
                Padding = new System.Windows.Forms.Padding(0, 8, 0, 0)
            };
            mainPanel.Controls.Add(detallePanel);

            var fontDetTitle = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            var fontDet = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular);

            this.lblDetNombre = new System.Windows.Forms.Label { Text = "Nombre: —", AutoSize = true, Font = fontDet, Margin = new System.Windows.Forms.Padding(4) };
            this.lblDetSku = new System.Windows.Forms.Label { Text = "SKU: —", AutoSize = true, Font = fontDet, Margin = new System.Windows.Forms.Padding(4) };
            this.lblDetCat = new System.Windows.Forms.Label { Text = "Categoría: —", AutoSize = true, Font = fontDet, Margin = new System.Windows.Forms.Padding(4) };
            this.lblDetUbic = new System.Windows.Forms.Label { Text = "Ubicación: —", AutoSize = true, Font = fontDet, Margin = new System.Windows.Forms.Padding(4) };
            this.lblDetStock = new System.Windows.Forms.Label { Text = "Stock / Mín.: —", AutoSize = true, Font = fontDetTitle, Margin = new System.Windows.Forms.Padding(4) };
            this.lblDetPrecio = new System.Windows.Forms.Label { Text = "Precio: —", AutoSize = true, Font = fontDet, Margin = new System.Windows.Forms.Padding(4) };
            this.lblDetActualizado = new System.Windows.Forms.Label { Text = "Actualizado: —", AutoSize = true, Font = fontDet, Margin = new System.Windows.Forms.Padding(4) };

            detallePanel.Controls.Add(this.lblDetNombre);
            detallePanel.Controls.Add(this.lblDetSku);
            detallePanel.Controls.Add(this.lblDetCat);
            detallePanel.Controls.Add(this.lblDetUbic);
            detallePanel.Controls.Add(this.lblDetStock);
            detallePanel.Controls.Add(this.lblDetPrecio);
            detallePanel.Controls.Add(this.lblDetActualizado);

            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "UcInventario";
            this.Size = new System.Drawing.Size(980, 580);

            this.ResumeLayout(false);
        }

        #endregion
    }
}
