using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Proyecto_Taller_2.Domain.Models;

namespace Proyecto_Taller_2.Forms
{
    public partial class ReporteVentasForm : Form
    {
        private readonly List<Venta> _ventas;
        private readonly List<Usuario> _vendedores;

        // Controles
        private TableLayoutPanel tlRoot;
        private GroupBox gbFiltros, gbReporte;
        private ComboBox cbPeriodo, cbVendedor, cbTipoReporte;
        private DateTimePicker dtpDesde, dtpHasta;
        private Button btnGenerar, btnExportar, btnCerrar;
        private RichTextBox rtbReporte;

        public ReporteVentasForm(List<Venta> ventas, List<Usuario> vendedores)
        {
            _ventas = ventas;
            _vendedores = vendedores;
            
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form
            this.Text = "Generar Reporte de Ventas";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(800, 600);

            // Root Layout
            tlRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(16)
            };
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 160)); // Filtros - Aumentado de 120 a 160
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Reporte
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // Botones

            // === FILTROS ===
            gbFiltros = new GroupBox
            {
                Text = "Filtros de Reporte",
                Dock = DockStyle.Fill,
                Padding = new Padding(12)
            };

            var tlFiltros = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 4  // Aumentado a 4 filas
            };

            // Fila 1
            tlFiltros.Controls.Add(new Label { Text = "Tipo de Reporte:", Anchor = AnchorStyles.Left | AnchorStyles.Bottom }, 0, 0);
            tlFiltros.Controls.Add(new Label { Text = "Periodo:", Anchor = AnchorStyles.Left | AnchorStyles.Bottom }, 1, 0);
            tlFiltros.Controls.Add(new Label { Text = "Vendedor:", Anchor = AnchorStyles.Left | AnchorStyles.Bottom }, 2, 0);

            cbTipoReporte = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 4, 8, 0)
            };
            cbTipoReporte.Items.AddRange(new[] { "Resumen General", "Por Vendedor", "Por Cliente", "Por Producto", "Comparativo Mensual" });
            cbTipoReporte.SelectedIndex = 0;

            cbPeriodo = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 4, 8, 0)
            };
            cbPeriodo.Items.AddRange(new[] { "Este mes", "Mes anterior", "�ltimos 3 meses", "Este a�o", "Personalizado" });
            cbPeriodo.SelectedIndex = 0;
            cbPeriodo.SelectedIndexChanged += CbPeriodo_SelectedIndexChanged;

            cbVendedor = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 4, 8, 0)
            };
            cbVendedor.Items.Add("Todos los vendedores");
            foreach (var vendedor in _vendedores)
                cbVendedor.Items.Add($"{vendedor.Nombre} {vendedor.Apellido}");
            cbVendedor.SelectedIndex = 0;

            tlFiltros.Controls.Add(cbTipoReporte, 0, 1);
            tlFiltros.Controls.Add(cbPeriodo, 1, 1);
            tlFiltros.Controls.Add(cbVendedor, 2, 1);

            // Fila 2 (Fechas personalizadas - inicialmente ocultas)
            tlFiltros.Controls.Add(new Label { Text = "Desde:", Anchor = AnchorStyles.Left | AnchorStyles.Bottom, Visible = false }, 0, 2);
            tlFiltros.Controls.Add(new Label { Text = "Hasta:", Anchor = AnchorStyles.Left | AnchorStyles.Bottom, Visible = false }, 1, 2);

            dtpDesde = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Short,
                Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                Visible = false,
                Margin = new Padding(0, 4, 8, 0)
            };

            dtpHasta = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now,
                Visible = false,
                Margin = new Padding(0, 4, 8, 0)
            };

            btnGenerar = new Button
            {
                Text = "Generar Reporte",
                Dock = DockStyle.Fill,
                Height = 36,
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Margin = new Padding(0, 8, 0, 4),
                Cursor = Cursors.Hand
            };
            btnGenerar.FlatAppearance.BorderSize = 0;
            btnGenerar.Click += BtnGenerar_Click;

            tlFiltros.Controls.Add(dtpDesde, 0, 3);
            tlFiltros.Controls.Add(dtpHasta, 1, 3);
            tlFiltros.Controls.Add(btnGenerar, 2, 3);

            // Configurar estilos
            for (int i = 0; i < 4; i++)
                tlFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            tlFiltros.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));  // Labels
            tlFiltros.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));  // Combos - Aumentado de 30 a 34
            tlFiltros.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));  // Labels fechas
            tlFiltros.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));  // Fechas + Botón - Cambiado de Percent a Absolute

            gbFiltros.Controls.Add(tlFiltros);

            // === REPORTE ===
            gbReporte = new GroupBox
            {
                Text = "Reporte Generado",
                Dock = DockStyle.Fill,
                Padding = new Padding(12)
            };

            rtbReporte = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10),
                ReadOnly = true,
                BackColor = Color.White,
                Text = "Seleccione los filtros y haga clic en 'Generar Reporte' para ver los resultados."
            };

            gbReporte.Controls.Add(rtbReporte);

            // === BOTONES ===
            var flAcciones = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 8, 0, 0)
            };

            btnCerrar = new Button
            {
                Text = "Cerrar",
                Height = 34,
                Width = 100,
                DialogResult = DialogResult.OK
            };

            btnExportar = new Button
            {
                Text = "Exportar Reporte",
                Height = 34,
                Width = 120,
                Margin = new Padding(8, 0, 0, 0),
                Enabled = false
            };
            btnExportar.Click += BtnExportar_Click;

            flAcciones.Controls.Add(btnCerrar);
            flAcciones.Controls.Add(btnExportar);

            // Agregar al layout principal
            tlRoot.Controls.Add(gbFiltros, 0, 0);
            tlRoot.Controls.Add(gbReporte, 0, 1);
            tlRoot.Controls.Add(flAcciones, 0, 2);

            this.Controls.Add(tlRoot);
            this.ResumeLayout(false);
        }

        private void CbPeriodo_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool esPersonalizado = cbPeriodo.SelectedIndex == 4; // "Personalizado"
            
            // Encontrar y mostrar/ocultar controles de fecha
            var tlFiltros = gbFiltros.Controls[0] as TableLayoutPanel;
            foreach (Control control in tlFiltros.Controls)
            {
                if (control is Label && (control.Text == "Desde:" || control.Text == "Hasta:"))
                    control.Visible = esPersonalizado;
                else if (control == dtpDesde || control == dtpHasta)
                    control.Visible = esPersonalizado;
            }
        }

        private void BtnGenerar_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                
                var ventasFiltradas = FiltrarVentas();
                var reporte = GenerarReporte(ventasFiltradas);
                
                rtbReporte.Clear();
                rtbReporte.AppendText(reporte);
                btnExportar.Enabled = true;
                
                this.Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show($"Error al generar reporte: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<Venta> FiltrarVentas()
        {
            var ventas = _ventas.AsEnumerable();

            // Filtrar por per�odo
            DateTime fechaDesde, fechaHasta;
            switch (cbPeriodo.SelectedIndex)
            {
                case 0: // Este mes
                    fechaDesde = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    fechaHasta = DateTime.Now;
                    break;
                case 1: // Mes anterior
                    var mesAnterior = DateTime.Now.AddMonths(-1);
                    fechaDesde = new DateTime(mesAnterior.Year, mesAnterior.Month, 1);
                    fechaHasta = fechaDesde.AddMonths(1).AddDays(-1);
                    break;
                case 2: // �ltimos 3 meses
                    fechaDesde = DateTime.Now.AddMonths(-3);
                    fechaHasta = DateTime.Now;
                    break;
                case 3: // Este a�o
                    fechaDesde = new DateTime(DateTime.Now.Year, 1, 1);
                    fechaHasta = DateTime.Now;
                    break;
                case 4: // Personalizado
                    fechaDesde = dtpDesde.Value;
                    fechaHasta = dtpHasta.Value;
                    break;
                default:
                    fechaDesde = DateTime.MinValue;
                    fechaHasta = DateTime.MaxValue;
                    break;
            }

            ventas = ventas.Where(v => v.FechaVenta >= fechaDesde && v.FechaVenta <= fechaHasta);

            // Filtrar por vendedor
            if (cbVendedor.SelectedIndex > 0)
            {
                var vendedorNombre = cbVendedor.SelectedItem.ToString();
                ventas = ventas.Where(v => v.NombreVendedor == vendedorNombre);
            }

            return ventas.ToList();
        }

        private string GenerarReporte(List<Venta> ventas)
        {
            var sb = new System.Text.StringBuilder();
            var tipoReporte = cbTipoReporte.SelectedItem.ToString();

            sb.AppendLine($"REPORTE DE VENTAS - {tipoReporte.ToUpper()}");
            sb.AppendLine($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"Per�odo: {cbPeriodo.SelectedItem}");
            if (cbVendedor.SelectedIndex > 0)
                sb.AppendLine($"Vendedor: {cbVendedor.SelectedItem}");
            sb.AppendLine(new string('=', 80));
            sb.AppendLine();

            switch (cbTipoReporte.SelectedIndex)
            {
                case 0: // Resumen General
                    GenerarResumenGeneral(sb, ventas);
                    break;
                case 1: // Por Vendedor
                    GenerarReportePorVendedor(sb, ventas);
                    break;
                case 2: // Por Cliente
                    GenerarReportePorCliente(sb, ventas);
                    break;
                case 3: // Por Producto
                    sb.AppendLine("REPORTE POR PRODUCTO - En desarrollo");
                    break;
                case 4: // Comparativo Mensual
                    GenerarComparativoMensual(sb, ventas);
                    break;
            }

            return sb.ToString();
        }

        private void GenerarResumenGeneral(System.Text.StringBuilder sb, List<Venta> ventas)
        {
            var ventasCompletadas = ventas.Where(v => v.Estado == "Completada" && v.Tipo == "Venta").ToList();
            var cotizaciones = ventas.Where(v => v.Tipo == "Cotización").ToList();

            sb.AppendLine("RESUMEN GENERAL");
            sb.AppendLine(new string('-', 40));
            sb.AppendLine($"Total de registros: {ventas.Count}");
            sb.AppendLine($"Ventas completadas: {ventasCompletadas.Count}");
            sb.AppendLine($"Cotizaciones: {cotizaciones.Count}");
            sb.AppendLine($"Ventas pendientes: {ventas.Count(v => v.Estado == "Pendiente")}");
            sb.AppendLine($"Ventas canceladas: {ventas.Count(v => v.Estado == "Cancelada")}");
            sb.AppendLine();
            sb.AppendLine($"Total facturado: {ventasCompletadas.Sum(v => v.Total):C2}");
            sb.AppendLine($"Ticket promedio: {(ventasCompletadas.Count > 0 ? ventasCompletadas.Average(v => v.Total) : 0):C2}");
            sb.AppendLine($"Venta máxima: {(ventasCompletadas.Count > 0 ? ventasCompletadas.Max(v => v.Total) : 0):C2}");
            sb.AppendLine($"Venta mínima: {(ventasCompletadas.Count > 0 ? ventasCompletadas.Min(v => v.Total) : 0):C2}");
        }

        private void GenerarReportePorVendedor(System.Text.StringBuilder sb, List<Venta> ventas)
        {
            sb.AppendLine("REPORTE POR VENDEDOR");
            sb.AppendLine(new string('-', 60));
            sb.AppendLine($"{"Vendedor",-25} {"Ventas",-8} {"Total",-12} {"Promedio",-12}");
            sb.AppendLine(new string('-', 60));

            var porVendedor = ventas.Where(v => v.Estado == "Completada" && v.Tipo == "Venta")
                                  .GroupBy(v => v.NombreVendedor)
                                  .Select(g => new
                                  {
                                      Vendedor = g.Key,
                                      Cantidad = g.Count(),
                                      Total = g.Sum(v => v.Total),
                                      Promedio = g.Average(v => v.Total)
                                  })
                                  .OrderByDescending(x => x.Total);

            foreach (var item in porVendedor)
            {
                sb.AppendLine($"{item.Vendedor,-25} {item.Cantidad,-8} {item.Total,-12:C0} {item.Promedio,-12:C0}");
            }
        }

        private void GenerarReportePorCliente(System.Text.StringBuilder sb, List<Venta> ventas)
        {
            sb.AppendLine("REPORTE POR CLIENTE (TOP 10)");
            sb.AppendLine(new string('-', 60));
            sb.AppendLine($"{"Cliente",-25} {"Ventas",-8} {"Total",-12} {"Promedio",-12}");
            sb.AppendLine(new string('-', 60));

            var porCliente = ventas.Where(v => v.Estado == "Completada" && v.Tipo == "Venta")
                                 .GroupBy(v => v.NombreCliente)
                                 .Select(g => new
                                 {
                                     Cliente = g.Key,
                                     Cantidad = g.Count(),
                                     Total = g.Sum(v => v.Total),
                                     Promedio = g.Average(v => v.Total)
                                 })
                                 .OrderByDescending(x => x.Total)
                                 .Take(10);

            foreach (var item in porCliente)
            {
                sb.AppendLine($"{item.Cliente,-25} {item.Cantidad,-8} {item.Total,-12:C0} {item.Promedio,-12:C0}");
            }
        }

        private void GenerarComparativoMensual(System.Text.StringBuilder sb, List<Venta> ventas)
        {
            sb.AppendLine("COMPARATIVO MENSUAL");
            sb.AppendLine(new string('-', 50));
            sb.AppendLine($"{"Mes",-15} {"Ventas",-8} {"Total",-12} {"Promedio",-12}");
            sb.AppendLine(new string('-', 50));

            var porMes = ventas.Where(v => v.Estado == "Completada" && v.Tipo == "Venta")
                             .GroupBy(v => new { v.FechaVenta.Year, v.FechaVenta.Month })
                             .Select(g => new
                             {
                                 Mes = $"{g.Key.Year}-{g.Key.Month:D2}",
                                 Cantidad = g.Count(),
                                 Total = g.Sum(v => v.Total),
                                 Promedio = g.Average(v => v.Total)
                             })
                             .OrderBy(x => x.Mes);

            foreach (var item in porMes)
            {
                sb.AppendLine($"{item.Mes,-15} {item.Cantidad,-8} {item.Total,-12:C0} {item.Promedio,-12:C0}");
            }
        }

        private void BtnExportar_Click(object sender, EventArgs e)
        {
            try
            {
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "Archivo CSV (*.csv)|*.csv|Archivo Excel (*.xlsx)|*.xlsx|Archivo de Texto (*.txt)|*.txt";
                    saveDialog.Title = "Exportar Reporte";
                    saveDialog.FileName = $"Reporte_Ventas_{DateTime.Now:yyyy-MM-dd_HHmm}";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        var extension = System.IO.Path.GetExtension(saveDialog.FileName).ToLower();
                        
                        switch (extension)
                        {
                            case ".csv":
                                ExportarCSV(saveDialog.FileName);
                                break;
                            case ".xlsx":
                                ExportarExcel(saveDialog.FileName);
                                break;
                            case ".txt":
                                System.IO.File.WriteAllText(saveDialog.FileName, rtbReporte.Text, System.Text.Encoding.UTF8);
                                break;
                        }
                        
                        MessageBox.Show($"Reporte exportado exitosamente a:\n{saveDialog.FileName}", 
                            "Exportación Completa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar reporte: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportarCSV(string filePath)
        {
            var ventas = FiltrarVentas();
            var sb = new System.Text.StringBuilder();

            // Encabezado
            sb.AppendLine("Número Venta,Fecha,Vendedor,Cliente,Tipo,Estado,Total,Observaciones");

            // Datos
            foreach (var venta in ventas.OrderByDescending(v => v.FechaVenta))
            {
                sb.AppendLine($"\"{venta.NumeroVenta}\",\"{venta.FechaVenta:dd/MM/yyyy}\",\"{venta.NombreVendedor}\",\"{venta.NombreCliente}\",\"{venta.Tipo}\",\"{venta.Estado}\",{venta.Total},\"{venta.Observaciones?.Replace("\"", "\"\"")}\"");
            }

            System.IO.File.WriteAllText(filePath, sb.ToString(), System.Text.Encoding.UTF8);
        }

        private void ExportarExcel(string filePath)
        {
            try
            {
                var ventas = FiltrarVentas();
                
                // Crear archivo CSV temporal que Excel puede abrir
                var csvContent = new System.Text.StringBuilder();
                
                // Información del reporte
                csvContent.AppendLine($"REPORTE DE VENTAS - {cbTipoReporte.SelectedItem}");
                csvContent.AppendLine($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}");
                csvContent.AppendLine($"Período: {cbPeriodo.SelectedItem}");
                if (cbVendedor.SelectedIndex > 0)
                    csvContent.AppendLine($"Vendedor: {cbVendedor.SelectedItem}");
                csvContent.AppendLine();

                // Resumen
                var ventasCompletadas = ventas.Where(v => v.Estado == "Completada" && v.Tipo == "Venta").ToList();
                csvContent.AppendLine("RESUMEN");
                csvContent.AppendLine($"Total Ventas,{ventasCompletadas.Count}");
                csvContent.AppendLine($"Total Facturado,{ventasCompletadas.Sum(v => v.Total):F2}");
                csvContent.AppendLine($"Ticket Promedio,{(ventasCompletadas.Count > 0 ? ventasCompletadas.Average(v => v.Total) : 0):F2}");
                csvContent.AppendLine();

                // Detalle de ventas
                csvContent.AppendLine("DETALLE DE VENTAS");
                csvContent.AppendLine("Número,Fecha,Vendedor,Cliente,Empresa,Tipo,Estado,Total,Observaciones");

                foreach (var venta in ventas.OrderByDescending(v => v.FechaVenta))
                {
                    var empresa = !string.IsNullOrEmpty(venta.EmpresaCliente) ? venta.EmpresaCliente : "-";
                    csvContent.AppendLine($"\"{venta.NumeroVenta}\",\"{venta.FechaVenta:dd/MM/yyyy}\",\"{venta.NombreVendedor}\",\"{venta.NombreCliente}\",\"{empresa}\",\"{venta.Tipo}\",\"{venta.Estado}\",{venta.Total:F2},\"{venta.Observaciones?.Replace("\"", "\"\"")}\"");
                }

                csvContent.AppendLine();
                csvContent.AppendLine("ANÁLISIS POR VENDEDOR");
                csvContent.AppendLine("Vendedor,Cantidad Ventas,Total Facturado,Promedio por Venta");
                
                var porVendedor = ventasCompletadas.GroupBy(v => v.NombreVendedor)
                                                  .Select(g => new
                                                  {
                                                      Vendedor = g.Key,
                                                      Cantidad = g.Count(),
                                                      Total = g.Sum(v => v.Total),
                                                      Promedio = g.Average(v => v.Total)
                                                  })
                                                  .OrderByDescending(x => x.Total);

                foreach (var item in porVendedor)
                {
                    csvContent.AppendLine($"\"{item.Vendedor}\",{item.Cantidad},{item.Total:F2},{item.Promedio:F2}");
                }

                // Guardar con codificación UTF-8 con BOM para que Excel lo abra correctamente
                var encoding = new System.Text.UTF8Encoding(true);
                System.IO.File.WriteAllText(filePath, csvContent.ToString(), encoding);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al exportar a Excel: {ex.Message}", ex);
            }
        }
    }
}