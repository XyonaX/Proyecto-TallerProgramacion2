using System.Drawing;
using System.Windows.Forms;

namespace Proyecto_Taller_2
{
    partial class UcConfiguracion
    {
        private System.ComponentModel.IContainer components = null;

        private Panel pnlScroll;          
        private TableLayoutPanel tlRoot;  
        private Label lblTitulo;

        private RoundedPanel cardApariencia;
        private RoundedPanel cardGeneral;
        private RoundedPanel cardDatos;

        private Panel pnlBottom;
        private Button btnGuardar;
        private Button btnAplicar;
        private Button btnCancelar;

        // Apariencia
        private ComboBox cbTema;
        private Button btnColorPrimario;
        private NumericUpDown nudFont;
        private CheckBox chkCompacto;

        // General
        private ComboBox cbIdioma;
        private ComboBox cbFecha;
        private ComboBox cbMoneda;

        // Datos
        private TextBox txtBackup;
        private Button btnElegirBackup;
        private CheckBox chkAutoBackup;
        private Button btnCrearBackup;
        private Button btnRestaurarBackup;
        private Label lblUltimoBackup;
        private Button btnExportar;
        private Button btnImportar;
        private Button btnReset;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            // ====== USERCONTROL ======
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.DoubleBuffered = true;
            this.Name = "UcConfiguracion";
            this.Size = new Size(1100, 680);
            this.BackColor = Color.White;

            // ====== HOST CON SCROLL ======
            pnlScroll = new Panel();
            pnlScroll.Dock = DockStyle.Fill;
            pnlScroll.AutoScroll = true;
            pnlScroll.Padding = new Padding(24, 24, 24, 0); // el bottom lo ocupa pnlBottom

            // ====== GRID (5 columnas: 33% | 16px | 33% | 16px | 33%) ======
            tlRoot = new TableLayoutPanel();
            tlRoot.AutoSize = true;
            tlRoot.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlRoot.Dock = DockStyle.Top;                 // importante para que el host calcule altura + scroll
            tlRoot.ColumnCount = 5;
            tlRoot.RowCount = 2;                         // Título + fila de cards
            tlRoot.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;

            tlRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            tlRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 16f)); // separador
            tlRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            tlRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 16f)); // separador
            tlRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));

            tlRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // ====== TÍTULO ======
            lblTitulo = new Label();
            lblTitulo.Text = "Configuración";
            lblTitulo.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitulo.AutoSize = true;
            lblTitulo.Margin = new Padding(0, 0, 0, 16);
            tlRoot.Controls.Add(lblTitulo, 0, 0);
            tlRoot.SetColumnSpan(lblTitulo, 5);

            // ====== CARDS ======
            cardApariencia = new RoundedPanel();
            cardGeneral = new RoundedPanel();
            cardDatos = new RoundedPanel();

            RoundedCard(cardApariencia);
            RoundedCard(cardGeneral);
            RoundedCard(cardDatos);

            // Apariencia
            var lblA = Title("Apariencia");
            var lblTema = FieldLabel("Tema");
            cbTema = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
            cbTema.Items.AddRange(new object[] { "Sistema", "Claro", "Oscuro" });

            var lblColor = FieldLabel("Color primario");
            btnColorPrimario = new Button { Text = "Elegir color", Width = 120 };

            var lblFont = FieldLabel("Tamaño de fuente");
            nudFont = new NumericUpDown { Minimum = 8, Maximum = 16, Value = 10, Width = 80 };

            chkCompacto = new CheckBox { Text = "Modo compacto (menos espacio)", AutoSize = true, Margin = new Padding(0, 8, 0, 0) };

            var layA = VStack();
            layA.Controls.Add(lblA);
            layA.Controls.Add(lblTema); layA.Controls.Add(cbTema);
            layA.Controls.Add(lblColor); layA.Controls.Add(btnColorPrimario);
            layA.Controls.Add(lblFont); layA.Controls.Add(nudFont);
            layA.Controls.Add(chkCompacto);
            cardApariencia.Controls.Add(layA);

            // General
            var lblG = Title("General");
            var lblIdioma = FieldLabel("Idioma");
            cbIdioma = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
            cbIdioma.Items.AddRange(new object[] { "es-AR", "en-US" });

            var lblFecha = FieldLabel("Formato de fecha");
            cbFecha = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
            cbFecha.Items.AddRange(new object[] { "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd" });

            var lblMon = FieldLabel("Formato de moneda (cultura)");
            cbMoneda = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
            cbMoneda.Items.AddRange(new object[] { "es-AR", "en-US", "es-ES", "pt-BR" });

            var layG = VStack();
            layG.Controls.Add(lblG);
            layG.Controls.Add(lblIdioma); layG.Controls.Add(cbIdioma);
            layG.Controls.Add(lblFecha); layG.Controls.Add(cbFecha);
            layG.Controls.Add(lblMon); layG.Controls.Add(cbMoneda);
            cardGeneral.Controls.Add(layG);

            // Datos
            // ---- Datos (sin scroll interno)
            var lblD = Title("Datos y copias de seguridad");

            // layout de 3 columnas: etiqueta | campo (stretch) | botón
            var tlDatos = new TableLayoutPanel();
            tlDatos.Dock = DockStyle.Fill;
            tlDatos.AutoSize = true;
            tlDatos.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlDatos.ColumnCount = 3;
            tlDatos.RowCount = 7; // Aumentado de 5 a 7
            tlDatos.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));        // etiqueta
            tlDatos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));   // campo elástico
            tlDatos.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));        // botón

            // fila 0: título ocupa todo
            tlDatos.Controls.Add(lblD, 0, 0);
            tlDatos.SetColumnSpan(lblD, 3);
            lblD.Margin = new Padding(0, 0, 0, 8);

            // fila 1: carpeta backups
            var lblRuta = FieldLabel("Carpeta de backups");
            this.txtBackup = new TextBox { ReadOnly = true, Margin = new Padding(0, 6, 0, 0) };
            this.txtBackup.Anchor = AnchorStyles.Left | AnchorStyles.Right;

            this.btnElegirBackup = new Button { Text = "Elegir carpeta...", AutoSize = true, Margin = new Padding(8, 4, 0, 0) };

            tlDatos.Controls.Add(lblRuta,        0, 1);
            tlDatos.Controls.Add(this.txtBackup,      1, 1);
            tlDatos.Controls.Add(this.btnElegirBackup,2, 1);

            // fila 2: información último backup
            this.lblUltimoBackup = new Label 
            { 
                Text = "Cargando información de backup...", 
                AutoSize = true, 
                Margin = new Padding(0, 8, 0, 0),
                ForeColor = Color.FromArgb(100, 100, 100)
            };
            tlDatos.Controls.Add(this.lblUltimoBackup, 0, 2);
            tlDatos.SetColumnSpan(this.lblUltimoBackup, 3);

            // fila 3: botones de backup
            this.btnCrearBackup = new Button 
            { 
                Text = "Crear Backup Ahora", 
                AutoSize = true,
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height = 32,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 8, 0, 0)
            };
            this.btnCrearBackup.FlatAppearance.BorderSize = 0;

            this.btnRestaurarBackup = new Button 
            { 
                Text = "Restaurar Backup", 
                AutoSize = true,
                Margin = new Padding(8, 8, 0, 0),
                Height = 32
            };

            var filaBackup = new FlowLayoutPanel 
            { 
                FlowDirection = FlowDirection.LeftToRight, 
                AutoSize = true, 
                WrapContents = false 
            };
            filaBackup.Controls.Add(this.btnCrearBackup);
            filaBackup.Controls.Add(this.btnRestaurarBackup);

            tlDatos.Controls.Add(filaBackup, 0, 3);
            tlDatos.SetColumnSpan(filaBackup, 3);

            // fila 4: backup automático
            this.chkAutoBackup = new CheckBox { Text = "Hacer backup automático al cerrar", AutoSize = true, Margin = new Padding(0, 10, 0, 0) };
            tlDatos.Controls.Add(this.chkAutoBackup, 0, 4);
            tlDatos.SetColumnSpan(this.chkAutoBackup, 3);

            // fila 5: exportar / importar
            this.btnExportar = new Button { Text = "Exportar configuración…", AutoSize = true };
            this.btnImportar = new Button { Text = "Importar configuración…", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };

            var filaEI = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, WrapContents = false, Margin = new Padding(0, 10, 0, 0) };
            filaEI.Controls.Add(this.btnExportar);
            filaEI.Controls.Add(this.btnImportar);

            tlDatos.Controls.Add(filaEI, 0, 5);
            tlDatos.SetColumnSpan(filaEI, 3);

            // fila 6: reset
            this.btnReset = new Button { Text = "Restablecer valores", AutoSize = true, Margin = new Padding(0, 8, 0, 0) };
            tlDatos.Controls.Add(this.btnReset, 0, 6);
            tlDatos.SetColumnSpan(this.btnReset, 3);

            // montar en card
            this.cardDatos.Controls.Clear();
            this.cardDatos.Controls.Add(tlDatos);

            // Ubicar las 3 cards en columnas 0,2,4
            tlRoot.Controls.Add(cardApariencia, 0, 1);
            tlRoot.Controls.Add(cardGeneral, 2, 1);
            tlRoot.Controls.Add(cardDatos, 4, 1);

            // ====== BOTTOM (fuera del host, fijo) ======
            pnlBottom = new Panel();
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Height = 56;
            pnlBottom.Padding = new Padding(24, 8, 24, 16);
            pnlBottom.BackColor = Color.Transparent;

            btnGuardar = new Button { Text = "Guardar", Width = 120 };
            btnAplicar = new Button { Text = "Aplicar", Width = 120, Margin = new Padding(6, 0, 0, 0) };
            btnCancelar = new Button { Text = "Cancelar", Width = 120, Margin = new Padding(6, 0, 0, 0) };

            var layB = new FlowLayoutPanel();
            layB.Dock = DockStyle.Right;
            layB.FlowDirection = FlowDirection.LeftToRight;
            layB.WrapContents = false;
            layB.Controls.Add(btnGuardar);
            layB.Controls.Add(btnAplicar);
            layB.Controls.Add(btnCancelar);

            pnlBottom.Controls.Add(layB);

            // ====== ENSAMBLADO ======
            pnlScroll.Controls.Add(tlRoot);
            this.Controls.Add(pnlScroll);
            this.Controls.Add(pnlBottom);
        }

        // ---------- Helpers UI ----------
        private static void RoundedCard(RoundedPanel p)
        {
            p.Dock = DockStyle.Fill;
            p.Padding = new Padding(16);
            p.Margin = new Padding(0, 0, 0, 16);
            p.MinimumSize = new Size(320, 380); // evita cortes
            p.AutoScroll = false;
        }

        private static Label Title(string text)
        {
            var l = new Label();
            l.Text = text;
            l.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            l.AutoSize = true;
            return l;
        }

        private static Label FieldLabel(string text)
        {
            var l = new Label();
            l.Text = text;
            l.AutoSize = true;
            l.Margin = new Padding(0, 12, 0, 4);
            return l;
        }

        private static FlowLayoutPanel VStack()
        {
            var f = new FlowLayoutPanel();
            f.Dock = DockStyle.Fill;
            f.FlowDirection = FlowDirection.TopDown;
            f.WrapContents = false;
            f.AutoScroll = true;
            return f;
        }
    }
}
