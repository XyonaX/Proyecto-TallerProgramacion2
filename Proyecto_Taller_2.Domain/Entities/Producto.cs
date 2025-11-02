using System;

namespace Proyecto_Taller_2.Domain.Entities
{
    public class Producto
    {
        public int IdProducto { get; set; }

        // --- Propiedades que faltaban ---
        // (Asegúrate que los nombres coincidan con tu tabla SQL)
        public string Sku { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int IdCategoria { get; set; }
        public string Ubicacion { get; set; }
        public int Stock { get; set; }
        public int Minimo { get; set; }
        public decimal Precio { get; set; }
        public string Proveedor { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime? Actualizado { get; set; }
    }
}