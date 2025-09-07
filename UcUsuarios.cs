using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Proyecto_Taller_2
{
    public class UcUsuarios : UserControl
    {
        // ===== Placeholder (cue banner) para TextBox en .NET Framework =====
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;
        private static void SetPlaceholder(TextBox tb, string text) { SendMessage(tb.Handle, EM_SETCUEBANNER, 1, text); }

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

        public UcUsuarios()
        {
            // Evita recortes por DPI
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.DoubleBuffered = true;
            this.BackColor = ColBg;
            this.Dock = DockStyle.Fill;

            BuildUI();
            CargarDatosPrueba();
            dgv.ClearSelection();
        }

        private void BuildUI()
        {
            // ===== ROOT (padding general para que nada toque los bordes) =====
            var rootPad = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16) };
            Controls.Add(rootPad);

            tlRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = ColBg
            };
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));  // Top bar (↑)
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Grid + details
            tlRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));  // Footer (↑)
            rootPad.Controls.Add(tlRoot);

            // ===== TOP BAR (con más padding para que no se corte) =====
            var top = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(4, 6, 4, 6)   // ↑ aire vertical
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
                Padding = new Padding(0, 6, 0, 6)   // ↑ aire interno
            };
            var txtWrap = new Panel
            {
                Width = 320,
                Height = 34,
                Padding = new Padding(8, 6, 8, 6),
                BackColor = ColSoft,
                Margin = new Padding(0, 0, 8, 0)
            };
            txtBuscar = new TextBox { BorderStyle = BorderStyle.None, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5f) };
            SetPlaceholder(txtBuscar, "Buscar por nombre, email o empresa…");
            txtWrap.Controls.Add(txtBuscar);

            cbSegmento = MakeCombo(new[] { "Todos los segmentos", "VIP", "Premium", "Regular" });
            cbEstado = MakeCombo(new[] { "Todos los estados", "Activo", "Inactivo" });

            flTopLeft.Controls.Add(txtWrap);
            flTopLeft.Controls.Add(cbSegmento);
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
            btnNuevo = MakeAction("+ Nuevo Cliente");
            btnExportar = MakeGhost("Exportar");
            btnImportar = MakeGhost("Importar");
            flTopRight.Controls.Add(btnNuevo);
            flTopRight.Controls.Add(btnExportar);
            flTopRight.Controls.Add(btnImportar);

            top.Controls.Add(flTopLeft, 0, 0);
            top.Controls.Add(flTopRight, 1, 0);

            // ===== LISTA + DETAILS =====
            var splitPad = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 4, 0, 0) };
            tlRoot.Controls.Add(splitPad, 0, 1);

            var split = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2
            };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320)); // ↑ un poco más ancho
            splitPad.Controls.Add(split);

            // ---- GRID ----
            var gridPad = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 12, 0) }; // separa de details
            split.Controls.Add(gridPad, 0, 0);

            gbLista = new GroupBox
            {
                Text = "Lista de Clientes",
                Dock = DockStyle.Fill,
                Padding = new Padding(12),     // ↑ más aire dentro del groupbox
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
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,   // controlamos altura
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                ColumnHeadersHeight = 42,                               // ↑ header alto
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                GridColor = ColBorder,
                EnableHeadersVisualStyles = false
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = ColSoftAlt;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = ColText;

            // padding y wrap en celdas para que no se corten textos
            dgv.DefaultCellStyle.Padding = new Padding(8, 10, 8, 10);
            dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 232, 220);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;

            // altura de fila generosa
            dgv.RowTemplate.Height = 64;

            // borde de grupo más limpio
            dgv.AdvancedCellBorderStyle.Left = DataGridViewAdvancedCellBorderStyle.Single;
            dgv.AdvancedCellBorderStyle.Right = DataGridViewAdvancedCellBorderStyle.Single;

            dgv.CellPainting += Dgv_CellPainting;
            dgv.SelectionChanged += Dgv_SelectionChanged;

            var cAvatar = new DataGridViewImageColumn { Name = "Avatar", HeaderText = "", FillWeight = 56, ImageLayout = DataGridViewImageCellLayout.Zoom };
            var cCliente = new DataGridViewTextBoxColumn { Name = "Cliente", HeaderText = "Cliente", FillWeight = 180, DefaultCellStyle = new DataGridViewCellStyle { WrapMode = DataGridViewTriState.True } };
            var cEmpresa = new DataGridViewTextBoxColumn { Name = "Empresa", HeaderText = "Empresa", FillWeight = 140 };
            var cContacto = new DataGridViewTextBoxColumn { Name = "Contacto", HeaderText = "Contacto", FillWeight = 70 };
            var cSegmento = new DataGridViewTextBoxColumn { Name = "Segmento", HeaderText = "Segmento", FillWeight = 90 };
            var cTotal = new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "Total Compras", FillWeight = 100 };
            var cUltima = new DataGridViewTextBoxColumn { Name = "Ultima", HeaderText = "Última Compra", FillWeight = 100 };
            var cEstado = new DataGridViewTextBoxColumn { Name = "Estado", HeaderText = "Estado", FillWeight = 90 };
            var cAcciones = new DataGridViewTextBoxColumn { Name = "Acciones", HeaderText = "Acciones", FillWeight = 60 };

            dgv.Columns.AddRange(new DataGridViewColumn[] { cAvatar, cCliente, cEmpresa, cContacto, cSegmento, cTotal, cUltima, cEstado, cAcciones });
            gbLista.Controls.Add(dgv);

            // ---- DETAILS ----
            var detailsPad = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 8, 12, 8), BackColor = ColSoft };
            split.Controls.Add(detailsPad, 1, 0);
            pnlDetails = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 16, 8, 8), BackColor = ColSoft }; // ↑ top padding
            detailsPad.Controls.Add(pnlDetails);
            RenderEmptyDetails();

            // ===== FOOTER =====
            var footer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 6, 8, 6), BackColor = ColBg };
            tlRoot.Controls.Add(footer, 0, 2);

            lblFooter = new Label { AutoSize = true, Text = "Mostrando 1–5 de 5", ForeColor = ColText, Font = new Font("Segoe UI", 9f) };
            btnPrev = MakeGhost("Anterior"); btnPrev.Enabled = false;
            btnNext = MakeGhost("Siguiente"); btnNext.Enabled = false;

            var flFoot = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(0) };
            flFoot.Controls.Add(lblFooter);
            flFoot.Controls.Add(btnPrev);
            flFoot.Controls.Add(btnNext);
            footer.Controls.Add(flFoot);
        }

        // ===== Helpers UI =====
        private ComboBox MakeCombo(string[] items)
        {
            var cb = new ComboBox
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
            var b = new Button
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
            var b = new Button
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

        // ===== Datos demo =====
        private void CargarDatosPrueba()
        {
            var items = new List<Tuple<string, string, string, string, string, string, string>>();
            items.Add(new Tuple<string, string, string, string, string, string, string>("Carmen López", "carmen.lopez@email.com", "Local Business", "Regular", "$1.850,25", "11/1/2024", "Activo"));
            items.Add(new Tuple<string, string, string, string, string, string, string>("María Gonzalez", "maria.gonzalez@email.com", "Tech Solutions SA", "VIP", "$15.420,50", "14/1/2024", "Activo"));
            items.Add(new Tuple<string, string, string, string, string, string, string>("Luis Fernández", "luis.fernandez@email.com", "Global Ent.", "VIP", "$22.100", "19/12/2023", "Inactivo"));
            items.Add(new Tuple<string, string, string, string, string, string, string>("Ana Martínez", "ana.martinez@email.com", "StartUp Inc", "Regular", "$3.200,75", "7/1/2024", "Activo"));
            items.Add(new Tuple<string, string, string, string, string, string, string>("Carlos Rodriguez", "carlos.rodriguez@email.com", "Innovate Corp", "Premium", "$8.750", "9/1/2024", "Activo"));

            foreach (var it in items)
            {
                Image img = MakeAvatar(it.Item1);
                string cliente = it.Item1 + "\n" + it.Item2;
                string contacto = "📞  ✉️";
                dgv.Rows.Add(img, cliente, it.Item3, contacto, it.Item4, it.Item5, it.Item6, it.Item7, "⋯");
            }

            // Alternar color de filas (suave, que no “corte”)
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 248);
        }

        // Avatares
        private Image MakeAvatar(string nombre)
        {
            string initials = GetInitials(nombre);
            int size = 44;
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var bg = RandomPastel(nombre);
                using (var b = new SolidBrush(bg)) g.FillEllipse(b, 0, 0, size - 1, size - 1);
                using (var p = new Pen(Color.FromArgb(220, 220, 220))) g.DrawEllipse(p, 0, 0, size - 1, size - 1);
                using (var f = new Font("Segoe UI", 11f, FontStyle.Bold, GraphicsUnit.Point))
                using (var sb = new SolidBrush(Color.White))
                {
                    var sz = g.MeasureString(initials, f);
                    g.DrawString(initials, f, sb, (size - sz.Width) / 2f, (size - sz.Height) / 2f - 1);
                }
            }
            return bmp;
        }
        private static string GetInitials(string full)
        {
            var parts = full.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
            return (parts[0].Substring(0, 1) + parts[1].Substring(0, 1)).ToUpper();
        }
        private static Color RandomPastel(string seed)
        {
            int h = seed.GetHashCode();
            var rnd = new Random(h);
            int r = (rnd.Next(120, 200) + 40) / 2;
            int g = (rnd.Next(120, 200) + 40) / 2;
            int b = (rnd.Next(120, 200) + 40) / 2;
            return Color.FromArgb(255, r, g, b);
        }

        // ===== Pintado custom de chips/badges =====
        private void Dgv_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Segmento
            if (dgv.Columns[e.ColumnIndex].Name == "Segmento")
            {
                e.Handled = true;
                e.PaintBackground(e.ClipBounds, true);
                string text = Convert.ToString(e.FormattedValue ?? "");
                var c = ChipColors(text);
                DrawChip(e.Graphics, e.CellBounds, text, c.Item1, c.Item2, 10);
                return;
            }

            // Estado
            if (dgv.Columns[e.ColumnIndex].Name == "Estado")
            {
                e.Handled = true;
                e.PaintBackground(e.ClipBounds, true);
                string text = Convert.ToString(e.FormattedValue ?? "");
                Color bg = text.Equals("Activo", StringComparison.OrdinalIgnoreCase) ? Color.FromArgb(34, 139, 34) : Color.FromArgb(200, 180, 80);
                DrawChip(e.Graphics, e.CellBounds, text, bg, Color.White, 12);
                return;
            }

            // Acciones
            if (dgv.Columns[e.ColumnIndex].Name == "Acciones")
            {
                e.Handled = true;
                e.PaintBackground(e.ClipBounds, true);
                TextRenderer.DrawText(e.Graphics, "⋯", new Font("Segoe UI", 12f, FontStyle.Bold), e.CellBounds, ColText,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            // Cliente (2 líneas)
            if (dgv.Columns[e.ColumnIndex].Name == "Cliente")
            {
                e.Handled = true;
                e.PaintBackground(e.ClipBounds, true);
                var rect = new Rectangle(e.CellBounds.X + 2, e.CellBounds.Y + 6, e.CellBounds.Width - 4, e.CellBounds.Height - 12);
                TextRenderer.DrawText(e.Graphics, Convert.ToString(e.FormattedValue ?? ""), new Font("Segoe UI", 9f),
                    rect, ColText, TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
                return;
            }
        }

        private void DrawChip(Graphics g, Rectangle cell, string text, Color bg, Color fg, int radius)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var font = new Font("Segoe UI", 9f, FontStyle.Bold);
            var sz = TextRenderer.MeasureText(text, font);
            int padX = 12, padY = 6;
            int w = Math.Min(cell.Width - 12, sz.Width + padX * 2);
            int h = Math.Min(cell.Height - 12, sz.Height - 4 + padY * 2);

            int x = cell.X + (cell.Width - w) / 2;
            int y = cell.Y + (cell.Height - h) / 2;

            using (GraphicsPath path = RoundedRect(new Rectangle(x, y, w, h), radius))
            using (SolidBrush sb = new SolidBrush(bg))
            {
                g.FillPath(sb, path);
            }
            var textRect = new Rectangle(x, y, w, h);
            TextRenderer.DrawText(g, text, font, textRect, fg,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
        private Tuple<Color, Color> ChipColors(string segmento)
        {
            if (segmento.Equals("VIP", StringComparison.OrdinalIgnoreCase))
                return new Tuple<Color, Color>(Color.FromArgb(34, 139, 34), Color.White);
            if (segmento.Equals("Premium", StringComparison.OrdinalIgnoreCase))
                return new Tuple<Color, Color>(Color.FromArgb(200, 190, 80), Color.White);
            return new Tuple<Color, Color>(Color.FromArgb(220, 224, 220), ColText);
        }

        // ===== Details =====
        private void RenderEmptyDetails()
        {
            pnlDetails.Controls.Clear();
            var lbl = new Label
            {
                Text = "Seleccioná un cliente para ver detalles",
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
            var r = dgv.SelectedRows[0];
            var img = r.Cells["Avatar"].Value as Image;
            string nombreEmail = Convert.ToString(r.Cells["Cliente"].Value ?? "");
            string empresa = Convert.ToString(r.Cells["Empresa"].Value ?? "");
            string segmento = Convert.ToString(r.Cells["Segmento"].Value ?? "");
            string total = Convert.ToString(r.Cells["Total"].Value ?? "");
            string ultima = Convert.ToString(r.Cells["Ultima"].Value ?? "");
            string estado = Convert.ToString(r.Cells["Estado"].Value ?? "");

            pnlDetails.SuspendLayout();
            pnlDetails.Controls.Clear();

            var topWrap = new Panel { Dock = DockStyle.Top, Height = 120, Padding = new Padding(8, 8, 8, 8) };
            var pic = new PictureBox { Image = img, SizeMode = PictureBoxSizeMode.Zoom, Width = 72, Height = 72, Location = new Point(8, 8) };
            var lblNombre = new Label { Text = nombreEmail.Split('\n')[0], AutoSize = true, Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = ColText, Location = new Point(88, 10) };
            var lblEmail = new Label { Text = nombreEmail.Contains("\n") ? nombreEmail.Split('\n')[1] : "", AutoSize = true, ForeColor = Color.FromArgb(90, 100, 90), Location = new Point(88, 36) };

            var lblEstado = new Label { Text = "Estado: " + estado + ", " + ultima, AutoSize = true, ForeColor = ColText, Location = new Point(8, 90) };
            topWrap.Controls.Add(lblEstado);
            topWrap.Controls.Add(lblEmail);
            topWrap.Controls.Add(lblNombre);
            topWrap.Controls.Add(pic);

            var flBtns = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(8, 0, 8, 0) };
            var btnLlamar = MakeAction("📞 Llamar");
            var btnMail = MakeGhost("✉️ Enviar correo");
            var btnEditar = MakeGhost("Editar");
            flBtns.Controls.Add(btnLlamar);
            flBtns.Controls.Add(btnMail);
            flBtns.Controls.Add(btnEditar);

            var info = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 8, 8, 8) };
            info.Controls.Add(new Label { Text = "Empresa: " + empresa, AutoSize = true, ForeColor = ColText, Location = new Point(8, 8) });
            info.Controls.Add(new Label { Text = "Segmento: " + segmento, AutoSize = true, ForeColor = ColText, Location = new Point(8, 30) });
            info.Controls.Add(new Label { Text = "Total compras: " + total, AutoSize = true, ForeColor = ColText, Location = new Point(8, 52) });

            pnlDetails.Controls.Add(info);
            pnlDetails.Controls.Add(flBtns);
            pnlDetails.Controls.Add(topWrap);
            pnlDetails.ResumeLayout();
        }
    }
}
