using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows.Forms;
using Proyecto_Taller_2.Data.Repositories;
using Proyecto_Taller_2.Domain.Models;

namespace Proyecto_Taller_2.UI
{
    public partial class LoginForm : Form
    {
        private readonly UsuarioRepository _repo;

        public Usuario CurrentUser { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
            _repo = new UsuarioRepository();

            this.Shown += LoginForm_Shown;
            btnLogin.Click += btnLogin_Click;
        }

        private async void LoginForm_Shown(object sender, EventArgs e)
        {
            try
            {
                var cs = ConfigurationManager.ConnectionStrings["ERP"].ConnectionString;
                using (var cn = new SqlConnection(cs))
                {
                    await cn.OpenAsync();
                }
            }
            catch (Exception ex)
            {
                ShowError("No se pudo conectar a la base de datos.\n\n" + ex.Message);
            }
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                btnLogin.Enabled = false;
                ShowError("");

                var email = (txtEmail?.Text ?? "").Trim();
                var pass = txtPassword?.Text ?? "";

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(pass))
                {
                    ShowError("Ingrese correo y contraseña.");
                    return;
                }
                if (!email.Contains("@") || !email.Contains("."))
                {
                    ShowError("Formato de correo inválido.");
                    return;
                }

                var user = await Task.Run(() => _repo.Login(email, pass));
                if (user == null)
                {
                    ShowError("Credenciales inválidas o usuario inactivo.");
                    return;
                }

                CurrentUser = user;
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ShowError("Error durante el login:\n\n" + ex.Message);
            }
            finally
            {
                btnLogin.Enabled = true;
            }
        }


        private void btnTogglePwd_Click(object sender, EventArgs e)
        {
            if (txtPassword == null) return;
            txtPassword.PasswordChar = txtPassword.PasswordChar == '•' ? '\0' : '•';
            txtPassword.Focus();
            txtPassword.SelectionStart = txtPassword.TextLength;
        }

        private void ShowError(string message)
        {
            if (lblError != null) lblError.Text = message;
            else MessageBox.Show(message, "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        private void txtEmail_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
