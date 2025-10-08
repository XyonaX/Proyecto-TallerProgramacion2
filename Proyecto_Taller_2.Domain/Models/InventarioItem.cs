using System;

namespace Proyecto_Taller_2.Domain.Models
{
    public class InventarioItem
    {
        public int IdProducto { get; set; }
        public string Sku { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";

        public byte CategoriaId { get; set; }          // 1=remera, 2=campera
        public string Categoria => CategoriaId == 2 ? "campera" : "remera";

        public string Ubicacion { get; set; } = "";
        public int Stock { get; set; }
        public int Minimo { get; set; }
        public decimal Precio { get; set; }            // map -> PrecioProducto
        public string Proveedor { get; set; } = "";    // map -> ProveedorProducto

        public bool Activo { get; set; }
        public string Estado => Activo ? "Activo" : "Inactivo";

        public DateTime Actualizado { get; set; }
    }
}
