using System;

namespace Proyecto_Taller_2.Domain.Models
{
    public class InventarioItem
    {
        public int IdProducto { get; set; }
        public string Sku { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";

        public int CategoriaId { get; set; }          // ID de la categoría
        
        // Propiedad calculada básica para compatibilidad (se puede mejorar cargando desde BD)
        public string Categoria 
        { 
            get
            {
                // Mapeo básico para compatibilidad con el código existente
                switch (CategoriaId)
                {
                    case 1:
                        return "remera";
                    case 2:
                        return "campera";
                    case 3:
                        return "pantalon";
                    case 4:
                        return "calzado";
                    default:
                        return "general";
                }
            }
        }

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
