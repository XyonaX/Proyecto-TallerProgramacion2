using System;
using Proyecto_Taller_2.Domain.Enums;

namespace Proyecto_Taller_2.Domain.Models
{
    public class Producto
    {
        public object Costo;

        public int IdProducto { get; set; }
        public string? Sku { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        
        // Relación con Categoria
        public int IdCategoria { get; set; }
        
        // Propiedades de navegación (opcional, para cuando uses Entity Framework)
        public virtual Categoria Categoria 
        { 
            get; 
            set; 
        }
        
        // Propiedad calculada para obtener el nombre de la categoría
        public string CategoriaNombre { get; set; } = "";

        public string Ubicacion { get; set; } = "";
        public int Stock { get; set; }
        public int Minimo { get; set; }
        public decimal Precio { get; set; }
        public string Proveedor { get; set; } = "";
        public bool Activo { get; set; } = true;
        public DateTime FechaAlta { get; set; } = DateTime.Now;
        public DateTime Actualizado { get; set; } = DateTime.Now;

        public string Estado => Activo ? "Activo" : "Inactivo";
        
        // Propiedades calculadas para estados de stock
        public string EstadoStock 
        { 
            get 
            {
                if (Stock <= 0) return "Sin Stock";
                if (Stock <= Minimo) return "Stock Bajo";
                return "Disponible";
            }
        }
        
        public bool TieneBajoStock => Stock <= Minimo && Stock > 0;
        public bool SinStock => Stock <= 0;
        
        // Helper para asignar categoria por string (compatibilidad con código legacy)
        public void SetCategoriaByName(string categoriaName)
        {
            if (string.IsNullOrEmpty(categoriaName))
            {
                IdCategoria = 1; // Default: remera
                return;
            }

            string categoria = categoriaName.Trim().ToLower();
            switch (categoria)
            {
                case "campera":
                case "2":
                    IdCategoria = 2;
                    break;
                case "pantalon":
                case "pantalón":
                case "3":
                    IdCategoria = 3;
                    break;
                case "calzado":
                case "4":
                    IdCategoria = 4;
                    break;
                case "remera":
                case "1":
                default:
                    IdCategoria = 1;
                    break;
            }
        }
    }
}
