// UcDashboardAdmin.cs
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Proyecto_Taller_2.Domain.Entities;

namespace Proyecto_Taller_2.Controls
{
    public partial class UcDashboardAdmin : UserControl
    {
        public UcDashboardAdmin()
        {
            InitializeComponent();
            InitializeLayout();    // ajusta tamaños/diseño
            LoadDashboardData();   // carga datos y crea los controles
        }

        // Ajusta tamaños y estilos en los paneles definidos en el diseñador
        private void InitializeLayout()
        {
            // Panel para KPIs: altura más baja y márgenes
            pnlTop.Height = 150;
            pnlTop.WrapContents = false;
            pnlTop.FlowDirection = FlowDirection.LeftToRight;
            pnlTop.Padding = new Padding(10);
            pnlTop.AutoScroll = false;

            // Panel central: altura más baja - FIX: Inicializar correctamente las columnas
            pnlCenter.Height = 350;
            pnlCenter.ColumnCount = 2;
            
            // Clear existing column styles and add new ones
            pnlCenter.ColumnStyles.Clear();
            pnlCenter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            pnlCenter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            // Panel inferior: ocupa el resto - FIX: Inicializar correctamente las filas y columnas
            pnlBottom.RowCount = 1;
            pnlBottom.ColumnCount = 2;
            
            // Clear existing styles and add new ones
            pnlBottom.ColumnStyles.Clear();
            pnlBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            pnlBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            
            pnlBottom.RowStyles.Clear();
            pnlBottom.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        }

        private void LoadDashboardData()
        {
            try
            {
                // Limpia los paneles antes de añadir controles
                pnlTop.Controls.Clear();
                pnlCenter.Controls.Clear();
                pnlBottom.Controls.Clear();

                // Crear KPIs (puedes ajustar tamaños aquí)
                CreateKPI("Ventas Totales", "$2,847,392", "+12.5% vs. mes anterior");
                CreateKPI("Clientes Activos", "1,247", "+8.2% vs. mes anterior");
                CreateKPI("Productos en Stock", "3,456", "-2.1% vs. mes anterior");
                CreateKPI("Margen de Ganancia", "23.4%", "+1.8% vs. mes anterior");

                // Crear gráfico de ventas
                CreateSalesChart();

                // Crear lista de ventas recientes
                CreateRecentSales();

                // Crear paneles de stock
                CreateStockPanels();

                // Crear alertas del sistema
                CreateAlertsPanels();
            }
            catch (Exception ex)
            {
                // Log error and show a simple error message instead of crashing
                var errorLabel = new Label
                {
                    Text = $"Error cargando dashboard: {ex.Message}",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.Red
                };
                this.Controls.Clear();
                this.Controls.Add(errorLabel);
            }
        }

        private void CreateKPI(string title, string value, string percentage)
        {
            var panel = new Panel();
            panel.Size = new Size(250, 120);
            panel.BackColor = Color.FromArgb(240, 255, 240); // Light green background
            panel.BorderStyle = BorderStyle.FixedSingle;
            panel.Padding = new Padding(10);
            panel.Margin = new Padding(10);
            panel.ForeColor = Color.Black;

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Top
            };

            var lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(34, 139, 58), // Green for value
                Dock = DockStyle.Top
            };

            var lblPercentage = new Label
            {
                Text = percentage,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Dock = DockStyle.Bottom
            };

            panel.Controls.Add(lblPercentage);
            panel.Controls.Add(lblValue);
            panel.Controls.Add(lblTitle);
            pnlTop.Controls.Add(panel);
        }


        private void CreateSalesChart()
        {
            var salesChart = new Chart();
            salesChart.Dock = DockStyle.Fill;
            salesChart.ChartAreas.Add(new ChartArea
            {
                BackColor = Color.WhiteSmoke, // Light background for the chart
                AxisX = { Title = "Meses", TitleFont = new Font("Segoe UI", 10, FontStyle.Bold) }, // Title for X-axis
                AxisY = { Title = "Ventas", TitleFont = new Font("Segoe UI", 10, FontStyle.Bold) } // Title for Y-axis
            });

            var series = new Series("Ventas por Mes")
            {
                ChartType = SeriesChartType.Column,
                Color = Color.FromArgb(34, 139, 58), // Change to a green color
                BorderWidth = 0
            };

            series.Points.AddXY("Ene", 100000);
            series.Points.AddXY("Feb", 120000);
            series.Points.AddXY("Mar", 150000);

            salesChart.Series.Add(series);
            pnlCenter.Controls.Add(salesChart, 0, 0);
        }



        private void CreateRecentSales()
        {
            var recentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10),
                Margin = new Padding(5)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Header
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Lista
            recentPanel.Controls.Add(layout);

            var lblHeader = new Label
            {
                Text = "Ventas Recientes",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 25
            };
            layout.Controls.Add(lblHeader, 0, 0);

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                WrapContents = false,
                Padding = new Padding(0)
            };
            layout.Controls.Add(flow, 0, 1);

            AddSaleItem("María González", "maria.gonzalez@email.com", "$1,250.00", flow);
            AddSaleItem("Carlos Rodríguez", "carlos.rodriguez@email.com", "$850.00", flow);
            AddSaleItem("Ana Martínez", "ana.martinez@email.com", "$2,100.00", flow);
            AddSaleItem("Luis Fernández", "luis.fernandez@email.com", "$750.00", flow);
            AddSaleItem("Carmen López", "carmen.lopez@email.com", "$1,800.00", flow);

            pnlCenter.Controls.Add(recentPanel, 1, 0);
        }


        private void AddSaleItem(string name, string email, string amount, FlowLayoutPanel parent)
        {
            var item = new Panel
            {
                Height = 60,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5),
                Margin = new Padding(0, 0, 0, 8)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70)); // texto
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30)); // monto
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            var lblName = new Label
            {
                Text = name,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            var lblEmail = new Label
            {
                Text = email,
                Font = new Font("Segoe UI", 8),
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            var lblAmount = new Label
            {
                Text = amount,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(34, 139, 58),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight
            };

            layout.Controls.Add(lblName, 0, 0);
            layout.Controls.Add(lblEmail, 0, 1);
            layout.Controls.Add(lblAmount, 1, 0);
            layout.SetRowSpan(lblAmount, 2);

            item.Controls.Add(layout);
            parent.Controls.Add(item);

            //Ajusta ancho inicial al del padre
            item.Width = parent.ClientSize.Width - parent.Padding.Horizontal - item.Margin.Horizontal;

            //Cuando se agregan controles nuevos, también ajusta
            parent.ControlAdded += (s, e) =>
            {
                e.Control.Width = parent.ClientSize.Width - parent.Padding.Horizontal - e.Control.Margin.Horizontal;
            };

            //Cuando el padre cambia de tamaño, reajusta todos los ítems
            parent.SizeChanged += (s, e) =>
            {
                foreach (Control ctrl in parent.Controls)
                {
                    ctrl.Width = parent.ClientSize.Width - parent.Padding.Horizontal - ctrl.Margin.Horizontal;
                }
            };
        }




        private void CreateStockPanels()
        {
            var stockContainer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                WrapContents = false,
                Margin = new Padding(5)
            };


            CreateStockBar("Electrónicos", 1250, 1500, stockContainer);
            CreateStockBar("Oficina", 890, 1000, stockContainer);
            CreateStockBar("Hogar", 650, 800, stockContainer);
            CreateStockBar("Deportes", 420, 600, stockContainer);
            CreateStockBar("Ropa", 280, 400, stockContainer);

            pnlBottom.Controls.Add(stockContainer, 0, 0);
        }

        private void CreateStockBar(string category, int current, int total, Control parent)
        {
            var item = new Panel
            {
                Width = 300,
                Height = 55,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5),
                BackColor = Color.White
            };

            var lblCategory = new Label
            {
                Text = category,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 18
            };

            var progressBar = new ProgressBar
            {
                Value = (int)((double)current / total * 100),
                Dock = DockStyle.Top,
                Height = 12
            };

            var lblStatus = new Label
            {
                Text = $"{current}/{total} disponible",
                Font = new Font("Segoe UI", 8),
                Dock = DockStyle.Top,
                Height = 16
            };


            item.Controls.Add(lblStatus);
            item.Controls.Add(progressBar);
            item.Controls.Add(lblCategory);
            parent.Controls.Add(item);
        }

        private void CreateAlertsPanels()
        {
            var alertsContainer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                WrapContents = false,
                Margin = new Padding(5)
            };

            CreateAlert("Stock Bajo: 15 productos con stock crítico", Color.FromArgb(255, 225, 225), alertsContainer);
            CreateAlert("Backup Completado: Último backup hace 2 horas", Color.FromArgb(225, 255, 225), alertsContainer);

            pnlBottom.Controls.Add(alertsContainer, 1, 0);
        }

        private void CreateAlert(string alertText, Color alertColor, FlowLayoutPanel parent)
        {
            var alertPanel = new Panel();
            alertPanel.Size = new Size(400, 50);
            alertPanel.BackColor = alertColor; // Red for critical alerts, Green for success
            alertPanel.BorderStyle = BorderStyle.FixedSingle;
            alertPanel.Margin = new Padding(10);

            var lblAlert = new Label
            {
                Text = alertText,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Black,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            alertPanel.Controls.Add(lblAlert);
            parent.Controls.Add(alertPanel);
        }

    }
}
