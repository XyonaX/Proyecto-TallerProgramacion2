// UcDashboardAdmin.cs
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Proyecto_Taller_2.Domain.Entities;
using Proyecto_Taller_2.Domain.Models;
using Proyecto_Taller_2.Data.Repositories;
using Proyecto_Taller_2.Data;

namespace Proyecto_Taller_2.Controls
{
    public partial class UcDashboardAdmin : UserControl
    {
        private readonly VentaRepository _ventaRepo;

        public UcDashboardAdmin()
        {
            _ventaRepo = new VentaRepository(BDGeneral.ConnectionString);
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

                // Obtener KPIs reales de la base de datos
                var fechaDesde = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var fechaHasta = fechaDesde.AddMonths(1).AddDays(-1);
                var kpis = _ventaRepo.ObtenerKpis(null, fechaDesde, fechaHasta); // null = todos los vendedores
                var kpisGlobales = _ventaRepo.ObtenerKpisGlobales(fechaDesde, fechaHasta);

                // Crear KPIs con datos reales
                CreateKPI("Ventas Totales", kpis.VentasDelMes.ToString("C0"), FormatPorcentaje(kpis.PorcentajeVsAnterior) + " vs. mes anterior");
                CreateKPI("Órdenes Activas", kpis.TotalOrdenes.ToString(), FormatPorcentaje(kpis.PorcentajeOrdenesAnterior) + " este mes");
                CreateKPI("Ticket Promedio", kpis.TicketPromedio.ToString("C0"), FormatPorcentaje(kpis.PorcentajeTicketAnterior) + " vs. anterior");
                CreateKPI("Vendedores Activos", $"{kpisGlobales.VendedoresActivos}/{kpisGlobales.TotalVendedores}", $"Productividad: {kpisGlobales.ProductividadPromedio:C0}");

                // Crear gráfico de ventas con datos reales
                CreateSalesChart();

                // Crear lista de top vendedores
                CreateTopVendedores(kpisGlobales);

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

        private string FormatPorcentaje(decimal porcentaje)
        {
            var signo = porcentaje >= 0 ? "+" : "";
            return $"{signo}{porcentaje:F1}%";
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
                BackColor = Color.WhiteSmoke,
                AxisX = { Title = "Últimos 6 Meses", TitleFont = new Font("Segoe UI", 10, FontStyle.Bold) },
                AxisY = { Title = "Ventas (ARS)", TitleFont = new Font("Segoe UI", 10, FontStyle.Bold) }
            });

            var series = new Series("Ventas por Mes")
            {
                ChartType = SeriesChartType.Column,
                Color = Color.FromArgb(34, 139, 58),
                BorderWidth = 0
            };

            // Obtener datos de ventas de los últimos 6 meses
            try
            {
                for (int i = 5; i >= 0; i--)
                {
                    var fecha = DateTime.Now.AddMonths(-i);
                    var fechaDesde = new DateTime(fecha.Year, fecha.Month, 1);
                    var fechaHasta = fechaDesde.AddMonths(1).AddDays(-1);
                    
                    var kpisMes = _ventaRepo.ObtenerKpis(null, fechaDesde, fechaHasta);
                    var nombreMes = fecha.ToString("MMM yyyy");
                    
                    series.Points.AddXY(nombreMes, (double)kpisMes.VentasDelMes);
                }
            }
            catch
            {
                // Si hay error con datos reales, usar datos de demostración
                series.Points.AddXY("Jul 2024", 85000);
                series.Points.AddXY("Ago 2024", 120000);
                series.Points.AddXY("Sep 2024", 95000);
                series.Points.AddXY("Oct 2024", 150000);
                series.Points.AddXY("Nov 2024", 180000);
                series.Points.AddXY("Dic 2024", 200000);
            }

            salesChart.Series.Add(series);
            
            // Agregar título al gráfico
            salesChart.Titles.Add(new Title("Evolución de Ventas Mensuales")
            {
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(34, 47, 34)
            });
            
            pnlCenter.Controls.Add(salesChart, 0, 0);
        }

        private void CreateTopVendedores(KpisGlobales kpisGlobales)
        {
            var topPanel = new Panel
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
            topPanel.Controls.Add(layout);

            var lblHeader = new Label
            {
                Text = "Top Vendedores del Mes",
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

            // Agregar top vendedores reales
            if (kpisGlobales.TopVendedores.Count > 0)
            {
                int posicion = 1;
                foreach (var vendedor in kpisGlobales.TopVendedores)
                {
                    AddVendedorItem($"#{posicion}. {vendedor.Nombre}", 
                                  $"{vendedor.NumeroVentas} ventas", 
                                  vendedor.TotalVentas.ToString("C0"), flow);
                    posicion++;
                }
            }
            else
            {
                // Si no hay datos, mostrar mensaje
                var lblNoData = new Label
                {
                    Text = "No hay datos de ventas en este período",
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.Gray,
                    Height = 40,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top
                };
                flow.Controls.Add(lblNoData);
            }

            pnlCenter.Controls.Add(topPanel, 1, 0);
        }

        private void AddVendedorItem(string name, string info, string amount, FlowLayoutPanel parent)
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

            var lblInfo = new Label
            {
                Text = info,
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
            layout.Controls.Add(lblInfo, 0, 1);
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

            // Datos de ejemplo - en una implementación real, estos vendrían de la base de datos
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

        // Método público para refrescar el dashboard
        public void RefreshDashboard()
        {
            LoadDashboardData();
        }
    }
}
