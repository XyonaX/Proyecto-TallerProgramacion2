using System;
using System.Collections.Generic;

namespace Proyecto_Taller_2.Domain.Entities
{
    public class Cliente
    {
        public int IdCliente { get; set; }
        public string Tipo { get; set; } // "PF" o "PJ"
        public string? NombreCliente { get; set; } // Nullable si es PJ
        public string? ApellidoCliente { get; set; } // Nullable si es PJ
        public string? RazonSocial { get; set; } // Nullable si es PF
        public string? Direccion { get; set; }
        public string? CUIT { get; set; }
        public string? CUIL { get; set; }
        public DateTime FechaAlta { get; set; }
        public bool Activo { get; set; }

        // --- Propiedades de Navegación ---
        // Un cliente puede tener MUCHOS emails
        public virtual ICollection<ClienteEmail> Emails { get; set; } = new List<ClienteEmail>();

        // Un cliente puede tener MUCHOS teléfonos
        public virtual ICollection<ClienteTelefono> Telefonos { get; set; } = new List<ClienteTelefono>();

        // Un cliente puede tener MUCHAS ventas
        public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}