using Proyecto_Taller_2.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading.Tasks;
using Proyecto_Taller_2.Domain.Entities;
using System.Text; // Para Exportar
using System.IO; // Para Importar/Exportar

namespace Proyecto_Taller_2
{
    public class UcClientes : UserControl
    {
        // ===== Placeholder (cue banner) para TextBox en .NET Framework =====
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;
        private static void SetPlaceholder(TextBox tb, string text) { SendMessage(tb.Handle, EM_SETCUEBANNER, 1, text); }

        // --- Conexión al Repositorio ---
        private readonly ClienteRepository _clienteRepository;

        // Paleta
        private readonly Color ColBg = Color.White;
        private readonly Color ColSoft = Color.FromArgb(246, 250, 246);
        private readonly Color ColSoftAlt = Color.FromArgb(236, 243, 236);
        private readonly Color ColText = Color.FromArgb(34, 47, 34);
        private readonly Color ColAccent = Color.FromArgb(34, 139, 34);
        private readonly Color ColBorder = Color.FromArgb(210, 220, 210);

        // UI
        private TableLayoutPanel tlRoot;
        private FlowLayoutPanel flTopLeft, flTopRight;
        private TextBox txtBuscar;
        private ComboBox cbSegmento, cbEstado;
        private Button btnNuevo, btnImportar, btnExportar;
        private GroupBox gbLista;
        private DataGridView dgv;
        private Panel pnlDetails;
        private Label lblFooter;
        private Button btnPrev, btnNext;
        private Button btnEditar; // Botón en panel de detalles
        private Button btnToggleActivo; // Botón en panel de detalles

        public UcClientes()
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.DoubleBuffered = true;
            this.BackColor = ColBg;
            this.Dock = DockStyle.Fill;
            _clienteRepository = new ClienteRepository();
            BuildUI();
            this.Load += UcClientes_Load; // Enganchar evento Load
        }

        // --- Lógica de Carga y Refresco ---
        private async void UcClientes_Load(object sender, EventArgs e)
        {
            await CargarDatosAsync(); // Llamada inicial al cargar el control
        }

        /// <summary>
        /// Contiene la lógica real para cargar y refrescar los datos de la grilla.
        /// Devuelve Task, por lo que SÍ se puede usar await en él.
        /// </summary>
        private async Task CargarDatosAsync(string filtroTexto = null, string filtroSegmento = null, string filtroEstado = null)
        {
            gbLista.Text = "Cargando clientes...";
            // Guardar la fila seleccionada (si existe)
            int? selectedId = null;
            if (dgv.SelectedRows.Count > 0 && dgv.SelectedRows[0].Tag is Cliente cli)
            {
                selectedId = cli.IdCliente;
            }

            dgv.Rows.Clear();
            RenderEmptyDetails();
            try
            {
                List<Cliente> clientesDeLaDB = await _clienteRepository.GetAllClientesCompletosAsync(filtroTexto, filtroSegmento, filtroEstado);
                int newSelectedIndex = -1; // Para restaurar selección

                foreach (var c in clientesDeLaDB)
                {
                    string nombre = c.Tipo == "PF" ? $"{c.NombreCliente} {c.ApellidoCliente}" : c.RazonSocial;
                    nombre = nombre ?? "Cliente";
                    string email = c.Emails.FirstOrDefault(em => em.EsPrincipal)?.Email ?? c.Emails.FirstOrDefault()?.Email ?? "N/A";
                    string contacto = c.Telefonos.FirstOrDefault(tel => tel.EsPrincipal)?.Telefono ?? c.Telefonos.FirstOrDefault()?.Telefono ?? "N/A";
                    string estado = c.Activo ? "Activo" : "Inactivo";
                    decimal total = c.Ventas.Sum(v => v.Total);
                    DateTime? ultima = c.Ventas.Any() ? c.Ventas.Max(v => v.FechaVenta) : (DateTime?)null;
                    string segmento = total > 15000 ? "VIP" : total > 2000 ? "Regular" : "Premium";
                    Image img = MakeAvatar(nombre);
                    string clienteStr = (nombre == "Cliente" && !string.IsNullOrWhiteSpace(c.RazonSocial) ? c.RazonSocial : nombre) + "\n" + email;
                    string totalStr = total.ToString("C");
                    string ultimaStr = ultima.HasValue ? ultima.Value.ToString("d/M/yyyy") : "N/A";
                    int rowIndex = dgv.Rows.Add(img, clienteStr, c.RazonSocial ?? "N/A", contacto, segmento, totalStr, ultimaStr, estado, "⋯");
                    dgv.Rows[rowIndex].Tag = c;

                    // Si este es el cliente que estaba seleccionado, guardar su nuevo índice
                    if (selectedId.HasValue && c.IdCliente == selectedId.Value)
                    {
                        newSelectedIndex = rowIndex;
                    }
                }
                gbLista.Text = "Lista de Clientes";
                lblFooter.Text = $"Mostrando {clientesDeLaDB.Count} de {clientesDeLaDB.Count}";

                // Restaurar selección si se encontró el cliente
                if (newSelectedIndex != -1 && newSelectedIndex < dgv.Rows.Count)
                {
                    dgv.ClearSelection(); // Limpiar primero
                    dgv.Rows[newSelectedIndex].Selected = true;
                    dgv.CurrentCell = dgv.Rows[newSelectedIndex].Cells[0]; // Opcional: enfocar la celda
                }
                else
                {
                    dgv.ClearSelection(); // Si no se encontró (o no había selección), limpiar
                }

            }
            catch (Exception ex)
            {
                gbLista.Text = "Error al cargar";
                MessageBox.Show($"No se pudieron cargar los clientes: {ex.Message}\n\nStackTrace:\n{ex.StackTrace}", "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dgv.ClearSelection(); // Limpiar selección en caso de error
            }
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 248);
            // dgv.ClearSelection(); // Se mueve dentro del try/catch
        }

        // --- Construcción de la Interfaz (BuildUI) ---
        private void BuildUI()
        {
            // ... (Código completo de BuildUI, igual que antes) ...
            var rootPad = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16) }; Controls.Add(rootPad);
            tlRoot = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = ColBg };
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 68)); tlRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); tlRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            rootPad.Controls.Add(tlRoot);
            var top = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(4, 6, 4, 6) };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60)); top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            tlRoot.Controls.Add(top, 0, 0);
            flTopLeft = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Margin = new Padding(0), Padding = new Padding(0, 6, 0, 6) };
            var txtWrap = new Panel { Width = 320, Height = 34, Padding = new Padding(8, 6, 8, 6), BackColor = ColSoft, Margin = new Padding(0, 0, 8, 0) };
            txtBuscar = new TextBox { BorderStyle = BorderStyle.None, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5f) }; SetPlaceholder(txtBuscar, "Buscar por nombre, email o empresa…"); txtWrap.Controls.Add(txtBuscar);
            cbSegmento = MakeCombo(new[] { "Todos los segmentos", "VIP", "Premium", "Regular" });
            cbEstado = MakeCombo(new[] { "Todos los estados", "Activo", "Inactivo" });
            flTopLeft.Controls.Add(txtWrap); flTopLeft.Controls.Add(cbSegmento); flTopLeft.Controls.Add(cbEstado);
            flTopRight = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Margin = new Padding(0), Padding = new Padding(0, 6, 0, 6) };
            btnNuevo = MakeAction("+ Nuevo Cliente");
            btnExportar = MakeGhost("Exportar");
            btnImportar = MakeGhost("Importar");
            btnNuevo.Click += BtnNuevo_Click;
            btnExportar.Click += BtnExportar_Click;
            btnImportar.Click += BtnImportar_Click;
            txtBuscar.TextChanged += Filtros_Changed;
            cbSegmento.SelectedIndexChanged += Filtros_Changed;
            cbEstado.SelectedIndexChanged += Filtros_Changed;
            flTopRight.Controls.Add(btnNuevo); flTopRight.Controls.Add(btnExportar); flTopRight.Controls.Add(btnImportar);
            top.Controls.Add(flTopLeft, 0, 0); top.Controls.Add(flTopRight, 1, 0);
            var splitPad = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 4, 0, 0) }; tlRoot.Controls.Add(splitPad, 0, 1);
            var split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 }; split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); split.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320)); splitPad.Controls.Add(split);
            var gridPad = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 12, 0) }; split.Controls.Add(gridPad, 0, 0);
            gbLista = new GroupBox { Text = "Lista de Clientes", Dock = DockStyle.Fill, Padding = new Padding(12), ForeColor = ColText }; gridPad.Controls.Add(gbLista);
            dgv = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false, RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, ColumnHeadersHeight = 42, ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing, GridColor = ColBorder, EnableHeadersVisualStyles = false };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = ColSoftAlt; dgv.ColumnHeadersDefaultCellStyle.ForeColor = ColText; dgv.DefaultCellStyle.Padding = new Padding(8, 10, 8, 10); dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True; dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 232, 220); dgv.DefaultCellStyle.SelectionForeColor = Color.Black; dgv.RowTemplate.Height = 64; dgv.AdvancedCellBorderStyle.Left = DataGridViewAdvancedCellBorderStyle.Single; dgv.AdvancedCellBorderStyle.Right = DataGridViewAdvancedCellBorderStyle.Single;
            dgv.CellPainting += Dgv_CellPainting; dgv.SelectionChanged += Dgv_SelectionChanged;
            var cAvatar = new DataGridViewImageColumn { Name = "Avatar", HeaderText = "", FillWeight = 56, ImageLayout = DataGridViewImageCellLayout.Zoom }; var cCliente = new DataGridViewTextBoxColumn { Name = "Cliente", HeaderText = "Cliente", FillWeight = 180, DefaultCellStyle = new DataGridViewCellStyle { WrapMode = DataGridViewTriState.True } }; var cEmpresa = new DataGridViewTextBoxColumn { Name = "Empresa", HeaderText = "Empresa", FillWeight = 140 }; var cContacto = new DataGridViewTextBoxColumn { Name = "Contacto", HeaderText = "Contacto", FillWeight = 70 }; var cSegmento = new DataGridViewTextBoxColumn { Name = "Segmento", HeaderText = "Segmento", FillWeight = 90 }; var cTotal = new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "Total Compras", FillWeight = 100 }; var cUltima = new DataGridViewTextBoxColumn { Name = "Ultima", HeaderText = "Última Compra", FillWeight = 100 }; var cEstado = new DataGridViewTextBoxColumn { Name = "Estado", HeaderText = "Estado", FillWeight = 90 }; var cAcciones = new DataGridViewTextBoxColumn { Name = "Acciones", HeaderText = "Acciones", FillWeight = 60 };
            dgv.Columns.AddRange(new DataGridViewColumn[] { cAvatar, cCliente, cEmpresa, cContacto, cSegmento, cTotal, cUltima, cEstado, cAcciones }); gbLista.Controls.Add(dgv);
            var detailsPad = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 8, 12, 8), BackColor = ColSoft }; split.Controls.Add(detailsPad, 1, 0);
            pnlDetails = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 16, 8, 8), BackColor = ColSoft }; detailsPad.Controls.Add(pnlDetails); RenderEmptyDetails();
            var footer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 6, 8, 6), BackColor = ColBg }; tlRoot.Controls.Add(footer, 0, 2);
            var flFoot = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true };
            lblFooter = new Label { AutoSize = true, Text = "Mostrando 0 de 0", ForeColor = ColText, Font = new Font("Segoe UI", 9f), Margin = new Padding(94, 0, 8, 0) };
            btnPrev = MakeGhost("Anterior"); btnPrev.Enabled = false; btnPrev.Margin = new Padding(0, 0, 8, 0); btnNext = MakeGhost("Siguiente"); btnNext.Enabled = false;
            flFoot.Controls.Add(lblFooter); flFoot.Controls.Add(btnPrev); flFoot.Controls.Add(btnNext);
            Action centrarFooter = () => { if (footer.ClientRectangle.Width > 0 && flFoot.PreferredSize.Width > 0) flFoot.Location = new Point((footer.ClientSize.Width - flFoot.PreferredSize.Width) / 2, (footer.ClientSize.Height - flFoot.PreferredSize.Height) / 2); };
            footer.Resize += (s, e) => centrarFooter(); this.Load += (s, e) => { centrarFooter(); };
            footer.Controls.Add(flFoot);
        }

        // --- MÉTODOS DE AYUDA (MakeCombo, MakeAction, MakeGhost, MakeAvatar, etc.) ---
        // ... (Código COMPLETO de todos tus métodos de ayuda, SIN cambios) ...
        private ComboBox MakeCombo(string[] items) { var cb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Height = 34, Width = 160, Margin = new Padding(0, 0, 8, 0) }; cb.Items.AddRange(items); cb.SelectedIndex = 0; return cb; }
        private Button MakeAction(string text) { var b = new Button { Text = text, AutoSize = true, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = ColAccent, ForeColor = Color.White, Margin = new Padding(8, 0, 0, 0), Padding = new Padding(12, 6, 12, 6) }; b.FlatAppearance.BorderSize = 0; return b; }
        private Button MakeGhost(string text) { var b = new Button { Text = text, AutoSize = true, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = ColText, Margin = new Padding(8, 0, 0, 0), Padding = new Padding(12, 6, 12, 6) }; b.FlatAppearance.BorderSize = 1; b.FlatAppearance.BorderColor = ColBorder; return b; }
        private Image MakeAvatar(string nombre) { string initials = GetInitials(nombre); int size = 44; var bmp = new Bitmap(size, size); using (var g = Graphics.FromImage(bmp)) { g.SmoothingMode = SmoothingMode.AntiAlias; var bg = RandomPastel(nombre); using (var brush = new SolidBrush(bg)) g.FillEllipse(brush, 0, 0, size - 1, size - 1); using (var p = new Pen(Color.FromArgb(220, 220, 220))) g.DrawEllipse(p, 0, 0, size - 1, size - 1); using (var f = new Font("Segoe UI", 11f, FontStyle.Bold, GraphicsUnit.Point)) using (var sb = new SolidBrush(Color.White)) { var sz = g.MeasureString(initials, f); g.DrawString(initials, f, sb, (size - sz.Width) / 2f, (size - sz.Height) / 2f - 1); } } return bmp; }
        private static string GetInitials(string full) { if (string.IsNullOrWhiteSpace(full)) return "?"; var parts = full.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries); if (parts.Length == 1) return parts[0].Length > 0 ? parts[0].Substring(0, 1).ToUpper() : "?"; if (parts.Length == 0) return "?"; return (parts[0].Length > 0 ? parts[0].Substring(0, 1) : "") + (parts.Length > 1 && parts[parts.Length - 1].Length > 0 ? parts[parts.Length - 1].Substring(0, 1) : "").ToUpper(); }
        private static Color RandomPastel(string seed) { if (string.IsNullOrWhiteSpace(seed)) { seed = "Cliente"; } int h = seed.GetHashCode(); var rnd = new Random(h); int r = (rnd.Next(120, 200) + 40) / 2; int g = (rnd.Next(120, 200) + 40) / 2; int b = (rnd.Next(120, 200) + 40) / 2; return Color.FromArgb(255, r, g, b); }
        private void Dgv_CellPainting(object sender, DataGridViewCellPaintingEventArgs e) { if (e.RowIndex < 0) return; if (dgv.Columns[e.ColumnIndex].Name == "Segmento") { e.Handled = true; e.PaintBackground(e.ClipBounds, true); string text = Convert.ToString(e.FormattedValue ?? ""); var c = ChipColors(text); DrawChip(e.Graphics, e.CellBounds, text, c.Item1, c.Item2, 10); return; } if (dgv.Columns[e.ColumnIndex].Name == "Estado") { e.Handled = true; e.PaintBackground(e.ClipBounds, true); string text = Convert.ToString(e.FormattedValue ?? ""); Color bg = text.Equals("Activo", StringComparison.OrdinalIgnoreCase) ? Color.FromArgb(34, 139, 34) : Color.FromArgb(200, 180, 80); DrawChip(e.Graphics, e.CellBounds, text, bg, Color.White, 12); return; } if (dgv.Columns[e.ColumnIndex].Name == "Acciones") { e.Handled = true; e.PaintBackground(e.ClipBounds, true); TextRenderer.DrawText(e.Graphics, "⋯", new Font("Segoe UI", 12f, FontStyle.Bold), e.CellBounds, ColText, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter); return; } if (dgv.Columns[e.ColumnIndex].Name == "Cliente") { e.Handled = true; e.PaintBackground(e.ClipBounds, true); var rect = new Rectangle(e.CellBounds.X + 2, e.CellBounds.Y + 6, e.CellBounds.Width - 4, e.CellBounds.Height - 12); TextRenderer.DrawText(e.Graphics, Convert.ToString(e.FormattedValue ?? ""), new Font("Segoe UI", 9f), rect, ColText, TextFormatFlags.WordBreak | TextFormatFlags.NoPadding); return; } }
        private void DrawChip(Graphics g, Rectangle cell, string text, Color bg, Color fg, int radius) { g.SmoothingMode = SmoothingMode.AntiAlias; var font = new Font("Segoe UI", 9f, FontStyle.Bold); var sz = TextRenderer.MeasureText(text, font); int padX = 12, padY = 6; int w = Math.Min(cell.Width - 12, sz.Width + padX * 2); int h = Math.Min(cell.Height - 12, sz.Height - 4 + padY * 2); int x = cell.X + (cell.Width - w) / 2; int y = cell.Y + (cell.Height - h) / 2; using (GraphicsPath path = RoundedRect(new Rectangle(x, y, w, h), radius)) using (SolidBrush sb = new SolidBrush(bg)) { g.FillPath(sb, path); } var textRect = new Rectangle(x, y, w, h); TextRenderer.DrawText(g, text, font, textRect, fg, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter); }
        private static GraphicsPath RoundedRect(Rectangle bounds, int radius) { int d = radius * 2; var path = new GraphicsPath(); if (radius <= 0 || d <= 0 || bounds.Width <= d || bounds.Height <= d) { path.AddRectangle(bounds); return path; } path.AddArc(bounds.X, bounds.Y, d, d, 180, 90); path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90); path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90); path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90); path.CloseFigure(); return path; }
        private Tuple<Color, Color> ChipColors(string segmento) { if (segmento.Equals("VIP", StringComparison.OrdinalIgnoreCase)) return new Tuple<Color, Color>(Color.FromArgb(34, 139, 34), Color.White); if (segmento.Equals("Premium", StringComparison.OrdinalIgnoreCase)) return new Tuple<Color, Color>(Color.FromArgb(200, 190, 80), Color.White); return new Tuple<Color, Color>(Color.FromArgb(220, 224, 220), ColText); }


        // --- FUNCIONES DE BOTONES Y EVENTOS ---

        private void RenderEmptyDetails()
        {
            pnlDetails.Controls.Clear();
            var lbl = new Label { Text = "Seleccioná un cliente para ver detalles", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = ColText, Font = new Font("Segoe UI", 10f, FontStyle.Italic) };
            pnlDetails.Controls.Add(lbl);
        }

        private void Dgv_SelectionChanged(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { RenderEmptyDetails(); return; }
            var r = dgv.SelectedRows[0];
            Cliente clienteSeleccionado = r.Tag as Cliente;
            if (clienteSeleccionado == null) { RenderEmptyDetails(); return; }

            var img = r.Cells["Avatar"].Value as Image;
            string nombreEmail = Convert.ToString(r.Cells["Cliente"].Value ?? "");
            string empresa = Convert.ToString(r.Cells["Empresa"].Value ?? "");
            string segmento = Convert.ToString(r.Cells["Segmento"].Value ?? "");
            string total = Convert.ToString(r.Cells["Total"].Value ?? "");
            string ultima = Convert.ToString(r.Cells["Ultima"].Value ?? "");
            string estado = Convert.ToString(r.Cells["Estado"].Value ?? "");

            pnlDetails.SuspendLayout(); pnlDetails.Controls.Clear();
            var topWrap = new Panel { Dock = DockStyle.Top, Height = 120, Padding = new Padding(8, 8, 8, 8) };
            var pic = new PictureBox { Image = img, SizeMode = PictureBoxSizeMode.Zoom, Width = 72, Height = 72, Location = new Point(8, 8) };
            var lblNombre = new Label { Text = nombreEmail.Split('\n')[0], AutoSize = true, Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = ColText, Location = new Point(88, 10) };
            var lblEmail = new Label { Text = nombreEmail.Contains("\n") ? nombreEmail.Split('\n')[1] : "", AutoSize = true, ForeColor = Color.FromArgb(90, 100, 90), Location = new Point(88, 36) };
            string textoEstado = $"Estado: {estado}"; if (!string.IsNullOrEmpty(ultima) && ultima != "N/A") textoEstado += $", Últ. Compra: {ultima}";
            var lblEstado = new Label { Text = textoEstado, AutoSize = true, ForeColor = ColText, Location = new Point(8, 90) };
            topWrap.Controls.Add(lblEstado); topWrap.Controls.Add(lblEmail); topWrap.Controls.Add(lblNombre); topWrap.Controls.Add(pic);
            var flBtns = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(8, 0, 8, 0) };
            btnEditar = MakeAction("✏️ Editar"); btnEditar.Click += BtnEditar_Click;
            string textoToggle = clienteSeleccionado.Activo ? "❌ Dar de Baja" : "✔️ Dar de Alta";
            btnToggleActivo = MakeGhost(textoToggle); btnToggleActivo.Click += BtnToggleActivo_Click;
            flBtns.Controls.Add(btnEditar); flBtns.Controls.Add(btnToggleActivo);
            var info = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 8, 8, 8) };
            info.Controls.Add(new Label { Text = "Empresa: " + (empresa ?? "N/A"), AutoSize = true, ForeColor = ColText, Location = new Point(8, 8) }); // Manejar null
            info.Controls.Add(new Label { Text = "Segmento: " + (segmento ?? "N/A"), AutoSize = true, ForeColor = ColText, Location = new Point(8, 30) }); // Manejar null
            info.Controls.Add(new Label { Text = "Total compras: " + (total ?? "$0.00"), AutoSize = true, ForeColor = ColText, Location = new Point(8, 52) }); // Manejar null
            pnlDetails.Controls.Add(info); pnlDetails.Controls.Add(flBtns); pnlDetails.Controls.Add(topWrap);
            pnlDetails.ResumeLayout();
        }

        private async void BtnEditar_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) return;
            var cliente = dgv.SelectedRows[0].Tag as Cliente;
            if (cliente == null) return;
            // Asegúrate de que FormEditarCliente exista y acepte Cliente en constructor
            using (FormEditarCliente form = new FormEditarCliente(cliente))
            {
                if (form.ShowDialog() == DialogResult.OK) { await CargarDatosAsync(); }
            }
        }

        private async void BtnToggleActivo_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) return;
            var cliente = dgv.SelectedRows[0].Tag as Cliente;
            if (cliente == null) return;
            string accion = cliente.Activo ? "dar de baja" : "dar de alta";
            string nombreCliente = cliente.Tipo == "PF" ? $"{cliente.NombreCliente} {cliente.ApellidoCliente}" : cliente.RazonSocial;
            if (string.IsNullOrWhiteSpace(nombreCliente)) nombreCliente = $"Cliente ID {cliente.IdCliente}";
            if (MessageBox.Show($"¿Estás seguro de que deseas {accion} a {nombreCliente}?", "Confirmar Acción", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try { await _clienteRepository.SetActivoAsync(cliente.IdCliente, !cliente.Activo); await CargarDatosAsync(); }
                catch (Exception ex) { MessageBox.Show($"Error al cambiar el estado: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private async void BtnExportar_Click(object sender, EventArgs e)
        {
            if (dgv.Rows.Count == 0) { MessageBox.Show("No hay datos para exportar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "Archivos CSV (*.csv)|*.csv", Title = "Exportar Clientes" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("IdCliente;Tipo;Nombre;Apellido;RazonSocial;CUIL;CUIT;Direccion;Estado;EmailPrincipal;TelefonoPrincipal");
                        List<Cliente> clientes = await _clienteRepository.GetAllClientesCompletosAsync();
                        foreach (var c in clientes)
                        {
                            string nombre = (c.NombreCliente ?? "").Replace(";", ","); string apellido = (c.ApellidoCliente ?? "").Replace(";", ","); string razon = (c.RazonSocial ?? "").Replace(";", ","); string cuil = c.CUIL ?? ""; string cuit = c.CUIT ?? ""; string dir = (c.Direccion ?? "").Replace(";", ","); string estado = c.Activo ? "Activo" : "Inactivo"; string email = (c.Emails.FirstOrDefault(em => em.EsPrincipal)?.Email ?? c.Emails.FirstOrDefault()?.Email ?? "").Replace(";", ","); string tel = (c.Telefonos.FirstOrDefault(t => t.EsPrincipal)?.Telefono ?? c.Telefonos.FirstOrDefault()?.Telefono ?? "").Replace(";", ",");
                            sb.AppendLine($"{c.IdCliente};{c.Tipo};{nombre};{apellido};{razon};{cuil};{cuit};{dir};{estado};{email};{tel}");
                        }
                        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                        MessageBox.Show("Datos exportados con éxito.", "Exportación Completa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex) { MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                }
            }
        }

        private async void BtnImportar_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "Archivos CSV (*.csv)|*.csv", Title = "Importar Clientes" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var lineas = File.ReadAllLines(ofd.FileName).Skip(1); int importados = 0; int errores = 0; var sbErrores = new StringBuilder();
                        foreach (var linea in lineas)
                        {
                            if (string.IsNullOrWhiteSpace(linea)) continue; var datos = linea.Split(';');
                            if (datos.Length < 11) { sbErrores.AppendLine($"Línea omitida (formato incorrecto): {linea.Substring(0, Math.Min(linea.Length, 30))}..."); errores++; continue; }
                            try
                            {
                                string tipo = datos.Length > 1 ? datos[1]?.Trim() : ""; string nombre = datos.Length > 2 ? datos[2]?.Trim() : ""; string apellido = datos.Length > 3 ? datos[3]?.Trim() : ""; string razonSocial = datos.Length > 4 ? datos[4]?.Trim() : ""; string cuil = datos.Length > 5 ? datos[5]?.Trim() : ""; string cuit = datos.Length > 6 ? datos[6]?.Trim() : ""; string direccion = datos.Length > 7 ? datos[7]?.Trim() : ""; string estadoStr = datos.Length > 8 ? datos[8]?.Trim() : "Activo"; string email = datos.Length > 9 ? datos[9]?.Trim() : ""; string telefono = datos.Length > 10 ? datos[10]?.Trim() : "";
                                if (string.IsNullOrWhiteSpace(tipo) || (tipo != "PF" && tipo != "PJ")) { sbErrores.AppendLine($"Línea omitida (Tipo inválido '{tipo}'): {linea.Substring(0, Math.Min(linea.Length, 30))}..."); errores++; continue; }
                                if (string.IsNullOrWhiteSpace(email)) { sbErrores.AppendLine($"Línea omitida (Email obligatorio): {linea.Substring(0, Math.Min(linea.Length, 30))}..."); errores++; continue; }
                                if (await _clienteRepository.CheckDniExistsAsync(cuil, cuit, 0)) { sbErrores.AppendLine($"Línea omitida (DNI/CUIT ya existe): {linea.Substring(0, Math.Min(linea.Length, 30))}..."); errores++; continue; }
                                if (await _clienteRepository.CheckEmailExistsAsync(email, 0)) { sbErrores.AppendLine($"Línea omitida (Email ya existe): {linea.Substring(0, Math.Min(linea.Length, 30))}..."); errores++; continue; }
                                var nuevoCliente = new Cliente { Tipo = tipo, NombreCliente = tipo == "PF" ? nombre : null, ApellidoCliente = tipo == "PF" ? apellido : null, RazonSocial = tipo == "PJ" ? razonSocial : null, CUIL = tipo == "PF" ? cuil : null, CUIT = tipo == "PJ" ? cuit : null, Direccion = direccion, Activo = estadoStr.Equals("Activo", StringComparison.OrdinalIgnoreCase) };
                                await _clienteRepository.InsertClienteAsync(nuevoCliente, email, telefono); importados++;
                            }
                            catch (FormatException fx) { sbErrores.AppendLine($"Error de formato en línea '{linea.Substring(0, Math.Min(linea.Length, 30))}...': {fx.Message}"); errores++; }
                            catch (Exception exLinea) { sbErrores.AppendLine($"Error en línea '{linea.Substring(0, Math.Min(linea.Length, 30))}...': {exLinea.Message}"); errores++; }
                        }
                        string reporte = $"Importación completada.\n\nClientes nuevos: {importados}\nLíneas con errores: {errores}\n\nDetalles:\n{(sbErrores.Length > 0 ? sbErrores.ToString() : "Ninguno")}";
                        MessageBox.Show(reporte, "Importación", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await CargarDatosAsync();
                    }
                    catch (Exception ex) { MessageBox.Show($"Error fatal al leer el archivo: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                }
            }
        }

        // --- CORREGIDO BtnNuevo_Click ---
        private async void BtnNuevo_Click(object sender, EventArgs e)
        {
            using (var form = new Form() { Text = "Nuevo Cliente", StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false, Width = 400, Height = 350 })
            {
                // --- Layout del Formulario Dinámico ---
                TableLayoutPanel root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), RowCount = 2 }; root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); form.Controls.Add(root);
                TableLayoutPanel grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoSize = true, Padding = new Padding(5) }; grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100)); grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); root.Controls.Add(grid, 0, 0);
                // --- Controles ---
                var cmbTipo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill }; cmbTipo.Items.AddRange(new object[] { "PF", "PJ" }); cmbTipo.SelectedIndex = 0;
                var txtNombre = new TextBox { Dock = DockStyle.Fill }; var txtApellido = new TextBox { Dock = DockStyle.Fill }; var txtRazonSocial = new TextBox { Dock = DockStyle.Fill, Enabled = false }; var txtDireccion = new TextBox { Dock = DockStyle.Fill }; var txtCUIL = new TextBox { Dock = DockStyle.Fill }; var txtCUIT = new TextBox { Dock = DockStyle.Fill, Enabled = false }; var txtEmail = new TextBox { Dock = DockStyle.Fill }; var txtTelefono = new TextBox { Dock = DockStyle.Fill };
                // --- Añadir Controles al Grid ---
                int row = 0; grid.Controls.Add(new Label { Text = "Tipo:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, row); grid.Controls.Add(cmbTipo, 1, row++); grid.Controls.Add(new Label { Text = "Nombre:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, row); grid.Controls.Add(txtNombre, 1, row++); grid.Controls.Add(new Label { Text = "Apellido:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, row); grid.Controls.Add(txtApellido, 1, row++); grid.Controls.Add(new Label { Text = "Razón Social:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, row); grid.Controls.Add(txtRazonSocial, 1, row++); grid.Controls.Add(new Label { Text = "Dirección:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, row); grid.Controls.Add(txtDireccion, 1, row++); grid.Controls.Add(new Label { Text = "CUIL:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, row); grid.Controls.Add(txtCUIL, 1, row++); grid.Controls.Add(new Label { Text = "CUIT:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, row); grid.Controls.Add(txtCUIT, 1, row++); grid.Controls.Add(new Label { Text = "Email:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, row); grid.Controls.Add(txtEmail, 1, row++); grid.Controls.Add(new Label { Text = "Teléfono:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, row); grid.Controls.Add(txtTelefono, 1, row++);
                // --- Lógica Habilitar/Deshabilitar ---
                Action actualizarCampos = () => { bool esPF = cmbTipo.SelectedItem.ToString() == "PF"; txtNombre.Enabled = esPF; txtApellido.Enabled = esPF; txtCUIL.Enabled = esPF; txtRazonSocial.Enabled = !esPF; txtCUIT.Enabled = !esPF; if (!esPF) { txtNombre.Clear(); txtApellido.Clear(); txtCUIL.Clear(); } else { txtRazonSocial.Clear(); txtCUIT.Clear(); } }; cmbTipo.SelectedIndexChanged += (s, args) => actualizarCampos(); actualizarCampos();
                // --- Botones ---
                FlowLayoutPanel panelBtns = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill }; Button btnOk = new Button { Text = "Guardar", DialogResult = DialogResult.None, AutoSize = true }; Button btnCancel = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, AutoSize = true }; panelBtns.Controls.Add(btnOk); panelBtns.Controls.Add(btnCancel); root.Controls.Add(panelBtns, 0, 1); form.AcceptButton = btnOk; form.CancelButton = btnCancel;

                // --- Lógica de Guardado (Click en OK - CORREGIDA) ---
                btnOk.Click += async (s, args) =>
                {
                    // No cambiamos DialogResult aquí todavía

                    string tipo = cmbTipo.SelectedItem.ToString(); string nombre = txtNombre.Text.Trim(); string apellido = txtApellido.Text.Trim(); string razonSocial = txtRazonSocial.Text.Trim(); string direccion = txtDireccion.Text.Trim(); string cuil = txtCUIL.Text.Trim(); string cuit = txtCUIT.Text.Trim(); string email = txtEmail.Text.Trim(); string telefono = txtTelefono.Text.Trim();

                    // Validación básica (si falla, simplemente no hacemos nada y el form sigue abierto)
                    if (tipo == "PF" && (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido))) { MessageBox.Show("Nombre y Apellido obligatorios para PF."); return; }
                    if (tipo == "PJ" && string.IsNullOrWhiteSpace(razonSocial)) { MessageBox.Show("Razón Social obligatoria para PJ."); return; }
                    if (string.IsNullOrWhiteSpace(email)) { MessageBox.Show("Email obligatorio."); return; }

                    try
                    {
                        // Validación duplicados BD (si falla, no hacemos nada)
                        if (!string.IsNullOrWhiteSpace(cuil) || !string.IsNullOrWhiteSpace(cuit)) { if (await _clienteRepository.CheckDniExistsAsync(cuil, cuit, 0)) { MessageBox.Show("CUIL/CUIT ya existe."); return; } }
                        if (await _clienteRepository.CheckEmailExistsAsync(email, 0)) { MessageBox.Show("Email ya existe."); return; }

                        // Crear objeto y guardar
                        var nuevoCliente = new Cliente { Tipo = tipo, NombreCliente = tipo == "PF" ? nombre : null, ApellidoCliente = tipo == "PF" ? apellido : null, RazonSocial = tipo == "PJ" ? razonSocial : null, Direccion = direccion, CUIL = tipo == "PF" ? cuil : null, CUIT = tipo == "PJ" ? cuit : null, Activo = true };
                        await _clienteRepository.InsertClienteAsync(nuevoCliente, email, telefono);

                        // Si llegamos aquí, el guardado fue exitoso
                        form.DialogResult = DialogResult.OK; // Establecer resultado OK
                        form.Close(); // Cerrar el formulario explícitamente

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al guardar: {ex.Message}", "Error");
                        // No cerramos si hay error
                    }
                };

                // --- Mostrar el formulario ---
                var result = form.ShowDialog(); // Espera hasta que se cierre (con OK o Cancel)

                // Si se guardó OK
                if (result == DialogResult.OK)
                {
                    await CargarDatosAsync(); // Refrescar la grilla principal
                }
            }
        }


        /// <summary>
        /// Manejador para cambios en los filtros (texto o combos).
        /// </summary>
        private async void Filtros_Changed(object sender, EventArgs e)
        {
            string texto = txtBuscar.Text;
            string segmento = cbSegmento.SelectedItem?.ToString();
            string estado = cbEstado.SelectedItem?.ToString();
            await CargarDatosAsync(texto, segmento, estado);
        }
    }
}