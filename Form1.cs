using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Proyecto_Taller_2.Controls;
using Proyecto_Taller_2.Domain.Entities;


namespace Proyecto_Taller_2.UI
{
    public partial class Form1 : Form
    {
        // NO declares acá los controles del Designer (ya están en Form1.Designer.cs)

        private Button _btnActivo;
        private readonly Dictionary<Type, UserControl> _cache = new Dictionary<Type, UserControl>();

        // Paleta de colores
        private readonly Color Sidebar = Color.FromArgb(236, 243, 236);
        private readonly Color Hover = Color.FromArgb(220, 232, 220);
        private readonly Color Activo = Color.FromArgb(201, 222, 201);
        private readonly Color Txt = Color.FromArgb(34, 47, 34);

        private readonly Usuario _currentUser;

        public Form1()
        {
            InitializeComponent();

            if (IsDesigner()) return; // Evita correr lógica en el diseñador

            initUi();

            
        }

        public Form1(Usuario user)
        {
            InitializeComponent();
            _currentUser = user;
            if (IsDesigner()) return;
            initUi();

            if (lblTitulo != null && _currentUser != null)
                lblTitulo.Text = $"Bienvenido, {_currentUser.Nombre} {_currentUser.Apellido}";
        }

        private void initUi()
        {
            // Hover
            WireHover(btnVentas);
            WireHover(btnInventario);
            WireHover(btnUsuarios);
            WireHover(btnConfiguracion);
            WireHover(btnDashboard);
            WireHover(btnReportes);

            // Clicks (aunque el Designer tenga alguno, no pasa nada)
            btnVentas.Click += btnVentas_Click;
            btnInventario.Click += btnInventario_Click;
            btnUsuarios.Click += btnUsuarios_Click;
            btnConfiguracion.Click += btnConfiguracion_Click;
            btnDashboard.Click += btnDashboard_Click;
            btnReportes.Click += btnReportes_Click;

            // Pantalla inicial
            btnInventario.PerformClick();

            btnDashboard.Text = "Dashboard";

        }

        private static bool IsDesigner()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime
                   || Process.GetCurrentProcess().ProcessName.Equals("devenv", StringComparison.OrdinalIgnoreCase);
        }

        private void WireHover(Button b)
        {
            if (b == null) return;
            b.FlatAppearance.BorderSize = 0;
            b.BackColor = Sidebar;
            b.ForeColor = Txt;

            b.MouseEnter += (s, e) => { if (b != _btnActivo) b.BackColor = Hover; };
            b.MouseLeave += (s, e) => { if (b != _btnActivo) b.BackColor = Sidebar; };
        }

        private void Activar(Button b)
        {
            if (_btnActivo != null) _btnActivo.BackColor = Sidebar;
            _btnActivo = b;
            if (_btnActivo != null) _btnActivo.BackColor = Activo;
        }

        private T GetOrCreate<T>() where T : UserControl, new()
        {
            if (!_cache.TryGetValue(typeof(T), out var uc))
            {
                uc = new T { Dock = DockStyle.Fill };
                _cache[typeof(T)] = uc;
            }
            return (T)uc;
        }

        private void Mostrar(UserControl uc, string titulo)
        {
            if (uc == null) return;

            if (lblTitulo != null) lblTitulo.Text = titulo ?? "";
            if (pnlContent == null) return;

            pnlContent.SuspendLayout();
            pnlContent.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(uc);
            pnlContent.ResumeLayout();
        }

        // === Clicks de menú ===
        private void btnDashboard_Click(object sender, EventArgs e)
        {
            Activar(btnDashboard);
            Mostrar(GetOrCreate<UcDashboardAdmin>(), "Dashboard Administrativo");
        }

        private void btnReportes_Click(object sender, EventArgs e)
        {
            Activar(btnReportes);
            Mostrar(GetOrCreate<UcReportes>(), "Reportes");
        }

        private void btnVentas_Click(object sender, EventArgs e)
        {
            Activar(btnVentas);
            Mostrar(GetOrCreate<UcVentas>(), "Ventas");
        }

        private void btnInventario_Click(object sender, EventArgs e)
        {
            Activar(btnInventario);
            Mostrar(GetOrCreate<UcInventario>(), "Inventario");
        }

        private void btnUsuarios_Click(object sender, EventArgs e)
        {
            Activar(btnUsuarios);
            Mostrar(GetOrCreate<UcUsuarios>(), "Usuarios");
        }

        private void btnConfiguracion_Click(object sender, EventArgs e)
        {
            Activar(btnConfiguracion);
            Mostrar(GetOrCreate<UcConfiguracion>(), "Configuración");
        }

        private void pnlContent_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
