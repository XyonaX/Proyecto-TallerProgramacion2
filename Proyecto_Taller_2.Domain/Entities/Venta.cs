using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Taller_2.Domain.Entities
{
    public class Venta
    {
        public int IdVenta { get; set; }
        public int IdUsuario { get; set; }
        public int IdCliente { get; set; }
        public DateTime FechaVenta { get; set; }

        // --- Columnas que faltaban ---
        public string NumeroVenta { get; set; }
        public string Tipo { get; set; }
        public string Estado { get; set; }

    
        public decimal Total { get; set; }

        public string Observaciones { get; set; }
        public DateTime FechaCreacion { get; set; }

        // Debe ser 'DateTime?' (nullable) porque puede ser NULL
        public DateTime? FechaActualizacion { get; set; }

        // --- Propiedades de Navegación (útiles) ---
        public virtual Cliente Cliente { get; set; }

       
    }
}
