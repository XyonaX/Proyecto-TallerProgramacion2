using System;

namespace Proyecto_Taller_2.Domain.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }     // PK identidad
        public int Dni { get; set; }

        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Telefono { get; set; }

        public string? PasswordHash { get; set; } // lectura
        public string? Password { get; set; }     // escritura (si null o "", no cambia)

        public DateTime? FechaAlta { get; set; }  // en BD existe
        public bool Activo { get; set; }

        public int IdRol { get; set; }
        public string RolNombre { get; set; } = "";

        public string NombreCompleto => $"{Apellido}, {Nombre}".Trim(',', ' ');
    }
}
