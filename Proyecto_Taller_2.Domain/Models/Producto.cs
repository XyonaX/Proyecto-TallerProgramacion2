using System;
using Proyecto_Taller_2.Domain.Enums;

namespace Proyecto_Taller_2.Domain.Models
{
    public class Producto
    {
        public int IdProducto { get; set; }
        public string Sku { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public CategoriaProducto CategoriaId { get; set; }   // 1 = Remera, 2 = Campera

       
        public string Categoria
        {
            get
            {
                return CategoriaId switch
                {
                    CategoriaProducto.Remera => "remera",
                    CategoriaProducto.Campera => "campera",
                    _ => "N/D"
                };
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    CategoriaId = CategoriaProducto.Remera;
                else
                {
                    string v = value.Trim().ToLower();
                    CategoriaId = (v == "campera" || v == "2")
                        ? CategoriaProducto.Campera
                        : CategoriaProducto.Remera;
                }
            }
        }

        public string Ubicacion { get; set; } = "";
        public int Stock { get; set; }
        public int Minimo { get; set; }
        public decimal Precio { get; set; }
        public string Proveedor { get; set; } = "";
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime Actualizado { get; set; }

        public string Estado => Activo ? "Activo" : "Inactivo";
    }
}
