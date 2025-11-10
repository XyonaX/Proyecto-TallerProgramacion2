using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Proyecto_Taller_2.Domain.Models;

namespace Proyecto_Taller_2.Forms
{
    public partial class AnalisisVentasForm : Form
    {
        private readonly List<Venta> _ventas;
        private readonly List<Usuario> _vendedores;

        // Controles
        private TabControl tabControl;
        private TabPage tabResumen, tabTendencias, tabComparaciones, tabKpis;

        public AnalisisVentasForm(List<Venta> ventas, List<Usuario> vendedores)
        {
            _ventas = ventas;
            _vendedores = vendedores;
            
            InitializeComponent();
            GenerarAnalisis();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form
            this.Text = "Análisis Detallado de Ventas";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(900, 600);

            // Tab Control
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };
            tabControl.Padding = new Point(16, 16); // Corregido: usar Point en lugar de Padding

            // Tabs
            tabResumen = new TabPage("Resumen Ejecutivo");
            tabTendencias = new TabPage("Tendencias");
            tabComparaciones = new TabPage("Comparaciones");
            tabKpis = new TabPage("KPIs Detallados");

            tabControl.TabPages.Add(tabResumen);
            tabControl.TabPages.Add(tabTendencias);
            tabControl.TabPages.Add(tabComparaciones);
            tabControl.TabPages.Add(tabKpis);

            this.Controls.Add(tabControl);
            this.ResumeLayout(false);
        }

        private void GenerarAnalisis()
        {
            CrearTabResumen();
            CrearTabTendencias();
            CrearTabComparaciones();
            CrearTabKpis();
        }

        private void CrearTabResumen()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true };

            var ventasCompletadas = _ventas.Where(v => v.Estado == "Completada" && v.Tipo == "Venta").ToList();
            var totalFacturado = ventasCompletadas.Sum(v => v.Total);
            var promedioVenta = ventasCompletadas.Count > 0 ? ventasCompletadas.Average(v => v.Total) : 0;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("ANÁLISIS EJECUTIVO DE VENTAS");
            sb.AppendLine(new string('=', 50));
            sb.AppendLine();
            sb.AppendLine("📊 MÉTRICAS PRINCIPALES");
            sb.AppendLine($"• Total facturado: {totalFacturado:C2}");
            sb.AppendLine($"• Número de ventas: {ventasCompletadas.Count}");
            sb.AppendLine($"• Ticket promedio: {promedioVenta:C2}");
            sb.AppendLine($"• Tasa de conversión: {CalcularTasaConversion():P1}");
            sb.AppendLine();

            if (ventasCompletadas.Any())
            {
                sb.AppendLine("👥 RENDIMIENTO POR VENDEDOR");
                var topVendedores = ventasCompletadas.GroupBy(v => v.NombreVendedor)
                                                   .Select(g => new { Vendedor = g.Key, Total = g.Sum(v => v.Total), Ventas = g.Count() })
                                                   .OrderByDescending(x => x.Total)
                                                   .Take(5);

                foreach (var vendedor in topVendedores)
                {
                    sb.AppendLine($"• {vendedor.Vendedor}: {vendedor.Total:C0} ({vendedor.Ventas} ventas)");
                }
                sb.AppendLine();
            }

            sb.AppendLine("🎯 TOP CLIENTES");
            var topClientes = ventasCompletadas.GroupBy(v => v.NombreCliente)
                                             .Select(g => new { Cliente = g.Key, Total = g.Sum(v => v.Total), Ventas = g.Count() })
                                             .OrderByDescending(x => x.Total)
                                             .Take(5);

            if (topClientes.Any())
            {
                foreach (var cliente in topClientes)
                {
                    sb.AppendLine($"• {cliente.Cliente}: {cliente.Total:C0} ({cliente.Ventas} ventas)");
                }
            }
            else
            {
                sb.AppendLine("• No hay datos disponibles");
            }
            sb.AppendLine();

            sb.AppendLine("📅 ANÁLISIS TEMPORAL");
            var ventasPorMes = ventasCompletadas.GroupBy(v => new { v.FechaVenta.Year, v.FechaVenta.Month })
                                              .Select(g => new { 
                                                  Periodo = $"{g.Key.Year}-{g.Key.Month:D2}", 
                                                  Total = g.Sum(v => v.Total),
                                                  Cantidad = g.Count()
                                              })
                                              .OrderBy(x => x.Periodo);

            if (ventasPorMes.Any())
            {
                foreach (var mes in ventasPorMes.Skip(Math.Max(0, ventasPorMes.Count() - 6)))
                {
                    sb.AppendLine($"• {mes.Periodo}: {mes.Total:C0} ({mes.Cantidad} ventas)");
                }
            }
            else
            {
                sb.AppendLine("• No hay datos disponibles");
            }

            var rtb = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                ReadOnly = true,
                BackColor = Color.White,
                Text = sb.ToString()
            };

            panel.Controls.Add(rtb);
            tabResumen.Controls.Add(panel);
        }

        private void CrearTabTendencias()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true };

            var ventasCompletadas = _ventas.Where(v => v.Estado == "Completada" && v.Tipo == "Venta").ToList();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("ANÁLISIS DE TENDENCIAS");
            sb.AppendLine(new string('=', 50));
            sb.AppendLine();

            // Tendencia mensual
            sb.AppendLine("📈 TENDENCIA MENSUAL");
            var ventasPorMes = ventasCompletadas.GroupBy(v => new { v.FechaVenta.Year, v.FechaVenta.Month })
                                              .Select(g => new { 
                                                  Periodo = new DateTime(g.Key.Year, g.Key.Month, 1),
                                                  Total = g.Sum(v => v.Total),
                                                  Cantidad = g.Count()
                                              })
                                              .OrderBy(x => x.Periodo)
                                              .ToList();

            if (ventasPorMes.Count > 1)
            {
                var ultimoMes = ventasPorMes.Last();
                var mesAnterior = ventasPorMes[ventasPorMes.Count - 2];
                var crecimiento = mesAnterior.Total > 0 ? ((ultimoMes.Total - mesAnterior.Total) / mesAnterior.Total) * 100 : 0;
                
                sb.AppendLine($"• Crecimiento mes actual vs anterior: {crecimiento:+0.0;-0.0;0.0}%");
                sb.AppendLine($"• Mes actual: {ultimoMes.Total:C0} ({ultimoMes.Cantidad} ventas)");
                sb.AppendLine($"• Mes anterior: {mesAnterior.Total:C0} ({mesAnterior.Cantidad} ventas)");
            }
            else if (ventasPorMes.Count == 1)
            {
                var mes = ventasPorMes.First();
                sb.AppendLine($"• Solo hay datos de un mes: {mes.Total:C0} ({mes.Cantidad} ventas)");
            }
            else
            {
                sb.AppendLine("• No hay datos suficientes para análisis de tendencia");
            }
            sb.AppendLine();

            // Tendencia por día de la semana
            sb.AppendLine("📅 VENTAS POR DÍA DE LA SEMANA");
            if (ventasCompletadas.Any())
            {
                var ventasPorDia = ventasCompletadas.GroupBy(v => v.FechaVenta.DayOfWeek)
                                                  .Select(g => new { 
                                                      Dia = TranslateDay(g.Key),
                                                      Promedio = g.Average(v => v.Total),
                                                      Cantidad = g.Count()
                                                  })
                                                  .OrderByDescending(x => x.Promedio);

                foreach (var dia in ventasPorDia)
                {
                    sb.AppendLine($"• {dia.Dia}: Promedio {dia.Promedio:C0} ({dia.Cantidad} ventas)");
                }
            }
            else
            {
                sb.AppendLine("• No hay datos disponibles");
            }
            sb.AppendLine();

            // Análisis de estacionalidad
            sb.AppendLine("📊 ANÁLISIS ESTACIONAL");
            if (ventasCompletadas.Any())
            {
                var ventasPorTrimestre = ventasCompletadas.GroupBy(v => $"Q{(v.FechaVenta.Month - 1) / 3 + 1}-{v.FechaVenta.Year}")
                                                        .Select(g => new {
                                                            Trimestre = g.Key,
                                                            Total = g.Sum(v => v.Total),
                                                            Cantidad = g.Count()
                                                        })
                                                        .OrderBy(x => x.Trimestre);

                foreach (var trimestre in ventasPorTrimestre)
                {
                    sb.AppendLine($"• {trimestre.Trimestre}: {trimestre.Total:C0} ({trimestre.Cantidad} ventas)");
                }
            }
            else
            {
                sb.AppendLine("• No hay datos disponibles");
            }

            var rtb = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                ReadOnly = true,
                BackColor = Color.White,
                Text = sb.ToString()
            };

            panel.Controls.Add(rtb);
            tabTendencias.Controls.Add(panel);
        }

        private void CrearTabComparaciones()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true };

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("ANÁLISIS COMPARATIVO");
            sb.AppendLine(new string('=', 50));
            sb.AppendLine();

            // Comparación Ventas vs Cotizaciones
            var ventas = _ventas.Where(v => v.Tipo == "Venta" && v.Estado == "Completada").ToList();
            var cotizaciones = _ventas.Where(v => v.Tipo == "Cotización").ToList();

            sb.AppendLine("💰 VENTAS vs COTIZACIONES");
            sb.AppendLine($"• Ventas completadas: {ventas.Count} ({ventas.Sum(v => v.Total):C0})");
            sb.AppendLine($"• Cotizaciones totales: {cotizaciones.Count} ({cotizaciones.Sum(v => v.Total):C0})");
            sb.AppendLine($"• Cotizaciones pendientes: {cotizaciones.Count(c => c.Estado == "Pendiente")}");
            sb.AppendLine($"• Tasa de conversión estimada: {CalcularTasaConversion():P1}");
            sb.AppendLine();

            // Comparación por vendedor
            sb.AppendLine("👥 COMPARACIÓN ENTRE VENDEDORES");
            if (ventas.Any())
            {
                var ventasPorVendedor = ventas.GroupBy(v => v.NombreVendedor)
                                            .Select(g => new {
                                                Vendedor = g.Key,
                                                Total = g.Sum(v => v.Total),
                                                Cantidad = g.Count(),
                                                Promedio = g.Average(v => v.Total),
                                                MaxVenta = g.Max(v => v.Total)
                                            })
                                            .OrderByDescending(x => x.Total);

                sb.AppendLine($"{"Vendedor",-20} {"Total",-12} {"Ventas",-8} {"Promedio",-10} {"Máxima",-10}");
                sb.AppendLine(new string('-', 65));
                
                foreach (var vendedor in ventasPorVendedor)
                {
                    sb.AppendLine($"{vendedor.Vendedor,-20} {vendedor.Total,-12:C0} {vendedor.Cantidad,-8} {vendedor.Promedio,-10:C0} {vendedor.MaxVenta,-10:C0}");
                }
            }
            else
            {
                sb.AppendLine("• No hay datos de ventas disponibles");
            }
            sb.AppendLine();

            // Análisis de ticket promedio por tipo de cliente
            sb.AppendLine("🏢 ANÁLISIS POR TIPO DE CLIENTE");
            var clientesPersonaFisica = ventas.Where(v => string.IsNullOrEmpty(v.EmpresaCliente)).ToList();
            var clientesEmpresa = ventas.Where(v => !string.IsNullOrEmpty(v.EmpresaCliente)).ToList();

            sb.AppendLine($"• Clientes Persona Física: {clientesPersonaFisica.Count} ventas, promedio: {(clientesPersonaFisica.Count > 0 ? clientesPersonaFisica.Average(v => v.Total) : 0):C0}");
            sb.AppendLine($"• Clientes Empresa: {clientesEmpresa.Count} ventas, promedio: {(clientesEmpresa.Count > 0 ? clientesEmpresa.Average(v => v.Total) : 0):C0}");

            var rtb = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                ReadOnly = true,
                BackColor = Color.White,
                Text = sb.ToString()
            };

            panel.Controls.Add(rtb);
            tabComparaciones.Controls.Add(panel);
        }

        private void CrearTabKpis()
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(20),
                BackColor = Color.White
            };

            // Configurar estilos
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33f));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33f));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33f));

            var ventasCompletadas = _ventas.Where(v => v.Estado == "Completada" && v.Tipo == "Venta").ToList();
            var totalFacturado = ventasCompletadas.Sum(v => v.Total);
            var ticketPromedio = ventasCompletadas.Count > 0 ? ventasCompletadas.Average(v => v.Total) : 0;
            var ventasMesActual = ventasCompletadas.Count(v => v.FechaVenta.Month == DateTime.Now.Month && v.FechaVenta.Year == DateTime.Now.Year);
            var tasaConversion = CalcularTasaConversion();
            var mejorVendedor = ObtenerMejorVendedor();
            var crecimiento = CalcularCrecimientoMensual();

            // KPI Cards
            var kpis = new[]
            {
                new { Titulo = "Total Facturado", Valor = totalFacturado.ToString("C0"), Desc = "Ventas completadas" },
                new { Titulo = "Ticket Promedio", Valor = ticketPromedio.ToString("C0"), Desc = "Promedio por venta" },
                new { Titulo = "Ventas del Mes", Valor = ventasMesActual.ToString(), Desc = "Mes actual" },
                new { Titulo = "Tasa Conversión", Valor = tasaConversion.ToString("P1"), Desc = "Cotizaciones → Ventas" },
                new { Titulo = "Mejor Vendedor", Valor = mejorVendedor, Desc = "Por volumen de ventas" },
                new { Titulo = "Crecimiento", Valor = crecimiento.ToString("+0.0%;-0.0%;0.0%"), Desc = "vs mes anterior" }
            };

            for (int i = 0; i < kpis.Length; i++)
            {
                var card = CrearKpiCard(kpis[i].Titulo, kpis[i].Valor, kpis[i].Desc);
                panel.Controls.Add(card, i % 2, i / 2);
            }

            tabKpis.Controls.Add(panel);
        }

        private Panel CrearKpiCard(string titulo, string valor, string descripcion)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(246, 250, 246),
                Margin = new Padding(10),
                Padding = new Padding(20)
            };

            var lblTitulo = new Label
            {
                Text = titulo,
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(90, 100, 90),
                Location = new Point(0, 0)
            };

            var lblValor = new Label
            {
                Text = valor,
                AutoSize = true,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(34, 139, 34),
                Location = new Point(0, 30)
            };

            var lblDesc = new Label
            {
                Text = descripcion,
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(120, 130, 120),
                Location = new Point(0, 80)
            };

            card.Controls.Add(lblTitulo);
            card.Controls.Add(lblValor);
            card.Controls.Add(lblDesc);

            return card;
        }

        private decimal CalcularTasaConversion()
        {
            var cotizaciones = _ventas.Where(v => v.Tipo == "Cotización").Count();
            var ventas = _ventas.Where(v => v.Tipo == "Venta" && v.Estado == "Completada").Count();
            
            if (cotizaciones == 0) return ventas > 0 ? 1 : 0;
            return (decimal)ventas / (cotizaciones + ventas);
        }

        private string ObtenerMejorVendedor()
        {
            var ventasCompletadas = _ventas.Where(v => v.Estado == "Completada" && v.Tipo == "Venta").ToList();
            if (!ventasCompletadas.Any()) return "N/A";

            var mejor = ventasCompletadas.GroupBy(v => v.NombreVendedor)
                                       .OrderByDescending(g => g.Sum(v => v.Total))
                                       .FirstOrDefault();

            return mejor?.Key ?? "N/A";
        }

        private decimal CalcularCrecimientoMensual()
        {
            var ventasCompletadas = _ventas.Where(v => v.Estado == "Completada" && v.Tipo == "Venta").ToList();
            
            var fechaActual = DateTime.Now;
            var primerDiaMesActual = new DateTime(fechaActual.Year, fechaActual.Month, 1);
            var primerDiaMesSiguiente = primerDiaMesActual.AddMonths(1);
            
            var fechaAnterior = fechaActual.AddMonths(-1);
            var primerDiaMesAnterior = new DateTime(fechaAnterior.Year, fechaAnterior.Month, 1);
            var primerDiaMesActual2 = primerDiaMesAnterior.AddMonths(1);
            
            var mesActual = ventasCompletadas.Where(v => v.FechaVenta >= primerDiaMesActual && v.FechaVenta < primerDiaMesSiguiente)
                                           .Sum(v => v.Total);
            
            var mesAnterior = ventasCompletadas.Where(v => v.FechaVenta >= primerDiaMesAnterior && v.FechaVenta < primerDiaMesActual2)
                                            .Sum(v => v.Total);

            if (mesAnterior == 0) return mesActual > 0 ? 100 : 0;
            return ((mesActual - mesAnterior) / mesAnterior) * 100;
        }
        

        private string TranslateDay(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Sunday: return "Domingo";
                case DayOfWeek.Monday: return "Lunes";
                case DayOfWeek.Tuesday: return "Martes";
                case DayOfWeek.Wednesday: return "Miércoles";
                case DayOfWeek.Thursday: return "Jueves";
                case DayOfWeek.Friday: return "Viernes";
                case DayOfWeek.Saturday: return "Sábado";
                default: return day.ToString();
            }
        }
    }
}