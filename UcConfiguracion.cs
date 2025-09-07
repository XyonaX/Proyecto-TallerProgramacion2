using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

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
            if (!IsDesigner()) HookEvents();
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
                if (_folderDlg.ShowDialog() == DialogResult.OK)
                    txtBackup.Text = _folderDlg.SelectedPath;
            };

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
    }
}
