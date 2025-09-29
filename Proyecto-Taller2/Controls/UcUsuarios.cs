using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Proyecto_Taller_2.Domain.Models;
using Proyecto_Taller_2.Data.Repositories;


namespace Proyecto_Taller_2
{
    public class UcUsuarios : UserControl
    {
        // ===== Placeholder (cue banner) para TextBox en .NET Framework =====
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;
        private static void SetPlaceholder(TextBox tb, string text) { SendMessage(tb.Handle, EM_SETCUEBANNER, 1, text); }

        // ===== Data / Repo =====
        private readonly UsuarioRepository _repo = new UsuarioRepository();
        private readonly BindingList<Usuario> _datos = new BindingList<Usuario>();   // lo que ve la grilla
        private List<Usuario> _all = new List<Usuario>();                            // dataset maestro (para filtrar)
        private readonly Dictionary<int, Image> _avatarCache = new Dictionary<int, Image>(); // Dni -> avatar

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
        private ComboBox cbEstado;
        private Button btnNuevo, btnImportar, btnExportar;

        private GroupBox gbLista;
        private DataGridView dgv;

        private Panel pnlDetails;
        private Label lblFooter;
        private Button btnPrev, btnNext;

        // Binding source
        private readonly BindingSource _bs = new BindingSource();

        public UcUsuarios()
        {
            AutoScaleMode = AutoScaleMode.Dpi;
            DoubleBuffered = true;
            BackColor = ColBg;
            Dock = DockStyle.Fill;

            BuildUI();
            WireEvents();

            // Binding
            _bs.DataSource = _datos;
            dgv.AutoGenerateColumns = false;
            dgv.DataSource = _bs;

            // Cargar al mostrar el control
            if (!DesignMode)
            {
                Load += delegate { RefrescarDesdeBD(); };
            }
        }

        // =========================
        // Construcción UI
        // =========================
        private void BuildUI()
        {
            // ===== ROOT =====
            Panel rootPad = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16) };
            Controls.Add(rootPad);

            tlRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = ColBg
            };
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));  // Top bar
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Grid + details
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));  // Footer
            rootPad.Controls.Add(tlRoot);

            // ===== TOP BAR =====
            TableLayoutPanel top = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(4, 6, 4, 6)
            };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            tlRoot.Controls.Add(top, 0, 0);

            // Izquierda: buscar + filtros
            flTopLeft = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0),
                Padding = new Padding(0, 6, 0, 6)
            };
            Panel txtWrap = new Panel
            {
                Width = 320,
                Height = 34,
                Padding = new Padding(8, 6, 8, 6),
                BackColor = ColSoft,
                Margin = new Padding(0, 0, 8, 0)
            };
            txtBuscar = new TextBox { BorderStyle = BorderStyle.None, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5f) };
            SetPlaceholder(txtBuscar, "Buscar por nombre o email…");
            txtWrap.Controls.Add(txtBuscar);

            cbEstado = MakeCombo(new string[] { "Todos", "Activo", "Inactivo" });

            flTopLeft.Controls.Add(txtWrap);
            flTopLeft.Controls.Add(cbEstado);

            // Derecha: acciones
            flTopRight = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Margin = new Padding(0),
                Padding = new Padding(0, 6, 0, 6)
            };
            btnNuevo = MakeAction("+ Nuevo Usuario");
            btnExportar = MakeGhost("Exportar");
            btnImportar = MakeGhost("Importar");
            flTopRight.Controls.Add(btnNuevo);
            flTopRight.Controls.Add(btnExportar);
            flTopRight.Controls.Add(btnImportar);

            top.Controls.Add(flTopLeft, 0, 0);
            top.Controls.Add(flTopRight, 1, 0);

            // ===== LISTA + DETAILS =====
            Panel splitPad = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 4, 0, 0) };
            tlRoot.Controls.Add(splitPad, 0, 1);

            TableLayoutPanel split = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2
            };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320));
            splitPad.Controls.Add(split);

            // ---- GRID ----
            Panel gridPad = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 12, 0) };
            split.Controls.Add(gridPad, 0, 0);

            gbLista = new GroupBox
            {
                Text = "Lista de Usuarios",
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                ForeColor = ColText
            };
            gridPad.Controls.Add(gbLista);

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
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                ColumnHeadersHeight = 42,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                GridColor = ColBorder,
                EnableHeadersVisualStyles = false
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = ColSoftAlt;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = ColText;

            dgv.DefaultCellStyle.Padding = new Padding(8, 10, 8, 10);
            dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 232, 220);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.RowTemplate.Height = 64;
            dgv.AdvancedCellBorderStyle.Left = DataGridViewAdvancedCellBorderStyle.Single;
            dgv.AdvancedCellBorderStyle.Right = DataGridViewAdvancedCellBorderStyle.Single;

            // === Columnas (data-bound) ===
            DataGridViewTextBoxColumn cId = new DataGridViewTextBoxColumn { Name = "Id", DataPropertyName = "IdUsuario", HeaderText = "Id", Visible = true, FillWeight = 40 };
            DataGridViewTextBoxColumn cDni = new DataGridViewTextBoxColumn { Name = "Dni", DataPropertyName = "Dni", HeaderText = "DNI", Visible = true, FillWeight = 60 };
            DataGridViewImageColumn cAvatar = new DataGridViewImageColumn { Name = "Avatar", HeaderText = "", FillWeight = 56, ImageLayout = DataGridViewImageCellLayout.Zoom };
            DataGridViewTextBoxColumn cUsuario = new DataGridViewTextBoxColumn
            {
                Name = "Usuario",
                HeaderText = "Usuario",
                FillWeight = 180,
                DataPropertyName = "NombreCompleto",
                DefaultCellStyle = new DataGridViewCellStyle { WrapMode = DataGridViewTriState.True }
            };
            DataGridViewTextBoxColumn cEmail = new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "Email", FillWeight = 160, DataPropertyName = "Email" };
            DataGridViewTextBoxColumn cRol = new DataGridViewTextBoxColumn { Name = "Rol", HeaderText = "Rol", FillWeight = 90, DataPropertyName = "RolNombre" };
            DataGridViewTextBoxColumn cTelefono = new DataGridViewTextBoxColumn { Name = "Telefono", HeaderText = "Teléfono", FillWeight = 90, DataPropertyName = "Telefono" };
            DataGridViewTextBoxColumn cEstado = new DataGridViewTextBoxColumn { Name = "Estado", HeaderText = "Estado", FillWeight = 80, DataPropertyName = "Estado" }; // bool → chip con CellFormatting
            DataGridViewTextBoxColumn cAcciones = new DataGridViewTextBoxColumn { Name = "Acciones", HeaderText = "Acciones", FillWeight = 60 };

            dgv.Columns.AddRange(new DataGridViewColumn[] { cId, cDni, cAvatar, cUsuario, cEmail, cRol, cTelefono, cEstado });

            // Hooks
            dgv.CellPainting += Dgv_CellPainting;
            dgv.CellFormatting += Dgv_CellFormatting;
            dgv.DataBindingComplete += Dgv_DataBindingComplete;
            dgv.SelectionChanged += Dgv_SelectionChanged;

            gbLista.Controls.Add(dgv);

            // ---- DETAILS ----
            Panel detailsPad = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 8, 12, 8), BackColor = ColSoft };
            split.Controls.Add(detailsPad, 1, 0);
            pnlDetails = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 16, 8, 8), BackColor = ColSoft };
            detailsPad.Controls.Add(pnlDetails);
            RenderEmptyDetails();

            // ===== FOOTER =====
            Panel footer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 6, 8, 6), BackColor = ColBg };
            tlRoot.Controls.Add(footer, 0, 2);

            lblFooter = new Label { AutoSize = true, Text = "Mostrando 0 de 0", ForeColor = ColText, Font = new Font("Segoe UI", 9f) };
            btnPrev = MakeGhost("Anterior"); btnPrev.Enabled = false;
            btnNext = MakeGhost("Siguiente"); btnNext.Enabled = false;

            FlowLayoutPanel flFoot = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(0) };
            flFoot.Controls.Add(lblFooter);
            flFoot.Controls.Add(btnPrev);
            flFoot.Controls.Add(btnNext);
            footer.Controls.Add(flFoot);
        }

        private void WireEvents()
        {
            txtBuscar.TextChanged += delegate { AplicarFiltro(); };
            cbEstado.SelectedIndexChanged += delegate { AplicarFiltro(); };

            btnNuevo.Click += delegate
            {
                using (NuevoUsuarioForm f = new NuevoUsuarioForm(_repo))
                {
                    if (f.ShowDialog(FindForm()) == DialogResult.OK && f.Resultado != null)
                    {
                        try
                        {
                            _repo.AgregarUsuario(f.Resultado);
                            RefrescarDesdeBD();
                            MessageBox.Show("Usuario agregado con éxito.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("No se pudo agregar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            };

            dgv.CellContentClick += Dgv_CellContentClick;
        }

        private void Dgv_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgv.Columns[e.ColumnIndex].Name == "Acciones")
            {
                Usuario u = dgv.Rows[e.RowIndex].DataBoundItem as Usuario;
                if (u == null) return;
                var result = MessageBox.Show($"¿Desea cambiar el estado del usuario {u.NombreCompleto}?\n\nActualmente: {(u.Estado ? "Activo" : "Inactivo")}\"",
                    "Cambiar estado", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    u.Estado = !u.Estado;
                    _repo.ActualizarEstadoUsuario(u.Dni, u.Estado);
                    RefrescarDesdeBD();
                }
            }
        }

        // =========================
        // Carga y filtro
        // =========================
        private void RefrescarDesdeBD()
        {
            _avatarCache.Clear();
            _all = _repo.ObtenerUsuarios();  // viene con RolNombre
            AplicarFiltro();
        }

        private void AplicarFiltro()
        {
            string term = (txtBuscar.Text ?? "").Trim().ToLowerInvariant();
            int estadoIdx = cbEstado.SelectedIndex; // 0=Todos, 1=Activo, 2=Inactivo

            IEnumerable<Usuario> q = _all;

            if (estadoIdx == 1) q = q.Where(x => x.Estado);           // Activo
            else if (estadoIdx == 2) q = q.Where(x => !x.Estado);     // Inactivo

            if (term.Length > 0)
            {
                q = q.Where(x =>
                    (!string.IsNullOrEmpty(x.Nombre) && x.Nombre.ToLower().Contains(term)) ||
                    (!string.IsNullOrEmpty(x.Apellido) && x.Apellido.ToLower().Contains(term)) ||
                    (!string.IsNullOrEmpty(x.Email) && x.Email.ToLower().Contains(term)));
            }

            List<Usuario> arr = q.OrderBy(x => x.Apellido).ThenBy(x => x.Nombre).ToList();

            _datos.RaiseListChangedEvents = false;
            _datos.Clear();
            foreach (Usuario u in arr) _datos.Add(u);
            _datos.RaiseListChangedEvents = true;
            _datos.ResetBindings();

            lblFooter.Text = string.Format("Mostrando {0} de {1}", _datos.Count, _all.Count);

            if (_datos.Count == 0) RenderEmptyDetails();
        }

        // =========================
        // DGV: formateo y pintado
        // =========================
        private void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string col = dgv.Columns[e.ColumnIndex].Name;

            // Avatar por fila (cacheado por DNI)
            if (col == "Avatar")
            {
                Usuario u = dgv.Rows[e.RowIndex].DataBoundItem as Usuario;
                if (u == null) return;

                Image img;
                if (!_avatarCache.TryGetValue(u.Dni, out img))
                {
                    img = MakeAvatar(string.Format("{0} {1}", u.Nombre, u.Apellido).Trim());
                    _avatarCache[u.Dni] = img;
                }
                e.Value = img;
                e.FormattingApplied = true;
                return;
            }

            // Estado: bool -> "Activo"/"Inactivo" (para que el chip se pinte)
            if (col == "Estado")
            {
                if (e.Value is bool)
                {
                    bool b = (bool)e.Value;
                    e.Value = b ? "Activo" : "Inactivo";
                    e.FormattingApplied = true;
                }
                return;
            }

            // Acciones (icono)
            if (col == "Acciones")
            {
                e.Value = "⋯";
                e.FormattingApplied = true;
                return;
            }
        }

        private void Dgv_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgv.ClearSelection();
        }

        private void Dgv_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Rol (chip)
            if (dgv.Columns[e.ColumnIndex].Name == "Rol")
            {
                e.Handled = true;
                e.PaintBackground(e.ClipBounds, true);
                string text = Convert.ToString(e.FormattedValue ?? "");
                Color bg = text.Equals("Administrador", StringComparison.OrdinalIgnoreCase) ? Color.FromArgb(34, 139, 34)
                         : text.Equals("Propietario", StringComparison.OrdinalIgnoreCase) ? Color.FromArgb(200, 190, 80)
                         : Color.FromArgb(120, 120, 120);
                DrawChip(e.Graphics, e.CellBounds, text, bg, Color.White, 10);
                return;
            }

            // Estado (chip) — ya viene formateado en CellFormatting a "Activo"/"Inactivo"
            if (dgv.Columns[e.ColumnIndex].Name == "Estado")
            {
                e.Handled = true;
                e.PaintBackground(e.ClipBounds, true);
                string text = Convert.ToString(e.FormattedValue ?? "");
                Color bg = text.Equals("Activo", StringComparison.OrdinalIgnoreCase) ? Color.FromArgb(34, 139, 34)
                         : Color.FromArgb(200, 180, 80);
                DrawChip(e.Graphics, e.CellBounds, text, bg, Color.White, 12);
                return;
            }

            // Usuario multi-línea (usa NombreCompleto)
            if (dgv.Columns[e.ColumnIndex].Name == "Usuario")
            {
                e.Handled = true;
                e.PaintBackground(e.ClipBounds, true);
                Rectangle rect = new Rectangle(e.CellBounds.X + 2, e.CellBounds.Y + 6, e.CellBounds.Width - 4, e.CellBounds.Height - 12);
                TextRenderer.DrawText(e.Graphics, Convert.ToString(e.FormattedValue ?? ""), new Font("Segoe UI", 9f),
                    rect, ColText, TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
                return;
            }

            // Acciones (icono)
            if (dgv.Columns[e.ColumnIndex].Name == "Acciones")
            {
                e.Handled = true;
                e.PaintBackground(e.ClipBounds, true);
                TextRenderer.DrawText(e.Graphics, "⋯", new Font("Segoe UI", 12f, FontStyle.Bold), e.CellBounds, ColText,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }
        }

        private void DrawChip(Graphics g, Rectangle cell, string text, Color bg, Color fg, int radius)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Font font = new Font("Segoe UI", 9f, FontStyle.Bold);
            Size sz = TextRenderer.MeasureText(text, font);
            int padX = 12, padY = 6;
            int w = Math.Min(cell.Width - 12, sz.Width + padX * 2);
            int h = Math.Min(cell.Height - 12, sz.Height - 4 + padY * 2);

            int x = cell.X + (cell.Width - w) / 2;
            int y = cell.Y + (cell.Height - h) / 2;

            using (GraphicsPath path = RoundedRect(new Rectangle(x, y, w, h), radius))
            {
                using (SolidBrush sb = new SolidBrush(bg))
                {
                    g.FillPath(sb, path);
                }
            }

            Rectangle textRect = new Rectangle(x, y, w, h);
            TextRenderer.DrawText(g, text, font, textRect, fg,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // ===== Details =====
        private void RenderEmptyDetails()
        {
            pnlDetails.Controls.Clear();
            Label lbl = new Label
            {
                Text = "No hay usuarios. Agregá uno con “+ Nuevo Usuario”.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = ColText,
                Font = new Font("Segoe UI", 10f, FontStyle.Italic)
            };
            pnlDetails.Controls.Add(lbl);
        }

        private void Dgv_SelectionChanged(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { RenderEmptyDetails(); return; }
            Usuario u = dgv.SelectedRows[0].DataBoundItem as Usuario;
            if (u == null) { RenderEmptyDetails(); return; }

            Image img;
            _avatarCache.TryGetValue(u.Dni, out img);

            pnlDetails.SuspendLayout();
            pnlDetails.Controls.Clear();

            Panel topWrap = new Panel { Dock = DockStyle.Top, Height = 120, Padding = new Padding(8, 8, 8, 8) };
            PictureBox pic = new PictureBox { Image = img ?? MakeAvatar((u.Nombre + " " + u.Apellido).Trim()), SizeMode = PictureBoxSizeMode.Zoom, Width = 72, Height = 72, Location = new Point(8, 8) };
            Label lblNombre = new Label { Text = u.NombreCompleto, AutoSize = true, Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = ColText, Location = new Point(88, 10) };
            Label lblEmail = new Label { Text = u.Email, AutoSize = true, ForeColor = Color.FromArgb(90, 100, 90), Location = new Point(88, 36) };
            Label lblEstado = new Label { Text = string.Format("Estado: {0} | Rol: {1}", (u.Estado ? "Activo" : "Inactivo"), u.RolNombre), AutoSize = true, ForeColor = ColText, Location = new Point(8, 90) };

            topWrap.Controls.Add(lblEstado);
            topWrap.Controls.Add(lblEmail);
            topWrap.Controls.Add(lblNombre);
            topWrap.Controls.Add(pic);

            FlowLayoutPanel flBtns = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(8, 0, 8, 0) };
            string btnEstadoText = u.Estado ? "Dar de baja" : "Activar";
            Button btnEstado = MakeGhost(btnEstadoText);
            Button btnEditar = MakeGhost("Editar");
            Button btnMail = MakeGhost("✉️ Enviar correo");
            flBtns.Controls.Add(btnEstado);
            flBtns.Controls.Add(btnEditar);
            flBtns.Controls.Add(btnMail);

            // Evento para cambiar estado
            btnEstado.Click += (sender2, args2) =>
            {
                if (u == null) return;
                bool nuevoEstado = !u.Estado;
                string accion = nuevoEstado ? "activar" : "dar de baja";
                var result = MessageBox.Show($"¿Desea {accion} al usuario {u.NombreCompleto}?\n\nActualmente: {(u.Estado ? "Activo" : "Inactivo")}",
                    btnEstado.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    u.Estado = nuevoEstado;
                    _repo.ActualizarEstadoUsuario(u.Dni, nuevoEstado);
                    RefrescarDesdeBD();
                }
            };

            // Evento para editar usuario
            btnEditar.Click += (sender2, args2) =>
            {
                if (u == null) return;
                using (var f = new EditarUsuarioForm(_repo, u, _all))
                {
                    if (f.ShowDialog(FindForm()) == DialogResult.OK && f.Resultado != null)
                    {
                        try
                        {
                            _repo.ActualizarUsuario(f.Resultado);
                            RefrescarDesdeBD();
                            MessageBox.Show("Usuario editado con éxito.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("No se pudo editar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            };

            Panel info = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 8, 8, 8) };
            info.Controls.Add(new Label { Text = "Email: " + u.Email, AutoSize = true, ForeColor = ColText, Location = new Point(8, 8) });
            info.Controls.Add(new Label { Text = "Rol: " + u.RolNombre, AutoSize = true, ForeColor = ColText, Location = new Point(8, 30) });
            info.Controls.Add(new Label { Text = "Teléfono: " + (u.Telefono ?? "-"), AutoSize = true, ForeColor = ColText, Location = new Point(8, 52) });
            if (u.FechaNacimiento.HasValue)
                info.Controls.Add(new Label { Text = "Fecha Nac.: " + u.FechaNacimiento.Value.ToString("dd/MM/yyyy"), AutoSize = true, ForeColor = ColText, Location = new Point(8, 74) });

            pnlDetails.Controls.Add(info);
            pnlDetails.Controls.Add(flBtns);
            pnlDetails.Controls.Add(topWrap);
            pnlDetails.ResumeLayout();
        }

        // ====== API util =====
        public void ClearUsuarios()
        {
            _all = new List<Usuario>();
            _datos.Clear();
            dgv.ClearSelection();
            lblFooter.Text = "Mostrando 0 de 0";
            RenderEmptyDetails();
        }

        // =========================
        // Helpers UI
        // =========================
        private ComboBox MakeCombo(string[] items)
        {
            ComboBox cb = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Height = 34,
                Width = 160,
                Margin = new Padding(0, 0, 8, 0)
            };
            cb.Items.AddRange(items);
            cb.SelectedIndex = 0;
            return cb;
        }

        private Button MakeAction(string text)
        {
            Button b = new Button
            {
                Text = text,
                AutoSize = true,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = ColAccent,
                ForeColor = Color.White,
                Margin = new Padding(8, 0, 0, 0),
                Padding = new Padding(12, 6, 12, 6)
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private Button MakeGhost(string text)
        {
            Button b = new Button
            {
                Text = text,
                AutoSize = true,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = ColText,
                Margin = new Padding(8, 0, 0, 0),
                Padding = new Padding(12, 6, 12, 6)
            };
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = ColBorder;
            return b;
        }

        // =========================
        // Avatares util
        // =========================
        private Image MakeAvatar(string nombre)
        {
            string initials = GetInitials(nombre);
            int size = 44;
            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Color bg = RandomPastel(nombre);
                using (SolidBrush br = new SolidBrush(bg)) g.FillEllipse(br, 0, 0, size - 1, size - 1);
                using (Pen p = new Pen(Color.FromArgb(220, 220, 220))) g.DrawEllipse(p, 0, 0, size - 1, size - 1);
                using (Font f = new Font("Segoe UI", 11f, FontStyle.Bold, GraphicsUnit.Point))
                using (SolidBrush sb = new SolidBrush(Color.White))
                {
                    SizeF sz = g.MeasureString(initials, f);
                    g.DrawString(initials, f, sb, (size - sz.Width) / 2f, (size - sz.Height) / 2f - 1);
                }
            }
            return bmp;
        }

        private static string GetInitials(string full)
        {
            string[] parts = (full ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "?";
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
            return (parts[0].Substring(0, 1) + parts[1].Substring(0, 1)).ToUpper();
        }

        private static Color RandomPastel(string seed)
        {
            int h = (seed ?? "").GetHashCode();
            Random rnd = new Random(h);
            int r = (rnd.Next(120, 200) + 40) / 2;
            int g = (rnd.Next(120, 200) + 40) / 2;
            int b = (rnd.Next(120, 200) + 40) / 2;
            return Color.FromArgb(255, r, g, b);
        }

        // =========================
        // Mini formulario interno para ALTAS
        // =========================
        private class NuevoUsuarioForm : Form
        {
            public Usuario Resultado { get; private set; }

            private readonly UsuarioRepository _repo;
            private TextBox txtDni, txtNombre, txtApellido, txtEmail, txtTelefono, txtPassword;
            private DateTimePicker dtpNac;
            private CheckBox chkActivo;
            private ComboBox cmbRol;

            public NuevoUsuarioForm(UsuarioRepository repo)
            {
                _repo = repo;
                Text = "Nuevo Usuario";
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false; MinimizeBox = false;
                Width = 420; Height = 460;

                TableLayoutPanel root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), RowCount = 2 };
                root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
                Controls.Add(root);

                TableLayoutPanel grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoSize = true };
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

                txtDni = AddText(grid, "DNI");
                txtNombre = AddText(grid, "Nombre");
                txtApellido = AddText(grid, "Apellido");
                txtEmail = AddText(grid, "Email");
                txtTelefono = AddText(grid, "Teléfono");
                txtPassword = AddText(grid, "Contraseña"); txtPassword.UseSystemPasswordChar = true;

                grid.Controls.Add(new Label { Text = "Fecha Nac.", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 6);
                dtpNac = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Dock = DockStyle.Fill };
                grid.Controls.Add(dtpNac, 1, 6);

                grid.Controls.Add(new Label { Text = "Estado", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 7);
                chkActivo = new CheckBox { Text = "Activo", Checked = true, Dock = DockStyle.Left };
                grid.Controls.Add(chkActivo, 1, 7);

                grid.Controls.Add(new Label { Text = "Rol", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 8);
                cmbRol = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
                grid.Controls.Add(cmbRol, 1, 8);

                root.Controls.Add(grid, 0, 0);

                FlowLayoutPanel panelBtns = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill };
                Button btnOk = new Button { Text = "Guardar", DialogResult = DialogResult.OK, AutoSize = true };
                Button btnCancel = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, AutoSize = true };
                panelBtns.Controls.Add(btnOk);
                panelBtns.Controls.Add(btnCancel);
                root.Controls.Add(panelBtns, 0, 1);

                Load += delegate
                {
                    List<ValueTuple<int, string>> roles = _repo.ObtenerRoles(); // (IdRol, Nombre)
                    cmbRol.DataSource = roles.Select(r => new { Id = r.Item1, Nombre = r.Item2 }).ToList();
                    cmbRol.DisplayMember = "Nombre";
                    cmbRol.ValueMember = "Id";
                    if (roles.Count > 0) cmbRol.SelectedIndex = 0;
                };

                btnOk.Click += delegate
                {
                    if (!Validar()) { DialogResult = DialogResult.None; return; }

                    Usuario u = new Usuario
                    {
                        Dni = int.Parse(txtDni.Text.Trim()),
                        Nombre = txtNombre.Text.Trim(),
                        Apellido = txtApellido.Text.Trim(),
                        Email = txtEmail.Text.Trim(),
                        Telefono = string.IsNullOrWhiteSpace(txtTelefono.Text) ? null : txtTelefono.Text.Trim(),
                        Password = txtPassword.Text, // puede ir vacío
                        FechaNacimiento = dtpNac.Checked ? (DateTime?)dtpNac.Value.Date : null,
                        Estado = chkActivo.Checked,
                        IdRol = (int)cmbRol.SelectedValue,
                        RolNombre = (cmbRol.SelectedItem as dynamic).Nombre
                    };
                    Resultado = u;
                };
            }

            private TextBox AddText(TableLayoutPanel grid, string label)
            {
                int r = grid.RowCount++;
                grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
                grid.Controls.Add(new Label { Text = label, Anchor = AnchorStyles.Left, AutoSize = true }, 0, r);
                TextBox tb = new TextBox { Dock = DockStyle.Fill };
                grid.Controls.Add(tb, 1, r);
                return tb;
            }

            private bool Validar()
            {
                int tmp;
                if (!int.TryParse(txtDni.Text.Trim(), out tmp)) { MessageBox.Show("DNI inválido"); return false; }
                if (string.IsNullOrWhiteSpace(txtNombre.Text)) { MessageBox.Show("Nombre requerido"); return false; }
                if (string.IsNullOrWhiteSpace(txtApellido.Text)) { MessageBox.Show("Apellido requerido"); return false; }
                if (string.IsNullOrWhiteSpace(txtEmail.Text)) { MessageBox.Show("Email requerido"); return false; }
                if (cmbRol.SelectedValue == null) { MessageBox.Show("Rol requerido"); return false; }
                return true;
            }
        }
    }
}
