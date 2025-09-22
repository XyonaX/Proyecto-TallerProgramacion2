using System;
using System.Data;            
using System.Windows.Forms;

using Proyecto_Taller_2.Domain.Entities;
using Proyecto_Taller_2.Data;
using Proyecto_Taller_2.Data.Repositories.DapperImpl;
using Proyecto_Taller_2.Services.Interfaces;
using Proyecto_Taller_2.Services.Impl;
using System.Threading.Tasks;

namespace Proyecto_Taller_2.UI
{
    public partial class LoginForm : Form   
    {
        private readonly ISqlConnectionFactory _factory;
        private readonly IAuthService _auth;

        public Usuario CurrentUser { get; private set; }

        public LoginForm()
        {
            InitializeComponent();


            _factory = new SqlConnectionFactory("ERP");
            var userRepo = new UsuarioRepository(_factory);
            _auth = new AuthService(userRepo);

            // Enter = click en Iniciar sesión
            this.AcceptButton = btnLogin;

            // Eventos
            this.Shown += LoginForm_Shown;
            btnLogin.Click += btnLogin_Click;
            ApplyTheme();
        }

        private readonly System.Drawing.Color Bg = System.Drawing.Color.FromArgb(236, 243, 236);   // #ECF3EC
        private readonly System.Drawing.Color Card = System.Drawing.Color.White;
        private readonly System.Drawing.Color Primary = System.Drawing.Color.FromArgb(34, 139, 58); // verde botón
        private readonly System.Drawing.Color PrimaryHover = System.Drawing.Color.FromArgb(28, 120, 50);
        private readonly System.Drawing.Color Title = System.Drawing.Color.FromArgb(34, 47, 34);
        private readonly System.Drawing.Color TextMuted = System.Drawing.Color.FromArgb(90, 104, 90);
        private readonly System.Drawing.Color Border = System.Drawing.Color.FromArgb(210, 224, 210);

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;

        private void ApplyTheme()
        {
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BackColor = Bg;

            if (pnlRoot != null) pnlRoot.BackColor = Bg;

            if (pnlCard != null)
            {
                pnlCard.BackColor = Card;
                pnlCard.Paint += (s, e) =>
                {
                    // “borde suave” de la tarjeta
                    using (var p = new System.Drawing.Pen(Border))
                    {
                        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        e.Graphics.DrawRectangle(p, 0, 0, pnlCard.Width - 1, pnlCard.Height - 1);
                    }
                };
            }

            if (lblTitle != null)
            {
                lblTitle.ForeColor = Title;
                lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 16F, System.Drawing.FontStyle.Bold);
            }
            if (lblSubtitle != null)
            {
                lblSubtitle.ForeColor = TextMuted;
                lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            }
            if (lblEmail != null) lblEmail.ForeColor = Title;
            if (lblPass  != null) lblPass.ForeColor  = Title;

            // Botón
            if (btnLogin != null)
            {
                btnLogin.BackColor = Primary;
                btnLogin.ForeColor = System.Drawing.Color.White;
                btnLogin.FlatStyle = FlatStyle.Flat;
                btnLogin.FlatAppearance.BorderSize = 0;
                btnLogin.Cursor = Cursors.Hand;
                btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = PrimaryHover;
                btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = Primary;
            }

            // Inputs
            StyleInput(txtEmail);
            StyleInput(txtPassword);

            // Placeholders
            if (txtEmail != null) SendMessage(txtEmail.Handle, EM_SETCUEBANNER, (IntPtr)1, "usuario@empresa.com");
            if (txtPassword != null) SendMessage(txtPassword.Handle, EM_SETCUEBANNER, (IntPtr)1, "••••••••");
        }

        private void StyleInput(TextBox tb)
        {
            if (tb == null) return;
            tb.BackColor = System.Drawing.Color.White;
            tb.ForeColor = Title;
            tb.BorderStyle = BorderStyle.FixedSingle;
        }

     

        // Ping de conexión SIN 'using var'
        private async void LoginForm_Shown(object sender, EventArgs e)
        {

            try
            {
                using (IDbConnection cn = _factory.Create())
                {
                    // No vuelvas a abrir la conexión, ya está abierta por SqlConnectionFactory
                    // cn.Open(); // Elimina esta línea
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo conectar a la base de datos.\n\n" + ex.Message,
                    "Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                btnLogin.Enabled = false;

                var email = (txtEmail.Text ?? "").Trim();
                var pass = txtPassword.Text ?? "";

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(pass))
                {
                    MessageBox.Show("Ingrese correo y contraseña.", "Login",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (!email.Contains("@") || !email.Contains("."))
                {
                    MessageBox.Show("Formato de correo inválido.", "Login",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }


                var user = await _auth.LoginAsync(email, pass);
                if (user == null)
                {
                    MessageBox.Show("Credenciales inválidas o usuario inactivo.", "Login",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                CurrentUser = user;
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error durante el login:\n\n" + ex.Message,
                    "Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnLogin.Enabled = true;
            }
        }
    }
}
