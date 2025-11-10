using Proyecto_Taller_2.Data.Repositories;
using Proyecto_Taller_2.Domain.Models.Dtos;
using System;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Proyecto_Taller_2.Controls
{
    public partial class UcDashboardAdmin : UserControl
    {
        // Usamos el nuevo repositorio especializado
        private readonly DashboardRepository _dashboardRepo;

        public UcDashboardAdmin()
        {
            InitializeComponent();
            // Inicializamos el repositorio con la cadena de conexión "ERP"
            string connectionString = ConfigurationManager.ConnectionStrings["ERP"].ConnectionString;
            _dashboardRepo = new DashboardRepository(connectionString);

            InitializeLayout();

            // Cargar datos al iniciar
            this.Load += async (s, e) => await CargarDatosAsync();
        }

        private async Task CargarDatosAsync()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                // 1. Obtener todos los datos del repositorio en una sola llamada
                var data = await _dashboardRepo.ObtenerDatosHomeAsync();

                // 2. Limpiar paneles antes de recargar
                pnlTop.Controls.Clear();
                pnlCenter.Controls.Clear();
                pnlBottom.Controls.Clear();

                // 3. Llenar KPIs Superiores
                CreateKPI("Ventas Totales", data.VentasTotales.ToString("C0"), FormatPorcentaje(data.PorcentajeVentasVsAnterior) + " vs. mes anterior");
                CreateKPI("Órdenes Activas", data.OrdenesActivas.ToString(), FormatPorcentaje(data.PorcentajeOrdenesVsAnterior) + " vs. mes anterior");
                CreateKPI("Ticket Promedio", data.TicketPromedio.ToString("C0"), FormatPorcentaje(data.PorcentajeTicketVsAnterior) + " vs. mes anterior");
                CreateKPI("Vendedores Activos", $"{data.VendedoresActivos}/{data.TotalVendedores}", $"Productividad prom: {data.ProductividadPromedio:C0}");

                // 4. Crear Gráfico Central
                CreateSalesChart(data);

                // 5. Crear Lista Top Vendedores
                CreateTopVendedores(data);

                // 6. Crear Paneles Inferiores (Stock por Categoría y Alertas)
                CreateBottomPanels(data);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el dashboard: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private string FormatPorcentaje(decimal porcentaje)
        {
            return (porcentaje >= 0 ? "+" : "") + porcentaje.ToString("F1") + "%";
        }

        // =======================================================
        // MÉTODOS DE CREACIÓN DE UI (Actualizados para usar DTO)
        // =======================================================

        private void InitializeLayout()
        {
            pnlTop.Height = 150;
            pnlTop.WrapContents = false;
            pnlTop.FlowDirection = FlowDirection.LeftToRight;
            pnlTop.Padding = new Padding(10);
            pnlTop.AutoScroll = true; // Permitir scroll si hay muchos KPIs

            pnlCenter.Height = 350;
            pnlCenter.ColumnStyles.Clear();
            pnlCenter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F)); // Gráfico más ancho
            pnlCenter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F)); // Top Vendedores

            pnlBottom.RowStyles.Clear();
            pnlBottom.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            pnlBottom.ColumnStyles.Clear();
            pnlBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F)); // Categorías
            pnlBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F)); // Alertas
        }

        private void CreateKPI(string title, string value, string percentage)
        {
            var panel = new Panel
            {
                Size = new Size(220, 110),
                BackColor = Color.FromArgb(240, 255, 240),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10)
            };

            panel.Controls.Add(new Label { Text = percentage, Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, Dock = DockStyle.Bottom });
            panel.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(34, 139, 58), Dock = DockStyle.Top, Height = 30 });
            panel.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 11, FontStyle.Bold), Dock = DockStyle.Top });

            pnlTop.Controls.Add(panel);
        }

        private void CreateSalesChart(DashboardHomeDto data)
        {
            var chart = new Chart { Dock = DockStyle.Fill };
            var area = new ChartArea { BackColor = Color.WhiteSmoke };
            area.AxisX.MajorGrid.LineColor = Color.LightGray;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;
            area.AxisX.LabelStyle.Font = new Font("Segoe UI", 8);
            area.AxisY.LabelStyle.Format = "C0"; // Formato moneda sin decimales
            chart.ChartAreas.Add(area);

            var series = new Series
            {
                Name = "Ventas",
                ChartType = SeriesChartType.Column,
                Color = Color.FromArgb(34, 139, 58),
                IsValueShownAsLabel = true,
                Font = new Font("Segoe UI", 8)
            };

            // Llenar con datos reales (invertimos la lista para que vaya de más antiguo a más nuevo)
            foreach (var item in ((System.Collections.Generic.IEnumerable<VentaMensualDto>)data.EvolucionVentas).Reverse())
            {
                series.Points.AddXY(item.Mes, item.TotalVenta);
            }

            chart.Series.Add(series);
            chart.Titles.Add(new Title("Evolución de Ventas (Últimos 6 meses)", Docking.Top, new Font("Segoe UI", 12, FontStyle.Bold), Color.Black));

            pnlCenter.Controls.Add(chart, 0, 0);
        }

        private void CreateTopVendedores(DashboardHomeDto data)
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(10), Margin = new Padding(5) };
            panel.Controls.Add(new Label { Text = "Top Vendedores del Mes", Font = new Font("Segoe UI", 11, FontStyle.Bold), Dock = DockStyle.Top, Height = 30 });

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                WrapContents = false,
                Padding = new Padding(0, 80, 0, 0) // AQUÍ: 80 píxeles desde arriba
            };

            if (data.TopVendedores.Count > 0)
            {
                int rank = 1;
                foreach (var vendedor in data.TopVendedores)
                {
                    AddVendedorItem(rank++, vendedor, flow);
                }
            }
            else
            {
                flow.Controls.Add(new Label { Text = "No hay ventas registradas este mes.", AutoSize = true, ForeColor = Color.Gray, Padding = new Padding(5) });
            }

            panel.Controls.Add(flow);
            pnlCenter.Controls.Add(panel, 1, 0);
        }

        private void AddVendedorItem(int rank, TopVendedorDto vendedor, FlowLayoutPanel parent)
        {
            var item = new Panel { Width = 250, Height = 60, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 0, 0, 10), Padding = new Padding(5) };
            item.Controls.Add(new Label { Text = vendedor.TotalFacturado.ToString("C0"), Dock = DockStyle.Right, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.DarkGreen, AutoSize = true });
            item.Controls.Add(new Label { Text = $"{vendedor.CantidadVentas} ventas", Dock = DockStyle.Bottom, Font = new Font("Segoe UI", 8), ForeColor = Color.Gray });
            item.Controls.Add(new Label { Text = $"#{rank} {vendedor.Nombre}", Dock = DockStyle.Top, Font = new Font("Segoe UI", 9, FontStyle.Bold) });
            parent.Controls.Add(item);
        }

        private void CreateBottomPanels(DashboardHomeDto data)
        {
            // 1. Inventario por Categoría (Izquierda)
            var stockFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoScroll = true, WrapContents = false, Padding = new Padding(5) };
            foreach (var cat in data.InventarioPorCategoria)
            {
                CreateStockBar(cat.NombreCategoria, cat.StockActual, cat.StockEsperado, stockFlow);
            }
            pnlBottom.Controls.Add(stockFlow, 0, 0);

            // 2. Alertas (Derecha)
            var alertsFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoScroll = true, WrapContents = false, Padding = new Padding(5) };

            // Alerta real de Stock Bajo
            if (data.CantidadStockBajo > 0)
            {
                CreateAlert($"⚠️ Stock Bajo: {data.CantidadStockBajo} productos críticos", Color.FromArgb(255, 235, 235), Color.DarkRed, alertsFlow);
            }
            else
            {
                CreateAlert("✅ Inventario saludable", Color.FromArgb(235, 255, 235), Color.DarkGreen, alertsFlow);
            }

            // Alerta simulada de Backup (puedes conectarla a datos reales luego si tienes un log de backups)
            CreateAlert("💾 Último Backup: Hoy 09:00 AM", Color.FromArgb(240, 240, 255), Color.DarkBlue, alertsFlow);

            pnlBottom.Controls.Add(alertsFlow, 1, 0);
        }

        private void CreateStockBar(string category, int current, int max, Control parent)
        {
            var p = new Panel { Width = 300, Height = 50, Margin = new Padding(0, 0, 0, 10), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White, Padding = new Padding(5) };
            int percentage = max > 0 ? Math.Min((int)((double)current / max * 100), 100) : 0;

            var pb = new ProgressBar { Value = percentage, Dock = DockStyle.Bottom, Height = 10 };
            // Truco para cambiar el color de la barra si es estándar (no siempre funciona con estilos visuales activados)
            if (percentage < 20) pb.ForeColor = Color.Red; // Conceptualmente, aunque WinForms básico no lo muestra fácil

            p.Controls.Add(new Label { Text = $"{current}/{max}", Dock = DockStyle.Right, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 8) });
            p.Controls.Add(pb);
            p.Controls.Add(new Label { Text = category, Dock = DockStyle.Left, Font = new Font("Segoe UI", 9, FontStyle.Bold) });

            parent.Controls.Add(p);
        }

        private void CreateAlert(string text, Color backColor, Color foreColor, Control parent)
        {
            var p = new Panel { Width = 250, Height = 40, BackColor = backColor, Margin = new Padding(0, 0, 0, 10), BorderStyle = BorderStyle.FixedSingle };
            p.Controls.Add(new Label { Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = foreColor, Padding = new Padding(5, 0, 0, 0) });
            parent.Controls.Add(p);
        }
    }
}