using System.Drawing;
using System.Windows.Forms;
using ProductoEntity = Proyecto_Taller_2.Domain.Entities.Producto;

namespace Proyecto_Taller_2
{
    partial class UcInventario
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador

        private void InitializeComponent()
        {
            System.Windows.Forms.Panel mainPanel;
            System.Windows.Forms.FlowLayoutPanel filtrosPanel;
            System.Windows.Forms.Label lblBuscar;
            System.Windows.Forms.Label lblCat;
            System.Windows.Forms.Label lblEstado;
            System.Windows.Forms.FlowLayoutPanel botonesPanel;
            System.Windows.Forms.FlowLayoutPanel kpiPanel;
            System.Windows.Forms.Label lblKpiTotal;
            System.Windows.Forms.Label lblKpiBajo;
            System.Windows.Forms.Label lblKpiValor;
            System.Windows.Forms.Panel historialPanel;
            System.Windows.Forms.Panel detalleAjustePanel;

            // --- INICIALIZACIÓN DE CONTROLES ---
            mainPanel = new System.Windows.Forms.Panel();
            this.gbLista = new System.Windows.Forms.GroupBox();
            this.dgv = new System.Windows.Forms.DataGridView();
            historialPanel = new System.Windows.Forms.Panel();
            this.lblHistorialTitulo = new System.Windows.Forms.Label();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.dgvHistorial = new System.Windows.Forms.DataGridView();
            detalleAjustePanel = new System.Windows.Forms.Panel();
            this.txtDetalleAjuste = new System.Windows.Forms.TextBox();
            kpiPanel = new System.Windows.Forms.FlowLayoutPanel();
            lblKpiTotal = new System.Windows.Forms.Label();
            this.kpiTotalVal = new System.Windows.Forms.Label();
            lblKpiBajo = new System.Windows.Forms.Label();
            this.kpiBajoVal = new System.Windows.Forms.Label();
            lblKpiValor = new System.Windows.Forms.Label();
            this.kpiValVal = new System.Windows.Forms.Label();
            botonesPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.btnNuevo = new System.Windows.Forms.Button();
            this.btnEntrada = new System.Windows.Forms.Button();
            this.btnSalida = new System.Windows.Forms.Button();
            this.btnAjuste = new System.Windows.Forms.Button();
            this.btnImportar = new System.Windows.Forms.Button();
            this.btnExportar = new System.Windows.Forms.Button();
            this.btnEditar = new System.Windows.Forms.Button();
            filtrosPanel = new System.Windows.Forms.FlowLayoutPanel();
            lblBuscar = new System.Windows.Forms.Label();
            this.txtBuscar = new System.Windows.Forms.TextBox();
            lblCat = new System.Windows.Forms.Label();
            this.cbCategoria = new System.Windows.Forms.ComboBox();
            lblEstado = new System.Windows.Forms.Label();
            this.cbEstado = new System.Windows.Forms.ComboBox();
            this.chkSoloBajoStock = new System.Windows.Forms.CheckBox();

            // Configuración de controles
            mainPanel.SuspendLayout();
            this.gbLista.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            historialPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHistorial)).BeginInit();
            detalleAjustePanel.SuspendLayout();
            kpiPanel.SuspendLayout();
            botonesPanel.SuspendLayout();
            filtrosPanel.SuspendLayout();
            this.SuspendLayout();

            // mainPanel
            mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            mainPanel.Padding = new System.Windows.Forms.Padding(12);
            mainPanel.Controls.Add(historialPanel);      // Historial abajo
            mainPanel.Controls.Add(this.gbLista);         // Grilla en el medio
            mainPanel.Controls.Add(kpiPanel);             // KPIs arriba del todo
            mainPanel.Controls.Add(botonesPanel);         // Botones
            mainPanel.Controls.Add(filtrosPanel);         // Filtros arriba

            // filtrosPanel (Panel de filtros) - ARRIBA
            filtrosPanel.Dock = System.Windows.Forms.DockStyle.Top;
            filtrosPanel.Height = 44;
            filtrosPanel.Padding = new Padding(0, 8, 0, 0);
            filtrosPanel.Controls.Add(lblBuscar);
            filtrosPanel.Controls.Add(this.txtBuscar);
            filtrosPanel.Controls.Add(lblCat);
            filtrosPanel.Controls.Add(this.cbCategoria);
            filtrosPanel.Controls.Add(lblEstado);
            filtrosPanel.Controls.Add(this.cbEstado);
            filtrosPanel.Controls.Add(this.chkSoloBajoStock);

            lblBuscar.Text = "Buscar:";
            lblBuscar.AutoSize = true;
            lblBuscar.Margin = new Padding(0, 6, 4, 0);

            this.txtBuscar.Width = 230;
            this.txtBuscar.Margin = new Padding(0, 3, 12, 0);

            lblCat.Text = "Categoría:";
            lblCat.AutoSize = true;
            lblCat.Margin = new Padding(0, 6, 4, 0);

            this.cbCategoria.Width = 180;
            this.cbCategoria.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbCategoria.Margin = new Padding(0, 3, 12, 0);

            lblEstado.Text = "Estado:";
            lblEstado.AutoSize = true;
            lblEstado.Margin = new Padding(0, 6, 4, 0);

            this.cbEstado.Width = 110;
            this.cbEstado.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbEstado.Margin = new Padding(0, 3, 12, 0);

            this.chkSoloBajoStock.Text = "Solo bajo stock";
            this.chkSoloBajoStock.AutoSize = true;
            this.chkSoloBajoStock.Margin = new Padding(0, 6, 0, 0);

            // botonesPanel (Panel de botones)
            botonesPanel.Dock = System.Windows.Forms.DockStyle.Top;
            botonesPanel.Height = 44;
            botonesPanel.Padding = new Padding(0, 4, 0, 4);
            botonesPanel.Controls.Add(this.btnNuevo);
            botonesPanel.Controls.Add(this.btnEntrada);
            botonesPanel.Controls.Add(this.btnSalida);
            botonesPanel.Controls.Add(this.btnAjuste);
            botonesPanel.Controls.Add(this.btnImportar);
            botonesPanel.Controls.Add(this.btnExportar);
            botonesPanel.Controls.Add(this.btnEditar);

            this.btnNuevo.Text = "Nuevo";
            this.btnNuevo.AutoSize = true;
            this.btnNuevo.Margin = new Padding(0, 0, 6, 0);

            this.btnEntrada.Text = "Entrada";
            this.btnEntrada.AutoSize = true;
            this.btnEntrada.Margin = new Padding(0, 0, 6, 0);

            this.btnSalida.Text = "Salida";
            this.btnSalida.AutoSize = true;
            this.btnSalida.Margin = new Padding(0, 0, 6, 0);

            this.btnAjuste.Text = "Ajuste";
            this.btnAjuste.AutoSize = true;
            this.btnAjuste.Margin = new Padding(0, 0, 6, 0);

            this.btnImportar.Text = "Importar";
            this.btnImportar.AutoSize = true;
            this.btnImportar.Margin = new Padding(0, 0, 6, 0);

            this.btnExportar.Text = "Exportar";
            this.btnExportar.AutoSize = true;
            this.btnExportar.Margin = new Padding(0, 0, 6, 0);

            this.btnEditar.Text = "Editar";
            this.btnEditar.AutoSize = true;
            this.btnEditar.Margin = new Padding(0, 0, 0, 0);

            // kpiPanel (Indicadores)
            kpiPanel.Dock = System.Windows.Forms.DockStyle.Top;
            kpiPanel.Height = 30;
            kpiPanel.Padding = new Padding(0, 4, 0, 4);
            kpiPanel.Controls.Add(lblKpiTotal);
            kpiPanel.Controls.Add(this.kpiTotalVal);
            kpiPanel.Controls.Add(lblKpiBajo);
            kpiPanel.Controls.Add(this.kpiBajoVal);
            kpiPanel.Controls.Add(lblKpiValor);
            kpiPanel.Controls.Add(this.kpiValVal);

            lblKpiTotal.Text = "Total:";
            lblKpiTotal.AutoSize = true;
            lblKpiTotal.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblKpiTotal.Margin = new Padding(0, 2, 4, 0);

            this.kpiTotalVal.Text = "0";
            this.kpiTotalVal.AutoSize = true;
            this.kpiTotalVal.Margin = new Padding(0, 2, 16, 0);

            lblKpiBajo.Text = "Bajo stock:";
            lblKpiBajo.AutoSize = true;
            lblKpiBajo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblKpiBajo.Margin = new Padding(0, 2, 4, 0);

            this.kpiBajoVal.Text = "0";
            this.kpiBajoVal.AutoSize = true;
            this.kpiBajoVal.ForeColor = Color.Maroon;
            this.kpiBajoVal.Margin = new Padding(0, 2, 16, 0);

            lblKpiValor.Text = "Valorizado:";
            lblKpiValor.AutoSize = true;
            lblKpiValor.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblKpiValor.Margin = new Padding(0, 2, 4, 0);

            this.kpiValVal.Text = "$0";
            this.kpiValVal.AutoSize = true;
            this.kpiValVal.ForeColor = Color.DarkGreen;
            this.kpiValVal.Margin = new Padding(0, 2, 0, 0);

            // gbLista (Grupo para grilla principal)
            this.gbLista.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbLista.Padding = new System.Windows.Forms.Padding(8);
            this.gbLista.Text = "Inventario";
            this.gbLista.Controls.Add(this.dgv);

            // dgv (Grilla Principal)
            this.dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv.AllowUserToAddRows = false;
            this.dgv.AllowUserToDeleteRows = false;
            this.dgv.ReadOnly = true;
            this.dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgv.MultiSelect = false;

            // historialPanel (Panel inferior con historial)
            historialPanel.Dock = DockStyle.Bottom;
            historialPanel.Height = 220;
            historialPanel.Padding = new Padding(0, 8, 0, 0);
            historialPanel.Controls.Add(this.splitContainer2);
            historialPanel.Controls.Add(this.lblHistorialTitulo);

            // lblHistorialTitulo
            this.lblHistorialTitulo.Dock = DockStyle.Top;
            this.lblHistorialTitulo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.lblHistorialTitulo.Padding = new Padding(4, 0, 0, 4);
            this.lblHistorialTitulo.Text = "Historial de Movimientos";

            // splitContainer2 (Historial / Detalle)
            this.splitContainer2.Dock = DockStyle.Fill;
            this.splitContainer2.Orientation = Orientation.Vertical;
            this.splitContainer2.SplitterDistance = 600;
            this.splitContainer2.Panel1.Controls.Add(this.dgvHistorial);
            this.splitContainer2.Panel2.Controls.Add(detalleAjustePanel);

            // dgvHistorial
            this.dgvHistorial.Dock = DockStyle.Fill;
            this.dgvHistorial.AllowUserToAddRows = false;
            this.dgvHistorial.AllowUserToDeleteRows = false;
            this.dgvHistorial.ReadOnly = true;
            this.dgvHistorial.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvHistorial.MultiSelect = false;

            // detalleAjustePanel
            detalleAjustePanel.Dock = DockStyle.Fill;
            detalleAjustePanel.Padding = new Padding(8, 4, 4, 4);
            detalleAjustePanel.Controls.Add(this.txtDetalleAjuste);

            // txtDetalleAjuste
            this.txtDetalleAjuste.Dock = DockStyle.Fill;
            this.txtDetalleAjuste.Multiline = true;
            this.txtDetalleAjuste.ReadOnly = true;
            this.txtDetalleAjuste.ScrollBars = ScrollBars.Vertical;
            this.txtDetalleAjuste.BackColor = Color.WhiteSmoke;
            this.txtDetalleAjuste.BorderStyle = BorderStyle.FixedSingle;

            // --- Control Principal ---
            this.Controls.Add(mainPanel);
            this.Name = "UcInventario";
            this.Size = new System.Drawing.Size(980, 660);

            mainPanel.ResumeLayout(false);
            this.gbLista.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            historialPanel.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvHistorial)).EndInit();
            detalleAjustePanel.ResumeLayout(false);
            detalleAjustePanel.PerformLayout();
            kpiPanel.ResumeLayout(false);
            kpiPanel.PerformLayout();
            botonesPanel.ResumeLayout(false);
            botonesPanel.PerformLayout();
            filtrosPanel.ResumeLayout(false);
            filtrosPanel.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        // Declaraciones de los controles principales
        private System.Windows.Forms.TextBox txtBuscar;
        private System.Windows.Forms.ComboBox cbCategoria;
        private System.Windows.Forms.ComboBox cbEstado;
        private System.Windows.Forms.CheckBox chkSoloBajoStock;
        private System.Windows.Forms.DataGridView dgv;
        private System.Windows.Forms.Button btnAjuste;
        private System.Windows.Forms.Button btnImportar;
        private System.Windows.Forms.Button btnExportar;
        private System.Windows.Forms.Button btnEditar;
        private System.Windows.Forms.Label kpiTotalVal;
        private System.Windows.Forms.Label kpiBajoVal;
        private System.Windows.Forms.Label kpiValVal;
        private System.Windows.Forms.Label lblHistorialTitulo;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.DataGridView dgvHistorial;
        private System.Windows.Forms.TextBox txtDetalleAjuste;
        private System.Windows.Forms.GroupBox gbLista;

        // Controles ocultos (compatibilidad)
        private System.Windows.Forms.Button btnNuevo;
        private System.Windows.Forms.Button btnEntrada;
        private System.Windows.Forms.Button btnSalida;
    }
}