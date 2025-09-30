using System;

namespace Proyecto_Taller_2.Domain.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public int Dni { get; set; }

        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string Email { get; set; } = "";
        public string Telefono { get; set; } = null;

        public string PasswordHash { get; set; } = null;   // persistido
        public string Password { get; set; } = null;        // solo para alta/edición (no persistir)

        public DateTime FechaAlta { get; set; }
        public bool Activo { get; set; }                    // ⚠️ este es el que usa la UI

        public int IdRol { get; set; }
        public string RolNombre { get; set; } = "";

        public string NombreCompleto
            => $"{Apellido ?? ""}, {Nombre ?? ""}".Trim(',', ' ').Trim();
    }
}
