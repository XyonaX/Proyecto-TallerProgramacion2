using System;

namespace Proyecto_Taller_2.Domain.Models
{
    public class InventarioItem
    {
        public int IdProducto { get; set; }
        public string Sku { get; set; } = "";
        public string NombreProducto { get; set; } = "";
        public string DescripcionProducto { get; set; } = "";
        public int IdCategoria { get; set; }
        public string Categoria { get; set; } = "";
        public string Ubicacion { get; set; } = "";
        public int Stock { get; set; }
        public int Minimo { get; set; }
        public decimal PrecioProducto { get; set; }
        public string Proveedor { get; set; } = "";
        public bool Activo { get; set; }
        public string Estado => Activo ? "Activo" : "Inactivo";
        public DateTime Actualizado { get; set; }
    }
}
