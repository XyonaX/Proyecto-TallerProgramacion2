using Proyecto_Taller_2.Data.Repositories;
using Proyecto_Taller_2.Domain.Models.Dtos;
using System;
using System.Configuration;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proyecto_Taller_2.Controls
{
    public partial class UcReportes : UserControl
    {
        // --- CAMPOS PARA REFERENCIAS A LABELS (Data Binding) ---
        private Label _lblIngresosMensualesValor;
        private Label _lblIngresosVariacion;
        private Label _lblMetaMensualValor;
        private Label _lblMetaObjetivo;
        private Label _lblClientesActivosValor;
        private Label _lblClientesVariacion;
        private Label _lblStockCriticoValor;
        private Label _lblStockCriticoSubtitle;

        // KPIs Medios
        private Label _lblTicketPromedioValor;
        private Label _lblConversionValor;
        private Label _lblMargenBrutoValor;
        private Label _lblInventarioRotacion;
        private Label _lblInventarioValorTotal;
        private Label _lblInventarioProductosActivos;
        private Label _lblClientesRetencion;
        private Label _lblClientesValorPromedio;
        private Label _lblClientesNuevos;

        private FlowLayoutPanel _flowReportesRecientes;
        private readonly ReporteRepository _reporteRepository;
        private ComboBox _cmbPeriodoFiltro;

        public UcReportes()
        {
            InitializeComponent();
            string connectionString = ConfigurationManager.ConnectionStrings["ERP"].ConnectionString;
            _reporteRepository = new ReporteRepository(connectionString);
            InitializeLayout();
            this.Load += async (s, e) => await CargarDatosRangoAsync(
                new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                DateTime.Now
            );
        }

        private async Task CargarDatosRangoAsync(DateTime inicio, DateTime fin)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                var datos = await _reporteRepository.ObtenerDatosDashboardAsync(inicio, fin);

                // 1. Actualizar KPIs Superiores
                if (_lblIngresosMensualesValor != null) _lblIngresosMensualesValor.Text = $"${datos.IngresosMensuales:N0}";
                if (_lblIngresosVariacion != null) _lblIngresosVariacion.Text = $"{datos.PorcentajeVariacionIngreso:+0.0;-0.0}% vs. mes anterior";
                if (_lblMetaMensualValor != null) _lblMetaMensualValor.Text = $"{datos.PorcentajeMeta:N1}%";
                if (_lblMetaObjetivo != null) _lblMetaObjetivo.Text = $"${datos.MetaMensual:N0} objetivo";
                if (_lblClientesActivosValor != null) _lblClientesActivosValor.Text = datos.ClientesActivos.ToString("N0");
                if (_lblStockCriticoValor != null) _lblStockCriticoValor.Text = datos.StockCritico.ToString();

                // 2. Actualizar Bloques Medios
                if (_lblMargenBrutoValor != null) _lblMargenBrutoValor.Text = $"{datos.MargenBruto:N1}%";
                if (_lblInventarioValorTotal != null) _lblInventarioValorTotal.Text = $"${datos.ValorTotalInventario:N0}";
                if (_lblInventarioProductosActivos != null) _lblInventarioProductosActivos.Text = datos.CantidadProductosActivos.ToString("N0");
                if (_lblClientesNuevos != null) _lblClientesNuevos.Text = datos.NuevosClientesEsteMes.ToString();

                // --- CORRECCIÓN DE PROMEDIOS ---
                // TICKET PROMEDIO (Total Ingresos / Cantidad Ventas)
                if (datos.CantidadVentasPeriodo > 0 && _lblTicketPromedioValor != null)
                {
                    _lblTicketPromedioValor.Text = $"${(datos.IngresosMensuales / datos.CantidadVentasPeriodo):N2}";
                }
                else if (_lblTicketPromedioValor != null)
                {
                    _lblTicketPromedioValor.Text = "$0.00";
                }

                // VALOR PROMEDIO CLIENTE (Total Ingresos / Total Clientes Activos)
                if (datos.ClientesActivos > 0 && _lblClientesValorPromedio != null)
                {
                    decimal promedioCliente = datos.IngresosMensuales / (decimal)datos.ClientesActivos;
                    _lblClientesValorPromedio.Text = $"${promedioCliente:N2}";
                }
                else if (_lblClientesValorPromedio != null)
                {
                    _lblClientesValorPromedio.Text = "$0.00";
                }
                // -------------------------------

                // 3. Actualizar Lista de Reportes Recientes
                ActualizarListaReportes(datos);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando dashboard: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void ActualizarListaReportes(DashboardReporteDto datos)
        {
            if (_flowReportesRecientes == null) return;
            _flowReportesRecientes.Controls.Clear();

            foreach (var rep in datos.ReportesRecientes)
            {
                _flowReportesRecientes.Controls.Add(
                    CreateReporteRecienteItem(rep.FechaGeneracion, rep.Fecha, "Venta", rep.Estado)
                );
            }
        }

        private void InitializeLayout()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 5,
                ColumnCount = 1,
                Padding = new Padding(20),
                AutoScroll = true
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Header
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // KPIs
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Bloques Medios
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Filtros
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 500F)); // Paneles inferiores - ALTURA FIJA

            this.Controls.Add(mainLayout);

            // HEADER
            mainLayout.Controls.Add(CreateHeader(), 0, 0);

            // KPIs SUPERIORES
            var kpiLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = true,
                Margin = new Padding(0, 20, 0, 20)
            };

            kpiLayout.Controls.Add(CreateCard("Ingresos Mensuales", "$0", "...", Color.DarkGreen, out _lblIngresosMensualesValor, out _lblIngresosVariacion));
            kpiLayout.Controls.Add(CreateCard("Meta Mensual", "0%", "$0 objetivo", Color.DarkOliveGreen, out _lblMetaMensualValor, out _lblMetaObjetivo));
            kpiLayout.Controls.Add(CreateCard("Clientes Activos", "0", "...", Color.DarkGreen, out _lblClientesActivosValor, out _lblClientesVariacion));
            kpiLayout.Controls.Add(CreateCard("Stock Crítico", "0", "productos requieren reposición", Color.OliveDrab, out _lblStockCriticoValor, out _lblStockCriticoSubtitle));

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
                ("Ticket Promedio", "$0"),
                ("Conversión", "0%"),
                ("Margen Bruto", "0%")
            }, out var lblsVentas), 0, 0);
            _lblTicketPromedioValor = lblsVentas[0];
            _lblConversionValor = lblsVentas[1];
            _lblMargenBrutoValor = lblsVentas[2];

            middleLayout.Controls.Add(CreateMetricsBlock("Inventario", new[]
            {
                ("Rotación", "0x"),
                ("Valor Total", "$0"),
                ("Productos Activos", "0")
            }, out var lblsInv), 1, 0);
            _lblInventarioRotacion = lblsInv[0];
            _lblInventarioValorTotal = lblsInv[1];
            _lblInventarioProductosActivos = lblsInv[2];

            middleLayout.Controls.Add(CreateMetricsBlock("Clientes", new[]
            {
                ("Retención", "0%"),
                ("Valor Promedio", "$0"),
                ("Nuevos este mes", "0")
            }, out var lblsCli), 2, 0);
            _lblClientesRetencion = lblsCli[0];
            _lblClientesValorPromedio = lblsCli[1];
            _lblClientesNuevos = lblsCli[2];

            mainLayout.Controls.Add(middleLayout, 0, 2);

            // FILTROS
            mainLayout.Controls.Add(CreateFiltersPanel(), 0, 3);

            // PANELES INFERIORES (Rápidos y Recientes)
            var pnlBottomGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 20, 0, 0),
                AutoSize = false
            };
            pnlBottomGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            pnlBottomGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            pnlBottomGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            pnlBottomGrid.Controls.Add(CreateReportesRapidosPanel(), 0, 0);
            pnlBottomGrid.Controls.Add(CreateReportesRecientesPanel(), 1, 0);

            mainLayout.Controls.Add(pnlBottomGrid, 0, 4);
        }

        // =================================================
        // MÉTODOS AUXILIARES DE UI
        // =================================================

        private Panel CreateHeader()
        {
            var panel = new Panel { Dock = DockStyle.Top, Height = 60 };

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
            panel.Resize += (s, e) => { btnExport.Left = panel.Width - 140; };

            panel.Controls.Add(lblSubtitle);
            panel.Controls.Add(lblTitle);
            panel.Controls.Add(btnExport);

            return panel;
        }

        private Panel CreateCard(string title, string value, string subtitle, Color valueColor, out Label lblValueRef, out Label lblSubtitleRef)
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

            lblValueRef = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = valueColor,
                Dock = DockStyle.Top,
                AutoSize = true
            };

            lblSubtitleRef = new Label
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Dock = DockStyle.Top,
                AutoSize = true
            };

            panel.Controls.Add(lblSubtitleRef);
            panel.Controls.Add(lblValueRef);
            panel.Controls.Add(lblTitle);

            return panel;
        }

        private Panel CreateMetricsBlock(string title, (string, string)[] metrics, out Label[] valueLabels)
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

            panel.Controls.Add(new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Dock = DockStyle.Top,
                AutoSize = true
            });

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = metrics.Length,
                AutoSize = true
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            valueLabels = new Label[metrics.Length];

            for (int i = 0; i < metrics.Length; i++)
            {
                layout.Controls.Add(new Label
                {
                    Text = metrics[i].Item1,
                    Font = new Font("Segoe UI", 9),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft
                }, 0, i);

                var lblVal = new Label
                {
                    Text = metrics[i].Item2,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleRight
                };
                valueLabels[i] = lblVal;
                layout.Controls.Add(lblVal, 1, i);
            }

            panel.Controls.Add(layout);
            return panel;
        }

        private Panel CreateFiltersPanel()
        {
            var panel = new Panel
            {
                Height = 70,
                Dock = DockStyle.Top,
                BackColor = Color.WhiteSmoke,
                Padding = new Padding(15)
            };

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = false
            };

            var lbl = new Label { Text = "Período:", AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), Margin = new Padding(0, 8, 10, 0) };

            _cmbPeriodoFiltro = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 150,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(0, 5, 15, 0)
            };
            _cmbPeriodoFiltro.Items.AddRange(new object[] { "Este Mes", "Mes Pasado", "Este Año", "Todo" });
            _cmbPeriodoFiltro.SelectedIndex = 0;

            var btnAplicar = new Button
            {
                Text = "Aplicar Filtro",
                BackColor = Color.DarkGreen,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Height = 35,
                Width = 120,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 0, 0)
            };
            btnAplicar.FlatAppearance.BorderSize = 0;
            btnAplicar.Click += async (s, e) => await AplicarFiltrosAsync();

            flow.Controls.Add(lbl);
            flow.Controls.Add(_cmbPeriodoFiltro);
            flow.Controls.Add(btnAplicar);

            panel.Controls.Add(flow);
            return panel;
        }

        private async Task AplicarFiltrosAsync()
        {
            DateTime inicio = DateTime.Now;
            DateTime fin = DateTime.Now;

            switch (_cmbPeriodoFiltro.SelectedIndex)
            {
                case 0: // Este Mes
                    inicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    break;
                case 1: // Mes Pasado
                    inicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1);
                    fin = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1);
                    break;
                case 2: // Este Año
                    inicio = new DateTime(DateTime.Now.Year, 1, 1);
                    break;
                case 3: // Todo
                    inicio = DateTime.Now.AddYears(-5);
                    break;
            }

            await CargarDatosRangoAsync(inicio, fin);
        }

        private Panel CreateReportesRapidosPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(250, 252, 250),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(15),
                AutoSize = false
            };

            var lblTitulo = new Label
            {
                Text = "Accesos Directos",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 50,
                AutoSize = false,
                Padding = new Padding(0, 0, 0, 15)
            };
            panel.Controls.Add(lblTitulo);

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = false,
                WrapContents = false,
                Padding = new Padding(0, 80, 0, 0),
                AutoSize = false
            };

            flow.SizeChanged += (s, e) =>
            {
                foreach (Control c in flow.Controls)
                    c.Width = flow.ClientSize.Width - 5;
            };

            flow.Controls.Add(CreateReporteRapidoItem("Ventas del Día", "Resumen de hoy", "VENTAS_HOY"));
            flow.Controls.Add(CreateReporteRapidoItem("Top Productos", "Más vendidos del mes", "TOP_PRODUCTOS"));
            flow.Controls.Add(CreateReporteRapidoItem("Stock Bajo", "Productos a reponer urgentes", "STOCK_BAJO"));

            panel.Controls.Add(flow);
            return panel;
        }

        private Panel CreateReporteRapidoItem(string title, string subtitle, string actionTag)
        {
            var p = new Panel
            {
                Height = 90,
                Margin = new Padding(0, 0, 0, 20),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            var lblT = new Label { Text = title, Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(15, 15), AutoSize = true };
            var lblS = new Label { Text = subtitle, Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, Location = new Point(15, 42), AutoSize = true };

            var btn = new Button
            {
                Text = "Ver",
                Tag = actionTag,
                Size = new Size(70, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(p.Width - 85, 30),
                BackColor = Color.FromArgb(240, 240, 240),
                FlatStyle = FlatStyle.Flat
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += BtnReporteRapido_Click;

            p.SizeChanged += (s, e) => { btn.Left = p.Width - 85; };

            p.Controls.AddRange(new Control[] { lblT, lblS, btn });
            return p;
        }

        private async void BtnReporteRapido_Click(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is string action)
            {
                try
                {
                    this.Cursor = Cursors.WaitCursor;
                    string mensaje = "Cargando...";
                    string titulo = "Reporte Rápido";
                    MessageBoxIcon icono = MessageBoxIcon.Information;

                    switch (action)
                    {
                        case "STOCK_BAJO":
                            titulo = "Productos con Stock Bajo";
                            mensaje = await _reporteRepository.ObtenerStockBajoAsync();
                            icono = MessageBoxIcon.Warning;
                            break;

                        case "TOP_PRODUCTOS":
                            titulo = "Top Productos del Mes";
                            mensaje = await _reporteRepository.ObtenerTopProductosAsync();
                            break;

                        case "VENTAS_HOY":
                            titulo = "Resumen de Ventas de Hoy";
                            mensaje = await _reporteRepository.ObtenerVentasHoyAsync();
                            break;

                        default:
                            mensaje = "Acción no reconocida.";
                            break;
                    }

                    this.Cursor = Cursors.Default;
                    MessageBox.Show(mensaje, titulo, MessageBoxButtons.OK, icono);
                }
                catch (Exception ex)
                {
                    this.Cursor = Cursors.Default;
                    MessageBox.Show("Error al cargar el reporte: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private Panel CreateReportesRecientesPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(15),
                AutoSize = false
            };

            var lblTitulo = new Label
            {
                Text = "Últimas Actividades",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 50,
                AutoSize = false,
                Padding = new Padding(0, 0, 0, 15)
            };
            panel.Controls.Add(lblTitulo);

            _flowReportesRecientes = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                WrapContents = false,
                Padding = new Padding(0, 80, 0, 0),
                AutoSize = false
            };

            panel.Controls.Add(_flowReportesRecientes);
            return panel;
        }

        private Panel CreateReporteRecienteItem(string title, string date, string category, string status)
        {
            var item = new Panel { Width = 380, Height = 65, Margin = new Padding(0, 0, 0, 10), BackColor = Color.FromArgb(250, 250, 250), Padding = new Padding(5) };

            var lblTitle = new Label { Text = title, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(10, 12), AutoSize = true };
            var lblDate = new Label { Text = date, Font = new Font("Segoe UI", 8, FontStyle.Regular), ForeColor = Color.Gray, Location = new Point(10, 35), AutoSize = true };
            var lblCat = new Label { Text = category.ToUpper(), Font = new Font("Segoe UI", 7, FontStyle.Bold), ForeColor = Color.DarkSlateGray, Location = new Point(200, 22), AutoSize = true };

            var lblStatus = new Label
            {
                Text = status,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = status == "Completada" ? Color.Green : Color.Orange,
                Location = new Point(280, 22),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight
            };

            item.Controls.AddRange(new Control[] { lblTitle, lblDate, lblCat, lblStatus });
            return item;
        }
    }
}