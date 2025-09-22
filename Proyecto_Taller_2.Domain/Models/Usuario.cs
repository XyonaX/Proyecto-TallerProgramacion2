using System;

namespace Proyecto_Taller_2.Domain.Models
{
    public class Usuario
    {
        // En BD: IdUsuario
        public int Dni { get; set; }

        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string Email { get; set; } = "";

        // La UI los usa
        public string? Telefono { get; set; }      // hoy no está en la BD (opcional)
        public string? Password { get; set; }      // solo para alta/edición (se hashea)

        // En BD tenés FechaAlta; lo mapeamos aquí
        public DateTime? FechaNacimiento { get; set; }

        // En BD: Activo (bit)
        public bool Estado { get; set; }

        // En BD: IdRol
        public int IdRol { get; set; }

        // JOIN
        public string RolNombre { get; set; } = "";

        public string NombreCompleto => $"{Apellido}, {Nombre}".Trim(',', ' ');
    }
}
