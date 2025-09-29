using Proyecto_Taller_2.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
// Alias para el modelo de dominio:
using DomainUsuario = Proyecto_Taller_2.Domain.Models.Usuario;

namespace Proyecto_Taller_2.UI
{
    public partial class Form1 : Form
    {
        private Button _btnActivo;
        private readonly Dictionary<Type, UserControl> _cache = new Dictionary<Type, UserControl>();

        // Paleta de colores
        private readonly Color Sidebar = Color.FromArgb(236, 243, 236);
        private readonly Color Hover = Color.FromArgb(220, 232, 220);
        private readonly Color Activo = Color.FromArgb(201, 222, 201);
        private readonly Color Txt = Color.FromArgb(34, 47, 34);

        private readonly DomainUsuario _currentUser;

        // === Constructor vacío -> necesario para el Diseñador ===
        public Form1()
        {
            InitializeComponent();
            if (IsDesigner()) return; // Evita correr lógica en el diseñador
            initUi();
        }

        // === Único constructor con usuario ===
        public Form1(DomainUsuario user)
        {
            InitializeComponent();
            _currentUser = user ?? throw new ArgumentNullException(nameof(user));

            if (IsDesigner()) return;

            initUi();

            if (lblTitulo != null)
                lblTitulo.Text = $"Bienvenido, {_currentUser.Nombre} {_currentUser.Apellido}";

            ConfigureMenuAccess();

            // Pantalla inicial según el rol
            switch (_currentUser.IdRol)
            {
                case 1: // Administrador
                    btnDashboard.PerformClick();
                    break;
                case 2: // Vendedor
                    btnVentas.PerformClick();
                    break;
                case 3: // Depósito
                    btnInventario.PerformClick();
                    break;
                default:
                    btnDashboard.PerformClick();
                    break;
            }
        }

        private void ConfigureMenuAccess()
        {
            // Por defecto, ocultar todos los botones
            btnDashboard.Visible = false;
            btnVentas.Visible = false;
            btnInventario.Visible = false;
            btnUsuarios.Visible = false;
            btnConfiguracion.Visible = false;
            btnReportes.Visible = false;
            btnClientes.Visible = false;
            btnProductos.Visible = false;

            if (_currentUser == null) return;

            // Configurar acceso según el rol
            switch (_currentUser.IdRol)
            {
                case 1: // Administrador
                    btnDashboard.Visible = true;
                    btnVentas.Visible = true;
                    btnInventario.Visible = true;
                    btnUsuarios.Visible = true;
                    btnConfiguracion.Visible = true;
                    btnReportes.Visible = true;
                    btnClientes.Visible = true;
                    btnProductos.Visible = true;
                    break;

                case 2: // Vendedor
                    btnVentas.Visible = true;
                    btnClientes.Visible = true;
                    break;

                case 3: // Depósito
                    btnInventario.Visible = true;
                    btnProductos.Visible = true;
                    break;
            }
        }

        private void initUi()
        {
            // Hovers
            WireHover(btnVentas);
            WireHover(btnInventario);
            WireHover(btnUsuarios);
            WireHover(btnConfiguracion);
            WireHover(btnDashboard);
            WireHover(btnReportes);
            WireHover(btnClientes);
            WireHover(btnProductos);
            WireHover(btnLogout);

            // Clicks
            btnVentas.Click += btnVentas_Click;
            btnInventario.Click += btnInventario_Click;
            btnUsuarios.Click += btnUsuarios_Click;
            btnConfiguracion.Click += btnConfiguracion_Click;
            btnDashboard.Click += btnDashboard_Click;
            btnReportes.Click += btnReportes_Click;
            btnClientes.Click += btnClientes_Click;
            btnProductos.Click += btnProductos_Click;
            btnLogout.Click += btnLogout_Click;
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

        private void btnClientes_Click(object sender, EventArgs e)
        {
            Activar(btnClientes);
            Mostrar(GetOrCreate<UcClientes>(), "Gestión de Clientes");
        }

        private void btnProductos_Click(object sender, EventArgs e)
        {
            Activar(btnProductos);
            Mostrar(GetOrCreate<UcProductos>(), "Gestión de Productos");
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "¿Está seguro que desea cerrar sesión?",
                "Cerrar Sesión",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                this.Hide();
                using (var login = new LoginForm())
                {
                    if (login.ShowDialog() == DialogResult.OK && login.CurrentUser != null)
                    {
                        var main = new Form1(login.CurrentUser);
                        main.ShowDialog();
                    }
                }
                this.Close();
            }
        }
    }
}
