using System;
using System.Drawing;

namespace Proyecto_Taller_2
{
    [Serializable]
    public class AppSettings
    {
        public string Tema { get; set; } = "Sistema";        // "Claro" | "Oscuro" | "Sistema"
        public int FontSize { get; set; } = 10;               // 8..16
        public bool ModoCompacto { get; set; } = false;
        public string Idioma { get; set; } = "es-AR";         // "es-AR" | "en-US"
        public string FormatoFecha { get; set; } = "dd/MM/yyyy";
        public string FormatoMoneda { get; set; } = "es-AR";  // cultura para moneda
        public Color ColorPrimario { get; set; } = Color.FromArgb(34, 139, 94);

        public bool AutoBackup { get; set; } = false;
        public string CarpetaBackups { get; set; } = string.Empty;

        // versión para futuras migraciones
        public int Version { get; set; } = 1;
    }
}
