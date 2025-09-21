using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Taller_2.Domain.Entities
{
    public class Producto
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = "";
        public string DescripcionProducto { get; set; } = "";
        public decimal PrecioProducto { get; set; }
        public bool Activo { get; set; }
    }
}
