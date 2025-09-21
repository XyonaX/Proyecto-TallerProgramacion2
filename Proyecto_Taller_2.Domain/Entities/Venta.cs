using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Taller_2.Domain.Entities
{
    public class Venta
    {
        public int IdVenta { get; set;  }
        public int IdUsuario { get; set; }
        public int IdCliente { get; set; }
        public DateTime FechaVenta { get; set; }
    }
}
