using System;
using System.Drawing;
using System.Windows.Forms;

namespace Proyecto_Taller_2.Controls
{
    public partial class UcReportes : UserControl
    {
        public UcReportes()
        {
            InitializeComponent();
            InitializeLayout();
        }

        private void InitializeLayout()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1
            };

            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
                Padding = new Padding(20),
                AutoScroll = true
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Header
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // KPIs
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Bloques medios
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Filtros
            this.Controls.Add(mainLayout);

            // HEADER
            mainLayout.Controls.Add(CreateHeader(), 0, 0);

            // KPIs
            var kpiLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = true,
                Margin = new Padding(0, 20, 0, 20)
            };
            kpiLayout.Controls.Add(CreateCard("Ingresos Mensuales", "$847,392", "+15.3% vs. mes anterior", Color.DarkGreen));
            kpiLayout.Controls.Add(CreateCard("Meta Mensual", "78%", "$1,085,000 objetivo", Color.DarkOliveGreen));
            kpiLayout.Controls.Add(CreateCard("Clientes Activos", "892", "+8.2% vs. mes anterior", Color.DarkGreen));
            kpiLayout.Controls.Add(CreateCard("Stock Crítico", "15", "productos requieren reposición", Color.OliveDrab));
            mainLayout.Controls.Add(kpiLayout, 0, 1);

            // BLOQUES MEDIOS
            var middleLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 3,
                AutoSize = true
            };
            middleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            middleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            middleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));

            middleLayout.Controls.Add(CreateMetricsBlock("Rendimiento de Ventas", new[]
            {
                ("Ticket Promedio", "$5,432"),
                ("Conversión", "24.8%"),
                ("Margen Bruto", "34.2%")
            }), 0, 0);

            middleLayout.Controls.Add(CreateMetricsBlock("Inventario", new[]
            {
                ("Rotación", "6.2x"),
                ("Valor Total", "$2.4M"),
                ("Productos Activos", "3,456")
            }), 1, 0);

            middleLayout.Controls.Add(CreateMetricsBlock("Clientes", new[]
            {
                ("Retención", "87.3%"),
                ("Valor Promedio", "$2,847"),
                ("Nuevos este mes", "47")
            }), 2, 0);

            mainLayout.Controls.Add(middleLayout, 0, 2);

            // FILTROS DE PERIODO
            mainLayout.Controls.Add(CreateFiltersPanel(), 0, 3);
            mainLayout.Controls.Add(layout, 0, mainLayout.RowCount++);

            var pnlMiddle = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
            };
            pnlMiddle.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            pnlMiddle.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            pnlMiddle.Controls.Add(CreateReportesRapidosPanel(), 0, 0);
            pnlMiddle.Controls.Add(CreateReportesRecientesPanel(), 1, 0);

            layout.Controls.Add(pnlMiddle, 0, 1);

            // 3️⃣ Parte Inferior → Cards accesos ocultos
            var pnlBottom = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true
            };

            pnlBottom.Controls.Add(CreateAccesoCard("Reportes de Ventas", "Análisis detallado de ventas, tendencias y rendimiento por vendedor"));
            pnlBottom.Controls.Add(CreateAccesoCard("Reportes de Inventario", "Control de stock, rotación de productos y análisis de inventario"));
            pnlBottom.Controls.Add(CreateAccesoCard("Reportes de Clientes", "Segmentación de clientes, análisis de comportamiento y retención"));

            layout.Controls.Add(pnlBottom, 0, 2);   
        }

        // ====== HEADER ======
        private Panel CreateHeader()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60
            };

            var lblTitle = new Label
            {
                Text = "Reportes y Análisis",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30
            };

            var lblSubtitle = new Label
            {
                Text = "Análisis detallado del rendimiento empresarial",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Dock = DockStyle.Top,
                Height = 25
            };

            var btnExport = new Button
            {
                Text = "Exportar Todo",
                Width = 120,
                Height = 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(this.Width - 150, 10)
            };

            var btnSchedule = new Button
            {
                Text = "Programar Reporte",
                Width = 140,
                Height = 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(this.Width - 300, 10)
            };

            panel.Controls.Add(lblSubtitle);
            panel.Controls.Add(lblTitle);
            panel.Controls.Add(btnExport);
            panel.Controls.Add(btnSchedule);

            return panel;
        }

        // ====== KPI CARD ======
        private Panel CreateCard(string title, string value, string subtitle, Color valueColor)
        {
            var panel = new Panel
            {
                Width = 250,
                Height = 120,
                BackColor = Color.FromArgb(245, 250, 245),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10),
                Padding = new Padding(10)
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock = DockStyle.Top,
                AutoSize = true
            };

            var lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = valueColor,
                Dock = DockStyle.Top,
                AutoSize = true
            };

            var lblSubtitle = new Label
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Dock = DockStyle.Top,
                AutoSize = true
            };

            panel.Controls.Add(lblSubtitle);
            panel.Controls.Add(lblValue);
            panel.Controls.Add(lblTitle);

            return panel;
        }

        // ====== MÉTRICAS ======
        private Panel CreateMetricsBlock(string title, (string, string)[] metrics)
        {
            var panel = new Panel
            {
                Height = 150,
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 252, 248),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10),
                Padding = new Padding(10)
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Dock = DockStyle.Top,
                AutoSize = true
            };
            panel.Controls.Add(lblTitle);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = metrics.Length,
                AutoSize = true
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

            for (int i = 0; i < metrics.Length; i++)
            {
                var lblMetric = new Label
                {
                    Text = metrics[i].Item1,
                    Font = new Font("Segoe UI", 9),
                    Dock = DockStyle.Fill
                };
                var lblValue = new Label
                {
                    Text = metrics[i].Item2,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleRight
                };
                layout.Controls.Add(lblMetric, 0, i);
                layout.Controls.Add(lblValue, 1, i);
            }

            panel.Controls.Add(layout);

            return panel;
        }

        // ====== FILTROS ======
        private Panel CreateFiltersPanel()
        {
            var panel = new Panel
            {
                Height = 100,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(248, 252, 248),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10),
                Padding = new Padding(10)
            };

            var lblTitle = new Label
            {
                Text = "Filtros de Período",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Dock = DockStyle.Top,
                AutoSize = true
            };
            panel.Controls.Add(lblTitle);

            var lblDesc = new Label
            {
                Text = "Seleccione el rango de fechas para los reportes",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Dock = DockStyle.Top,
                AutoSize = true
            };
            panel.Controls.Add(lblDesc);

            var cmbPeriodo = new ComboBox
            {
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbPeriodo.Items.AddRange(new[] { "Este mes", "Último mes", "Últimos 3 meses" });
            cmbPeriodo.SelectedIndex = 0;

            var cmbComparar = new ComboBox
            {
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbComparar.Items.AddRange(new[] { "Periodo anterior", "Mismo periodo año pasado" });
            cmbComparar.SelectedIndex = 0;

            var btnAplicar = new Button
            {
                Text = "Aplicar Filtros",
                Width = 120,
                Height = 30,
                BackColor = Color.DarkGreen,
                ForeColor = Color.White
            };

            var filtersLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 0)
            };

            filtersLayout.Controls.Add(new Label { Text = "Periodo:", AutoSize = true, Padding = new Padding(0, 5, 5, 0) });
            filtersLayout.Controls.Add(cmbPeriodo);
            filtersLayout.Controls.Add(new Label { Text = "Comparar con:", AutoSize = true, Padding = new Padding(10, 5, 5, 0) });
            filtersLayout.Controls.Add(cmbComparar);
            filtersLayout.Controls.Add(btnAplicar);

            panel.Controls.Add(filtersLayout);

            return panel;
        }
        private Panel CreateReportesRapidosPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 250, 245),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10)
            };

            var lblTitle = new Label
            {
                Text = "Reportes Rápidos",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 25
            };
            panel.Controls.Add(lblTitle);

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown
            };

            flow.Controls.Add(CreateReporteRapidoItem("Ventas Diarias", "Resumen de ventas del día"));
            flow.Controls.Add(CreateReporteRapidoItem("Top Productos", "Productos más vendidos"));
            flow.Controls.Add(CreateReporteRapidoItem("Rendimiento Vendedores", "Performance del equipo"));
            flow.Controls.Add(CreateReporteRapidoItem("Tendencias Mensuales", "Análisis de tendencias"));

            panel.Controls.Add(flow);
            return panel;
        }

        private Panel CreateReporteRapidoItem(string title, string subtitle)
        {
            var item = new Panel { Width = 350, Height = 50, Margin = new Padding(5) };

            var lblTitle = new Label { Text = title, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(5, 5), AutoSize = true };
            var lblSubtitle = new Label { Text = subtitle, Font = new Font("Segoe UI", 8), Location = new Point(5, 25), AutoSize = true };

            var btnGenerar = new Button { Text = "Generar", Width = 80, Height = 30, Location = new Point(250, 10) };

            item.Controls.Add(lblTitle);
            item.Controls.Add(lblSubtitle);
            item.Controls.Add(btnGenerar);

            return item;
        }

        private Panel CreateReportesRecientesPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 250, 245),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10)
            };

            var lblTitle = new Label
            {
                Text = "Reportes Recientes",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 25
            };
            panel.Controls.Add(lblTitle);

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown
            };

            flow.Controls.Add(CreateReporteRecienteItem("Reporte de Ventas - Enero 2024", "30/1/2024", "Ventas", "Completado"));
            flow.Controls.Add(CreateReporteRecienteItem("Análisis de Inventario - Q4 2023", "14/1/2024", "Inventario", "Completado"));
            flow.Controls.Add(CreateReporteRecienteItem("Segmentación de Clientes", "9/1/2024", "Clientes", "Procesando"));
            flow.Controls.Add(CreateReporteRecienteItem("KPIs Ejecutivos - Diciembre", "4/1/2024", "Ejecutivo", "Completado"));

            panel.Controls.Add(flow);
            return panel;
        }

        private Panel CreateReporteRecienteItem(string title, string date, string category, string status)
        {
            var item = new Panel { Width = 350, Height = 50, Margin = new Padding(5) };

            var lblTitle = new Label { Text = title, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(5, 5), AutoSize = true };
            var lblDate = new Label { Text = date, Font = new Font("Segoe UI", 8), Location = new Point(5, 25), AutoSize = true };

            var lblCategory = new Label { Text = category, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.DarkOliveGreen, Location = new Point(150, 5), AutoSize = true };
            var lblStatus = new Label { Text = status, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = (status == "Completado" ? Color.DarkGreen : Color.Orange), Location = new Point(150, 25), AutoSize = true };

            item.Controls.Add(lblTitle);
            item.Controls.Add(lblDate);
            item.Controls.Add(lblCategory);
            item.Controls.Add(lblStatus);

            return item;
        }

        private Panel CreateAccesoCard(string title, string description)
        {
            var card = new Panel
            {
                Width = 250,
                Height = 120,
                BackColor = Color.FromArgb(240, 250, 240),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10),
                Padding = new Padding(10),
                Cursor = Cursors.Hand
            };

            var lblTitle = new Label { Text = title, Font = new Font("Segoe UI", 10, FontStyle.Bold), Dock = DockStyle.Top };
            var lblDescription = new Label { Text = description, Font = new Font("Segoe UI", 8), Dock = DockStyle.Fill };

            card.Controls.Add(lblDescription);
            card.Controls.Add(lblTitle);

            return card;
        }
    }
}
