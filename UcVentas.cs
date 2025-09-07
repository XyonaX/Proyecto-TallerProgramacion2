using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Proyecto_Taller_2
{
    public class UcVentas : UserControl
    {
        // ===== Placeholder (cue banner) para TextBox en .NET Framework =====
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;
        private static void SetPlaceholder(TextBox tb, string text)
        {
            // wParam = 1 => se oculta el placeholder al enfocar
            SendMessage(tb.Handle, EM_SETCUEBANNER, 1, text);
        }

        // ===== Controles =====
        private TableLayoutPanel tlRoot;          // layout vertical principal
        private FlowLayoutPanel flAcciones;       // botones de acciones (derecha)
        private TableLayoutPanel tlKpis;          // 4 KPI cards
        private Panel cardVentasMes, cardOrdenes, cardTicket, cardCotizaciones;

        private GroupBox gbBuscar;                // bloque "Buscar y Filtrar"
        private TextBox txtBuscar;
        private ComboBox cbEstado, cbTipo, cbPeriodo, cbVendedor;
        private Button btnAplicarFiltros;

        private GroupBox gbLista;                 // bloque "Lista de Ventas"
        private DataGridView dgv;

        public UcVentas()
        {
            DoubleBuffered = true;
            Dock = DockStyle.Fill;
            BackColor = Color.White;

            BuildUI();
            CargarDatosPrueba();
        }

        // ================ UI ==================
        private void BuildUI()
        {
            // --------- Root layout (vertical) ----------
            tlRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Color.White
            };
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 75)); // acciones (top)
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 140)); // KPIs
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 160)); // Buscar / Filtros
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Lista
            Controls.Add(tlRoot);

            // --------- Acciones (derecha) ----------
            flAcciones = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 20, 20, 0),  
                BackColor = Color.White
            };
            var btnNuevaVenta = MakeActionButton(" +  Nueva Venta");
            var btnExportar = MakeGhostButton(" Exportar ");
            var btnNuevaCot = MakeGhostButton(" Nueva Cotización ");
            flAcciones.Controls.Add(btnNuevaVenta);
            flAcciones.Controls.Add(btnExportar);
            flAcciones.Controls.Add(btnNuevaCot);
            tlRoot.Controls.Add(flAcciones, 0, 0);

            // --------- KPIs ----------
            tlKpis = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Padding = new Padding(16, 0, 16, 0)
            };
            for (int i = 0; i < 4; i++)
                tlKpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            cardVentasMes = MakeKpiCard("Ventas del Mes", "$847,392", "+15.3% vs. mes anterior");
            cardOrdenes = MakeKpiCard("Órdenes", "156", "+8.1% este mes");
            cardTicket = MakeKpiCard("Ticket Promedio", "$5,432", "+3.2% vs. promedio");
            cardCotizaciones = MakeKpiCard("Cotizaciones", "43", "+12.5% pendientes");

            tlKpis.Controls.Add(cardVentasMes, 0, 0);
            tlKpis.Controls.Add(cardOrdenes, 1, 0);
            tlKpis.Controls.Add(cardTicket, 2, 0);
            tlKpis.Controls.Add(cardCotizaciones, 3, 0);
            tlRoot.Controls.Add(tlKpis, 0, 1);

            // --------- Buscar y Filtrar ----------
            gbBuscar = new GroupBox
            {
                Text = "Buscar y Filtrar Ventas",
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                ForeColor = Color.FromArgb(34, 47, 34)
            };
            var tlBuscar = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3,
            };
            tlBuscar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            tlBuscar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            tlBuscar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            tlBuscar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            tlBuscar.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // caja de búsqueda
            tlBuscar.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // combos 1
            tlBuscar.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // combos 2 + botón

            // Fila 1: búsqueda
            var pnlSearch = new Panel { Dock = DockStyle.Fill };
            txtBuscar = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
            };
            // Placeholder para .NET Framework
            SetPlaceholder(txtBuscar, "Buscar por número, cliente o producto...");

            pnlSearch.Padding = new Padding(0, 2, 0, 0);
            pnlSearch.Controls.Add(txtBuscar);
            tlBuscar.SetColumnSpan(pnlSearch, 4);
            tlBuscar.Controls.Add(pnlSearch, 0, 0);

            // Fila 2: Estado, Tipo, Periodo, Vendedor
            cbEstado = MakeCombo(new[] { "Todos los estados", "Pendiente", "Completada", "Cancelada" });
            cbTipo = MakeCombo(new[] { "Todos los tipos", "Venta", "Cotización", "Devolución" });
            cbPeriodo = MakeCombo(new[] { "Todos los períodos", "Este mes", "Mes anterior", "Últimos 90 días" });
            cbVendedor = MakeCombo(new[] { "Todos los vendedores", "Juan Pérez", "Ana García", "Marcos López" });

            tlBuscar.Controls.Add(cbEstado, 0, 1);
            tlBuscar.Controls.Add(cbTipo, 1, 1);
            tlBuscar.Controls.Add(cbPeriodo, 2, 1);
            tlBuscar.Controls.Add(cbVendedor, 3, 1);

            // Fila 3: botón aplicar (col 4)
            btnAplicarFiltros = new Button
            {
                Text = "Aplicar filtros",
                Dock = DockStyle.Right,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                Margin = new Padding(0, 2, 0, 0)
            };
            btnAplicarFiltros.FlatAppearance.BorderSize = 0;

            var pnlApply = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 2, 0, 0) };
            pnlApply.Controls.Add(btnAplicarFiltros);
            tlBuscar.Controls.Add(new Panel(), 0, 2);
            tlBuscar.Controls.Add(new Panel(), 1, 2);
            tlBuscar.Controls.Add(new Panel(), 2, 2);
            tlBuscar.Controls.Add(pnlApply, 3, 2);

            gbBuscar.Controls.Add(tlBuscar);
            tlRoot.Controls.Add(gbBuscar, 0, 2);

            // --------- Lista de Ventas ----------
            gbLista = new GroupBox
            {
                Text = "Lista de Ventas",
                Dock = DockStyle.Fill,
                Padding = new Padding(8),
                ForeColor = Color.FromArgb(34, 47, 34)
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
                ColumnHeadersHeight = 36
            };
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(236, 243, 236);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(34, 47, 34);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 232, 220);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.RowTemplate.Height = 32;

            // columnas
            dgv.Columns.Add(MakeCol("Numero", "Número"));
            dgv.Columns.Add(MakeCol("Cliente", "Cliente"));
            dgv.Columns.Add(MakeCol("Tipo", "Tipo"));
            dgv.Columns.Add(MakeCol("Estado", "Estado"));
            dgv.Columns.Add(MakeCol("Total", "Total"));
            dgv.Columns.Add(MakeCol("Fecha", "Fecha"));
            dgv.Columns.Add(MakeCol("Vendedor", "Vendedor"));
            dgv.Columns.Add(MakeCol("Acciones", "Acciones"));

            gbLista.Controls.Add(dgv);
            tlRoot.Controls.Add(gbLista, 0, 3);
        }

        private Button MakeActionButton(string text)
        {
            var b = new Button
            {
                Text = text,
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                Margin = new Padding(8, 6, 0, 0),
                Height = 34,
                Padding = new Padding(8, 2, 8, 2)
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private Button MakeGhostButton(string text)
        {
            var b = new Button
            {
                Text = text,
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(34, 47, 34),
                Margin = new Padding(8, 6, 0, 0),
                Height = 34,
                Padding = new Padding(8, 2, 8, 2)
            };
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = Color.FromArgb(210, 220, 210);
            return b;
        }

        private Panel MakeKpiCard(string titulo, string valor, string sub)
        {
            var p = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(246, 250, 246),
                Margin = new Padding(8),
                Padding = new Padding(16)
            };

            var lblTitle = new Label
            {
                Text = titulo,
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(90, 100, 90)
            };
            var lblValor = new Label
            {
                Text = valor,
                AutoSize = true,
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Color.FromArgb(34, 47, 34),
                Margin = new Padding(0, 8, 0, 0)
            };
            var lblSub = new Label
            {
                Text = sub,
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(90, 130, 90)
            };

            p.Controls.Add(lblSub);
            p.Controls.Add(lblValor);
            p.Controls.Add(lblTitle);

            // layout manual simple
            lblTitle.Location = new Point(0, 0);
            lblValor.Location = new Point(0, 24);
            lblSub.Location = new Point(0, 60);

            return p;
        }

        private ComboBox MakeCombo(string[] items)
        {
            var cb = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cb.Items.AddRange(items);
            cb.SelectedIndex = 0;
            return cb;
        }

        private DataGridViewTextBoxColumn MakeCol(string name, string header)
        {
            return new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                SortMode = DataGridViewColumnSortMode.Programmatic
            };
        }

        // ================ Datos de ejemplo ==================
        private void CargarDatosPrueba()
        {
            dgv.Rows.Add("SALE-001", "María González\nTech Solutions SA", "Venta", "Completada", "$1.250", "14/01/2024", "Juan Pérez", "...");
            dgv.Rows.Add("COT-021", "Carlos Rodríguez", "Cotización", "Pendiente", "$980", "09/01/2024", "Ana García", "...");
            dgv.Rows.Add("SALE-045", "Laura Pérez", "Venta", "Completada", "$2.350", "22/01/2024", "Marcos López", "...");
            dgv.Rows.Add("SALE-050", "Ricardo Núñez", "Venta", "Cancelada", "$0", "24/01/2024", "Juan Pérez", "...");

            // “chips” simples con color en Tipo/Estado
            PintarChipColumna("Tipo", "Venta", Color.FromArgb(210, 240, 210));
            PintarChipColumna("Tipo", "Cotización", Color.FromArgb(240, 240, 210));
            PintarChipColumna("Estado", "Completada", Color.FromArgb(200, 230, 200));
            PintarChipColumna("Estado", "Pendiente", Color.FromArgb(238, 238, 200));
            PintarChipColumna("Estado", "Cancelada", Color.FromArgb(240, 210, 210));
        }

        private void PintarChipColumna(string colName, string matchText, Color back)
        {
            foreach (DataGridViewRow r in dgv.Rows)
            {
                var cell = r.Cells[colName];
                if ((cell.Value?.ToString() ?? "").Equals(matchText, StringComparison.InvariantCultureIgnoreCase))
                {
                    cell.Style.BackColor = back;
                    cell.Style.SelectionBackColor = back;
                    cell.Style.ForeColor = Color.Black;
                }
            }
        }
    }
}
