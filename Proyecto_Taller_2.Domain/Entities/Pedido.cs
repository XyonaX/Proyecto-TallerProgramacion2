using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Taller_2.Domain.Entities
{
    public class Pedido
    {
        public int IdPedido { get; set; }
        public int IdEstadoPedido { get; set; }
        public int IdUsuario { get; set; }
        public int IdCliente { get; set; }
        public DateTime FechaPedido { get; set; }

    }
}
