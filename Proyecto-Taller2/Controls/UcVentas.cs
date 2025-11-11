using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Proyecto_Taller_2.Data;
using Proyecto_Taller_2.Data.Repositories;
using Proyecto_Taller_2.Domain.Models;

namespace Proyecto_Taller_2.Controls
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

        // ===== Datos y Repositorios =====
        private readonly VentaRepository _ventaRepo;
        private readonly Usuario _currentUser;
        private readonly BindingList<Venta> _ventas = new BindingList<Venta>();
        private List<Venta> _allVentas = new List<Venta>();
        private List<Usuario> _vendedores = new List<Usuario>();

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

        // ===== Paleta de colores =====
        private readonly Color ColBg = Color.White;
        private readonly Color ColSoft = Color.FromArgb(246, 250, 246);
        private readonly Color ColSoftAlt = Color.FromArgb(236, 243, 236);
        private readonly Color ColText = Color.FromArgb(34, 47, 34);
        private readonly Color ColAccent = Color.FromArgb(34, 139, 34);
        private readonly Color ColBorder = Color.FromArgb(210, 220, 210);

        public UcVentas(Usuario currentUser = null)
        {
            try
            {
                _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
                _ventaRepo = new VentaRepository(BDGeneral.ConnectionString);

                DoubleBuffered = true;
                Dock = DockStyle.Fill;
                BackColor = ColBg;

                BuildUI();
                WireEvents();

                if (!DesignMode)
                {
                    this.Load += (s, e) => 
                    {
                        CargarVendedores();
                        RefrescarDatos();
                    };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inicializando control de ventas: {ex.Message}", "Error de Inicialización", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // Crear interfaz mínima de error
                this.Controls.Clear();
                var errorLabel = new Label
                {
                    Text = $"Error: {ex.Message}",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.Red
                };
                this.Controls.Add(errorLabel);
            }
        }

        // Constructor sin parámetros para el diseñador
        public UcVentas() : this(new Usuario { IdRol = 1, RolNombre = "Administrador" })
        {
            // Solo para el diseñador
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
                BackColor = ColBg
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
                BackColor = ColBg
            };

            // Crear botones según el rol del usuario
            if (_currentUser.IdRol == 2) // Vendedor
            {
                var btnNuevaVenta = MakeActionButton("+ Nueva Venta");
                var btnNuevaCot = MakeGhostButton("Nueva Cotización");
                var btnExportar = MakeGhostButton("Exportar Mis Ventas");
                
                btnNuevaVenta.Click += BtnNuevaVenta_Click;
                btnNuevaCot.Click += BtnNuevaCotizacion_Click;
                btnExportar.Click += BtnExportar_Click;
                
                flAcciones.Controls.Add(btnNuevaVenta);
                flAcciones.Controls.Add(btnNuevaCot);
                flAcciones.Controls.Add(btnExportar);
            }
            else // Administrador
            {
                var btnReporte = MakeActionButton("Generar Reporte");
                var btnExportar = MakeGhostButton("Exportar Todo");
                var btnAnalisis = MakeGhostButton("Análisis Detallado");
                
                btnReporte.Click += BtnGenerarReporte_Click;
                btnExportar.Click += BtnExportar_Click;
                btnAnalisis.Click += BtnAnalisisDetallado_Click;
                
                flAcciones.Controls.Add(btnReporte);
                flAcciones.Controls.Add(btnExportar);
                flAcciones.Controls.Add(btnAnalisis);
            }
            
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

            // Los KPIs se crearán dinámicamente en RefrescarKpis()
            tlRoot.Controls.Add(tlKpis, 0, 1);

            // --------- Buscar y Filtrar ----------
            gbBuscar = new GroupBox
            {
                Text = _currentUser.IdRol == 2 ? "Buscar y Filtrar Mis Ventas" : "Buscar y Filtrar Ventas",
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                ForeColor = ColText
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
            SetPlaceholder(txtBuscar, "Buscar por número, cliente o producto...");

            pnlSearch.Padding = new Padding(0, 2, 0, 0);
            pnlSearch.Controls.Add(txtBuscar);
            tlBuscar.SetColumnSpan(pnlSearch, 4);
            tlBuscar.Controls.Add(pnlSearch, 0, 0);

            // Fila 2: Estado, Tipo, Periodo, Vendedor
            cbEstado = MakeCombo(new[] { "Todos los estados", "Pendiente", "Completada", "Cancelada" });
            cbTipo = MakeCombo(new[] { "Todos los tipos", "Venta", "Cotización", "Devolución" });
            cbPeriodo = MakeCombo(new[] { "Todos los períodos", "Este mes", "Mes anterior", "Últimos 90 días" });
            cbVendedor = MakeCombo(new[] { "Todos los vendedores" });

            // Si es vendedor, ocultar el filtro de vendedor
            if (_currentUser.IdRol == 2)
            {
                cbVendedor.Visible = false;
                var lblVendedor = new Label 
                { 
                    Text = $"Vendedor: {_currentUser.Nombre} {_currentUser.Apellido}",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = ColText
                };
                tlBuscar.Controls.Add(lblVendedor, 3, 1);
            }
            else
            {
                tlBuscar.Controls.Add(cbVendedor, 3, 1);
            }

            tlBuscar.Controls.Add(cbEstado, 0, 1);
            tlBuscar.Controls.Add(cbTipo, 1, 1);
            tlBuscar.Controls.Add(cbPeriodo, 2, 1);

            // Fila 3: botón aplicar
            btnAplicarFiltros = new Button
            {
                Text = "Aplicar filtros",
                Dock = DockStyle.Right,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = ColAccent,
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
                Text = _currentUser.IdRol == 2 ? "Mis Ventas" : "Lista de Ventas de Todos los Vendedores",
                Dock = DockStyle.Fill,
                Padding = new Padding(8),
                ForeColor = ColText
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
                BorderStyle = BorderStyle.FixedSingle,
                ColumnHeadersHeight = 40,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                GridColor = ColBorder,
                EnableHeadersVisualStyles = false,
                AutoGenerateColumns = false
            };
            
            dgv.ColumnHeadersDefaultCellStyle.BackColor = ColSoftAlt;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = ColText;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 232, 220);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.RowTemplate.Height = 40;

            CrearColumnasDataGridView();

            gbLista.Controls.Add(dgv);
            tlRoot.Controls.Add(gbLista, 0, 3);
        }

        private void CrearColumnasDataGridView()
        {
            dgv.Columns.Clear();

            // Definir columnas
            var columns = new List<DataGridViewColumn>
            {
                new DataGridViewTextBoxColumn 
                { 
                    Name = "NumeroVenta", 
                    HeaderText = "Número", 
                    DataPropertyName = "NumeroVenta",
                    Width = 100
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Cliente", 
                    HeaderText = "Cliente", 
                    Width = 200
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Tipo", 
                    HeaderText = "Tipo", 
                    DataPropertyName = "Tipo",
                    Width = 100
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Estado", 
                    HeaderText = "Estado", 
                    DataPropertyName = "Estado",
                    Width = 100
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Total", 
                    HeaderText = "Total", 
                    DataPropertyName = "Total",
                    Width = 100,
                    DefaultCellStyle = new DataGridViewCellStyle 
                    { 
                        Format = "C2",
                        Alignment = DataGridViewContentAlignment.MiddleRight
                    }
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "FechaVenta", 
                    HeaderText = "Fecha", 
                    DataPropertyName = "FechaVenta",
                    Width = 100,
                    DefaultCellStyle = new DataGridViewCellStyle 
                    { 
                        Format = "dd/MM/yyyy"
                    }
                }
            };

            // Solo mostrar columna de vendedor si es administrador
            if (_currentUser.IdRol == 1)
            {
                columns.Add(new DataGridViewTextBoxColumn 
                { 
                    Name = "NombreVendedor", 
                    HeaderText = "Vendedor", 
                    DataPropertyName = "NombreVendedor",
                    Width = 150
                });
            }

            columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "Acciones", 
                HeaderText = "Acciones", 
                Width = 80
            });

            dgv.Columns.AddRange(columns.ToArray());

            // Configurar DataSource
            dgv.DataSource = _ventas;
        }

        private void WireEvents()
        {
            // Filtros con debouncing
            Timer searchTimer = new Timer { Interval = 500 };
            searchTimer.Tick += (s, e) => 
            {
                searchTimer.Stop();
                AplicarFiltros();
            };
            
            txtBuscar.TextChanged += (s, e) => 
            {
                searchTimer.Stop();
                searchTimer.Start();
            };

            cbEstado.SelectedIndexChanged += (s, e) => AplicarFiltros();
            cbTipo.SelectedIndexChanged += (s, e) => AplicarFiltros();
            cbPeriodo.SelectedIndexChanged += (s, e) => AplicarFiltros();
            if (cbVendedor.Visible)
                cbVendedor.SelectedIndexChanged += (s, e) => AplicarFiltros();

            btnAplicarFiltros.Click += (s, e) => AplicarFiltros();

            // Eventos del DataGridView
            dgv.CellFormatting += Dgv_CellFormatting;
            dgv.CellPainting += Dgv_CellPainting;
            dgv.CellClick += Dgv_CellClick;
        }

        // ================ Manejo de datos ==================
        private void CargarVendedores()
        {
            try
            {
                _vendedores = _ventaRepo.ObtenerVendedores();
                
                if (_currentUser.IdRol == 1 && cbVendedor.Visible) // Solo si es admin y el combo es visible
                {
                    cbVendedor.Items.Clear();
                    cbVendedor.Items.Add("Todos los vendedores");
                    
                    foreach (var vendedor in _vendedores)
                    {
                        cbVendedor.Items.Add($"{vendedor.Nombre} {vendedor.Apellido}");
                    }
                    
                    cbVendedor.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar vendedores: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RefrescarDatos()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                // Obtener ventas según el rol
                int? usuarioId = _currentUser.IdRol == 2 ? _currentUser.IdUsuario : (int?)null;
                _allVentas = _ventaRepo.ObtenerTodas(usuarioId);
                
                RefrescarKpis();
                AplicarFiltros();

                this.Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefrescarKpis()
        {
            try
            {
                // Limpiar KPIs existentes
                tlKpis.Controls.Clear();

                int? usuarioId = _currentUser.IdRol == 2 ? _currentUser.IdUsuario : (int?)null;
                var fechaDesde = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var fechaHasta = fechaDesde.AddMonths(1).AddDays(-1);
                
                var kpis = _ventaRepo.ObtenerKpis(usuarioId, fechaDesde, fechaHasta);

                if (_currentUser.IdRol == 1) // Administrador
                {
                    // Para administradores, mostrar KPIs globales y agregar información adicional
                    var kpisGlobales = _ventaRepo.ObtenerKpisGlobales(fechaDesde, fechaHasta);
                    
                    cardVentasMes = MakeKpiCard("Ventas del Mes", 
                        kpis.VentasDelMes.ToString("C0"), 
                        $"{FormatPorcentaje(kpis.PorcentajeVsAnterior)} vs. mes anterior");
                        
                    cardOrdenes = MakeKpiCard("Total Órdenes", 
                        kpis.TotalOrdenes.ToString(), 
                        $"{FormatPorcentaje(kpis.PorcentajeOrdenesAnterior)} este mes");
                        
                    cardTicket = MakeKpiCard("Ticket Promedio", 
                        kpis.TicketPromedio.ToString("C0"), 
                        $"{FormatPorcentaje(kpis.PorcentajeTicketAnterior)} vs. promedio");
                        
                    cardCotizaciones = MakeKpiCard("Cotizaciones", 
                        kpis.CotizacionesPendientes.ToString(), 
                        $"{kpis.TotalCotizaciones} total este mes");
                }
                else // Vendedor
                {
                    cardVentasMes = MakeKpiCard("Mis Ventas del Mes", 
                        kpis.VentasDelMes.ToString("C0"), 
                        $"{FormatPorcentaje(kpis.PorcentajeVsAnterior)} vs. mes anterior");
                        
                    cardOrdenes = MakeKpiCard("Mis Órdenes", 
                        kpis.TotalOrdenes.ToString(), 
                        $"{FormatPorcentaje(kpis.PorcentajeOrdenesAnterior)} este mes");
                        
                    cardTicket = MakeKpiCard("Mi Ticket Promedio", 
                        kpis.TicketPromedio.ToString("C0"), 
                        $"{FormatPorcentaje(kpis.PorcentajeTicketAnterior)} vs. promedio");
                        
                    cardCotizaciones = MakeKpiCard("Mis Cotizaciones", 
                        kpis.CotizacionesPendientes.ToString(), 
                        $"{kpis.TotalCotizaciones} total este mes");
                }

                tlKpis.Controls.Add(cardVentasMes, 0, 0);
                tlKpis.Controls.Add(cardOrdenes, 1, 0);
                tlKpis.Controls.Add(cardTicket, 2, 0);
                tlKpis.Controls.Add(cardCotizaciones, 3, 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar KPIs: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                
                // Mostrar KPIs vacíos en caso de error
                tlKpis.Controls.Clear();
                tlKpis.Controls.Add(MakeKpiCard("Ventas del Mes", "$0", "Sin datos"), 0, 0);
                tlKpis.Controls.Add(MakeKpiCard("Órdenes", "0", "Sin datos"), 1, 0);
                tlKpis.Controls.Add(MakeKpiCard("Ticket Promedio", "$0", "Sin datos"), 2, 0);
                tlKpis.Controls.Add(MakeKpiCard("Cotizaciones", "0", "Sin datos"), 3, 0);
            }
        }

        private string FormatPorcentaje(decimal porcentaje)
        {
            var signo = porcentaje >= 0 ? "+" : "";
            return $"{signo}{porcentaje:F1}%";
        }

        private void AplicarFiltros()
        {
            try
            {
                var filtradas = _allVentas.AsEnumerable();

                // Filtro de búsqueda
                var buscar = txtBuscar?.Text?.Trim() ?? "";
                if (!string.IsNullOrEmpty(buscar) && buscar != "Buscar por número, cliente o producto...")
                {
                    filtradas = filtradas.Where(v =>
                        (!string.IsNullOrEmpty(v.NumeroVenta) && v.NumeroVenta.ToLowerInvariant().Contains(buscar.ToLowerInvariant())) ||
                        (!string.IsNullOrEmpty(v.NombreCliente) && v.NombreCliente.ToLowerInvariant().Contains(buscar.ToLowerInvariant())) ||
                        (!string.IsNullOrEmpty(v.EmpresaCliente) && v.EmpresaCliente.ToLowerInvariant().Contains(buscar.ToLowerInvariant())));
                }

                // Filtro de estado
                if (cbEstado?.SelectedIndex > 0)
                {
                    var estado = cbEstado.SelectedItem.ToString();
                    filtradas = filtradas.Where(v => v.Estado == estado);
                }

                // Filtro de tipo
                if (cbTipo?.SelectedIndex > 0)
                {
                    var tipo = cbTipo.SelectedItem.ToString();
                    filtradas = filtradas.Where(v => v.Tipo == tipo);
                }

                // Filtro de período
                if (cbPeriodo?.SelectedIndex > 0)
                {
                    DateTime fechaDesde;
                    switch (cbPeriodo.SelectedIndex)
                    {
                        case 1: // Este mes
                            fechaDesde = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                            filtradas = filtradas.Where(v => v.FechaVenta >= fechaDesde);
                            break;
                        case 2: // Mes anterior
                            var mesAnterior = DateTime.Now.AddMonths(-1);
                            fechaDesde = new DateTime(mesAnterior.Year, mesAnterior.Month, 1);
                            var fechaHasta = fechaDesde.AddMonths(1).AddDays(-1);
                            filtradas = filtradas.Where(v => v.FechaVenta >= fechaDesde && v.FechaVenta <= fechaHasta);
                            break;
                        case 3: // Últimos 90 días
                            fechaDesde = DateTime.Now.AddDays(-90);
                            filtradas = filtradas.Where(v => v.FechaVenta >= fechaDesde);
                            break;
                    }
                }

                // Filtro de vendedor (solo para admin)
                if (_currentUser.IdRol == 1 && cbVendedor?.SelectedIndex > 0)
                {
                    var vendedorNombre = cbVendedor.SelectedItem.ToString();
                    filtradas = filtradas.Where(v => v.NombreVendedor == vendedorNombre);
                }

                // Actualizar datos
                var ventasOrdenadas = filtradas.OrderByDescending(v => v.FechaVenta).ToList();
                
                _ventas.RaiseListChangedEvents = false;
                _ventas.Clear();
                
                foreach (var venta in ventasOrdenadas)
                {
                    _ventas.Add(venta);
                }
                
                _ventas.RaiseListChangedEvents = true;
                _ventas.ResetBindings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al aplicar filtros: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ================ Event Handlers ==================
        private void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (e.RowIndex >= _ventas.Count) return;

            try
            {
                var venta = _ventas[e.RowIndex];
                if (venta == null) return;

                // Formatear cliente
                if (dgv.Columns[e.ColumnIndex].Name == "Cliente")
                {
                    var cliente = venta.NombreCliente;
                    if (!string.IsNullOrEmpty(venta.EmpresaCliente))
                        cliente += $"\n{venta.EmpresaCliente}";
                    e.Value = cliente;
                    e.FormattingApplied = true;
                }

                // Formatear acciones
                if (dgv.Columns[e.ColumnIndex].Name == "Acciones")
                {
                    e.Value = "Ver";
                    e.FormattingApplied = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en CellFormatting: {ex.Message}");
            }
        }

        private void Dgv_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (e.RowIndex >= _ventas.Count) return;

            try
            {
                var columnName = dgv.Columns[e.ColumnIndex].Name;

                // Estado con chips
                if (columnName == "Estado")
                {
                    e.Handled = true;
                    e.PaintBackground(e.ClipBounds, true);
                    string text = Convert.ToString(e.FormattedValue ?? "");
                    Color bg;
                    switch (text)
                    {
                        case "Completada":
                            bg = Color.FromArgb(34, 139, 34);
                            break;
                        case "Pendiente":
                            bg = Color.FromArgb(255, 165, 0);
                            break;
                        case "Cancelada":
                            bg = Color.FromArgb(220, 53, 69);
                            break;
                        default:
                            bg = Color.Gray;
                            break;
                    }
                    DrawChip(e.Graphics, e.CellBounds, text, bg, Color.White, 8);
                }

                // Tipo con chips
                if (columnName == "Tipo")
                {
                    e.Handled = true;
                    e.PaintBackground(e.ClipBounds, true);
                    string text = Convert.ToString(e.FormattedValue ?? "");
                    Color bg;
                    switch (text)
                    {
                        case "Venta":
                            bg = Color.FromArgb(34, 139, 34);
                            break;
                        case "Cotización":
                            bg = Color.FromArgb(54, 162, 235);
                            break;
                        case "Devolución":
                            bg = Color.FromArgb(255, 99, 132);
                            break;
                        default:
                            bg = Color.Gray;
                            break;
                    }
                    DrawChip(e.Graphics, e.CellBounds, text, bg, Color.White, 8);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en CellPainting: {ex.Message}");
            }
        }

        private void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (e.RowIndex >= _ventas.Count) return;

            try
            {
                if (dgv.Columns[e.ColumnIndex].Name == "Acciones")
                {
                    var venta = _ventas[e.RowIndex];
                    if (venta != null)
                    {
                        VerDetalleVenta(venta);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al procesar la acción: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ================ Acciones de botones ==================
        private void BtnNuevaVenta_Click(object sender, EventArgs e)
        {
            try
            {
                using (var form = new Forms.NuevaVentaForm(_currentUser, "Venta"))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        RefrescarDatos();
                        MessageBox.Show("Venta creada exitosamente.", "Éxito", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear nueva venta: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnNuevaCotizacion_Click(object sender, EventArgs e)
        {
            try
            {
                using (var form = new Forms.NuevaVentaForm(_currentUser, "Cotización"))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        RefrescarDatos();
                        MessageBox.Show("Cotización creada exitosamente.", "Éxito", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear nueva cotización: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExportar_Click(object sender, EventArgs e)
        {
            try
            {
                if (_ventas.Count == 0)
                {
                    MessageBox.Show("No hay datos para exportar.", "Información", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "Archivo CSV (*.csv)|*.csv|Archivo Excel (*.xlsx)|*.xlsx";
                    saveDialog.Title = "Exportar Ventas";
                    saveDialog.FileName = $"Ventas_{DateTime.Now:yyyy-MM-dd}.csv";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        ExportarDatos(saveDialog.FileName, saveDialog.FilterIndex);
                        MessageBox.Show($"Datos exportados exitosamente a:\n{saveDialog.FileName}", "Exportación Completa", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportarDatos(string fileName, int formatType)
        {
            var sb = new System.Text.StringBuilder();

            // Encabezados
            sb.AppendLine("Número,Cliente,Empresa,Vendedor,Tipo,Estado,Total,Fecha,Observaciones");

            // Datos
            foreach (var venta in _ventas)
            {
                var linea = $"\"{venta.NumeroVenta}\"," +
                           $"\"{venta.NombreCliente}\"," +
                           $"\"{venta.EmpresaCliente}\"," +
                           $"\"{venta.NombreVendedor}\"," +
                           $"\"{venta.Tipo}\"," +
                           $"\"{venta.Estado}\"," +
                           $"\"{venta.Total:F2}\"," +
                           $"\"{venta.FechaVenta:yyyy-MM-dd}\"," +
                           $"\"{venta.Observaciones?.Replace("\"", "\"\"")}\"";
                sb.AppendLine(linea);
            }

            System.IO.File.WriteAllText(fileName, sb.ToString(), System.Text.Encoding.UTF8);
        }

        private void BtnGenerarReporte_Click(object sender, EventArgs e)
        {
            try
            {
                using (var form = new Forms.ReporteVentasForm(_allVentas, _vendedores))
                {
                    form.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar reporte: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAnalisisDetallado_Click(object sender, EventArgs e)
        {
            try
            {
                using (var form = new Forms.AnalisisVentasForm(_allVentas, _vendedores))
                {
                    form.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir análisis: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void VerDetalleVenta(Venta venta)
        {
            try
            {
                using (var form = new Forms.DetalleVentaForm(venta, _ventaRepo))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        // Si se modificó algo, refrescar
                        RefrescarDatos();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al ver detalle: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ================ Helpers UI ==================
        private Button MakeActionButton(string text)
        {
            var b = new Button
            {
                Text = text,
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                BackColor = ColAccent,
                ForeColor = Color.White,
                Margin = new Padding(8, 6, 0, 0),
                Height = 34,
                Padding = new Padding(12, 6, 12, 6)
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
                ForeColor = ColText,
                Margin = new Padding(8, 6, 0, 0),
                Height = 34,
                Padding = new Padding(12, 6, 12, 6)
            };
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = ColBorder;
            return b;
        }

        private Panel MakeKpiCard(string titulo, string valor, string sub)
        {
            var p = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColSoft,
                Margin = new Padding(8),
                Padding = new Padding(16)
            };

            var lblTitle = new Label
            {
                Text = titulo,
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(90, 100, 90),
                Location = new Point(0, 0)
            };
            
            var lblValor = new Label
            {
                Text = valor,
                AutoSize = true,
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = ColText,
                Location = new Point(0, 24)
            };
            
            var lblSub = new Label
            {
                Text = sub,
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(90, 130, 90),
                Location = new Point(0, 60)
            };

            p.Controls.Add(lblTitle);
            p.Controls.Add(lblValor);
            p.Controls.Add(lblSub);

            return p;
        }

        private ComboBox MakeCombo(string[] items)
        {
            var cb = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(2)
            };
            cb.Items.AddRange(items);
            cb.SelectedIndex = 0;
            return cb;
        }

        private void DrawChip(Graphics g, Rectangle cell, string text, Color bg, Color fg, int radius)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var font = new Font("Segoe UI", 8f, FontStyle.Bold);
            var sz = TextRenderer.MeasureText(text, font);
            int padX = 8, padY = 4;
            int w = Math.Min(cell.Width - 8, sz.Width + padX * 2);
            int h = Math.Min(cell.Height - 8, sz.Height + padY * 2);

            int x = cell.X + (cell.Width - w) / 2;
            int y = cell.Y + (cell.Height - h) / 2;

            using (var brush = new SolidBrush(bg))
            {
                g.FillRoundedRectangle(brush, x, y, w, h, radius);
            }

            var textRect = new Rectangle(x, y, w, h);
            TextRenderer.DrawText(g, text, font, textRect, fg,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        // Método auxiliar para RefreshData público
        public void RefreshData()
        {
            RefrescarDatos();
        }
    }
}

// Extension method para FillRoundedRectangle
public static class GraphicsExtensions
{
    public static void FillRoundedRectangle(this Graphics g, Brush brush, int x, int y, int width, int height, int radius)
    {
        using (var path = new System.Drawing.Drawing2D.GraphicsPath())
        {
            path.AddArc(x, y, radius * 2, radius * 2, 180, 90);
            path.AddArc(x + width - radius * 2, y, radius * 2, radius * 2, 270, 90);
            path.AddArc(x + width - radius * 2, y + height - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(x, y + height - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            g.FillPath(brush, path);
        }
    }
}
