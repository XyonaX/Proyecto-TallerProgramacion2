using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Proyecto_Taller_2.Services;

namespace Proyecto_Taller_2
{
    public partial class UcConfiguracion : UserControl
    {
        public event Action<AppSettings> SettingsSaved;

        private readonly ColorDialog _colorDlg = new ColorDialog { FullOpen = true, AnyColor = true };
        private readonly FolderBrowserDialog _folderDlg = new FolderBrowserDialog();

        public UcConfiguracion()
        {
            InitializeComponent();
            if (!IsDesigner()) 
            {
                HookEvents();
                ActualizarInfoBackup();
            }
            LoadSettingsToUI();
        }

        private static bool IsDesigner()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime
                   || Process.GetCurrentProcess().ProcessName.Equals("devenv", StringComparison.OrdinalIgnoreCase);
        }

        private void HookEvents()
        {
            btnColorPrimario.Click += (s, e) =>
            {
                _colorDlg.Color = SettingsService.Current.ColorPrimario;
                if (_colorDlg.ShowDialog() == DialogResult.OK)
                    btnColorPrimario.BackColor = _colorDlg.Color;
            };

            btnElegirBackup.Click += (s, e) =>
            {
                // Primero, ofrecer usar carpeta recomendada
                var carpetaRecomendada = BackupService.ObtenerCarpetaRecomendada();
                var result = MessageBox.Show(
                    $"💡 Se recomienda usar una carpeta que SQL Server pueda acceder sin problemas.\n\n" +
                    $"Carpeta recomendada:\n{carpetaRecomendada}\n\n" +
                    "¿Deseas usar esta carpeta recomendada?\n\n" +
                    "(Selecciona 'No' para elegir manualmente otra ubicación)",
                    "Configurar Carpeta de Backups",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    txtBackup.Text = carpetaRecomendada;
                    ActualizarInfoBackup();
                    MessageBox.Show("✅ Carpeta configurada correctamente.\n\nYa puedes crear backups.", 
                        "Configuración Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (result == DialogResult.No)
                {
                    // Permitir selección manual
                    if (_folderDlg.ShowDialog() == DialogResult.OK)
                    {
                        var validacion = BackupService.ValidarCarpetaBackup(_folderDlg.SelectedPath);
                        if (!validacion.esValida)
                        {
                            var usarRecomendada = MessageBox.Show(
                                validacion.mensaje + "\n\n¿Deseas usar la carpeta recomendada en su lugar?",
                                "Carpeta no válida",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning);

                            if (usarRecomendada == DialogResult.Yes)
                            {
                                txtBackup.Text = carpetaRecomendada;
                                ActualizarInfoBackup();
                            }
                            return;
                        }

                        txtBackup.Text = _folderDlg.SelectedPath;
                        ActualizarInfoBackup();
                    }
                }
            };

            btnCrearBackup.Click += BtnCrearBackup_Click;
            btnRestaurarBackup.Click += BtnRestaurarBackup_Click;

            btnGuardar.Click += (s, e) => { AppSettings s2; if (TryBuildSettings(out s2)) SaveAndApply(s2); };
            btnAplicar.Click += (s, e) => { AppSettings s2; if (TryBuildSettings(out s2)) ApplyOnly(s2); };
            btnCancelar.Click += (s, e) => LoadSettingsToUI();

            btnExportar.Click += (s, e) =>
            {
                using (var sfd = new SaveFileDialog { Filter = "JSON|*.json", FileName = "Proyecto_Taller_2.settings.json" })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        SettingsService.ExportTo(sfd.FileName);
                        MessageBox.Show("Configuración exportada.", "Proyecto_Taller_2", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            };

            btnImportar.Click += (s, e) =>
            {
                using (var ofd = new OpenFileDialog { Filter = "JSON|*.json" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        SettingsService.ImportFrom(ofd.FileName);
                        LoadSettingsToUI();
                        ThemeHelper.ApplyTheme(this, SettingsService.Current);
                        if (SettingsSaved != null) SettingsSaved(SettingsService.Current);
                    }
                }
            };

            btnReset.Click += (s, e) =>
            {
                if (MessageBox.Show("¿Restablecer valores por defecto?", "Proyecto_Taller_2",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    SettingsService.Reset();
                    LoadSettingsToUI();
                    ThemeHelper.ApplyTheme(this, SettingsService.Current);
                    if (SettingsSaved != null) SettingsSaved(SettingsService.Current);
                }
            };
        }

        private void LoadSettingsToUI()
        {
            if (SettingsService.Current == null) SettingsService.Load();
            var s = SettingsService.Current;

            cbTema.SelectedItem = s.Tema;
            if (cbTema.SelectedIndex < 0) cbTema.SelectedIndex = 0;

            btnColorPrimario.BackColor = s.ColorPrimario;
            nudFont.Value = Math.Max(nudFont.Minimum, Math.Min(nudFont.Maximum, s.FontSize));
            chkCompacto.Checked = s.ModoCompacto;

            cbIdioma.SelectedItem = s.Idioma;
            if (cbIdioma.SelectedIndex < 0) cbIdioma.SelectedIndex = 0;

            cbFecha.SelectedItem = s.FormatoFecha;
            if (cbFecha.SelectedIndex < 0) cbFecha.SelectedIndex = 0;

            cbMoneda.SelectedItem = s.FormatoMoneda;
            if (cbMoneda.SelectedIndex < 0) cbMoneda.SelectedIndex = 0;

            txtBackup.Text = s.CarpetaBackups;
            chkAutoBackup.Checked = s.AutoBackup;

            ThemeHelper.ApplyTheme(this, s);
        }

        private bool TryBuildSettings(out AppSettings s)
        {
            s = new AppSettings();
            try
            {
                s.Tema = cbTema.SelectedItem != null ? cbTema.SelectedItem.ToString() : "Sistema";
                s.ColorPrimario = btnColorPrimario.BackColor;
                s.FontSize = (int)nudFont.Value;
                s.ModoCompacto = chkCompacto.Checked;

                s.Idioma = cbIdioma.SelectedItem != null ? cbIdioma.SelectedItem.ToString() : "es-AR";
                s.FormatoFecha = cbFecha.SelectedItem != null ? cbFecha.SelectedItem.ToString() : "dd/MM/yyyy";
                s.FormatoMoneda = cbMoneda.SelectedItem != null ? cbMoneda.SelectedItem.ToString() : "es-AR";

                s.CarpetaBackups = txtBackup.Text != null ? txtBackup.Text.Trim() : "";
                s.AutoBackup = chkAutoBackup.Checked;

                var test = new CultureInfo(s.FormatoMoneda);
                if (!string.IsNullOrEmpty(s.CarpetaBackups) && !Directory.Exists(s.CarpetaBackups))
                    Directory.CreateDirectory(s.CarpetaBackups);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Revisá los datos de configuración.\n\n" + ex.Message,
                    "Proyecto_Taller_2", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        private void SaveAndApply(AppSettings s)
        {
            SettingsService.Save(s);
            ThemeHelper.ApplyTheme(this, s);
            if (SettingsSaved != null) SettingsSaved(s);
            MessageBox.Show("Configuración guardada.", "Proyecto_Taller_2",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ApplyOnly(AppSettings s)
        {
            ThemeHelper.ApplyTheme(this, s);
            if (SettingsSaved != null) SettingsSaved(s);
        }

        private void ActualizarInfoBackup()
        {
            if (lblUltimoBackup == null) return;

            try
            {
                var info = BackupService.GetLastBackupInfo();
                
                if (info.HasBackup)
                {
                    lblUltimoBackup.Text = $"📁 Último backup: {info.FechaUltimoBackup:dd/MM/yyyy HH:mm} ({info.GetTiempoTranscurrido()}) - {info.NombreArchivo}";
                    lblUltimoBackup.ForeColor = Color.FromArgb(34, 139, 34); // Verde
                }
                else
                {
                    lblUltimoBackup.Text = "⚠️ No se han realizado backups todavía";
                    lblUltimoBackup.ForeColor = Color.FromArgb(200, 100, 0); // Naranja
                }
            }
            catch (Exception ex)
            {
                lblUltimoBackup.Text = $"❌ Error al obtener información: {ex.Message}";
                lblUltimoBackup.ForeColor = Color.Red;
            }
        }

        private void BtnCrearBackup_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtBackup.Text))
                {
                    MessageBox.Show("Por favor, selecciona una carpeta para los backups primero.", 
                        "Carpeta no configurada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    "¿Deseas crear un backup de la base de datos?\n\nEsto puede tardar algunos minutos dependiendo del tamaño de la base de datos.",
                    "Confirmar Backup",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes) return;

                this.Cursor = Cursors.WaitCursor;
                btnCrearBackup.Enabled = false;

                // Guardar la configuración actual si es necesaria
                var tempSettings = SettingsService.Current;
                if (tempSettings.CarpetaBackups != txtBackup.Text.Trim())
                {
                    tempSettings.CarpetaBackups = txtBackup.Text.Trim();
                    SettingsService.Save(tempSettings);
                }

                string mensaje;
                bool exito = BackupService.CrearBackup(out mensaje);

                this.Cursor = Cursors.Default;
                btnCrearBackup.Enabled = true;

                if (exito)
                {
                    ActualizarInfoBackup();
                    MessageBox.Show(mensaje, "Backup Exitoso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(mensaje, "Error en Backup", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                btnCrearBackup.Enabled = true;
                MessageBox.Show($"Error inesperado al crear backup: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRestaurarBackup_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtBackup.Text) || !Directory.Exists(txtBackup.Text))
                {
                    MessageBox.Show("Por favor, configura una carpeta de backups válida primero.", 
                        "Carpeta no configurada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (var ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Archivos de Backup (*.bak)|*.bak|Todos los archivos (*.*)|*.*";
                    ofd.Title = "Seleccionar Backup a Restaurar";
                    ofd.InitialDirectory = txtBackup.Text;

                    if (ofd.ShowDialog() != DialogResult.OK) return;

                    var result = MessageBox.Show(
                        "⚠️ ADVERTENCIA: Restaurar un backup reemplazará TODOS los datos actuales de la base de datos.\n\n" +
                        "Esta acción NO se puede deshacer.\n\n" +
                        "¿Estás seguro de que deseas continuar?",
                        "Confirmar Restauración",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result != DialogResult.Yes) return;

                    this.Cursor = Cursors.WaitCursor;
                    btnRestaurarBackup.Enabled = false;

                    string mensaje;
                    bool exito = BackupService.RestaurarBackup(ofd.FileName, out mensaje);

                    this.Cursor = Cursors.Default;
                    btnRestaurarBackup.Enabled = true;

                    if (exito)
                    {
                        MessageBox.Show(mensaje + "\n\nLa aplicación se cerrará para aplicar los cambios.", 
                            "Restauración Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Application.Exit();
                    }
                    else
                    {
                        MessageBox.Show(mensaje, "Error en Restauración", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                btnRestaurarBackup.Enabled = true;
                MessageBox.Show($"Error inesperado al restaurar backup: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
