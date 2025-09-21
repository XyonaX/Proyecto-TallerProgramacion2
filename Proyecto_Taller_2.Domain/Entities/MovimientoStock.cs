using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Taller_2.Domain.Entities
{
    public class MovimientoStock
    {
        public long IdMov { get; set; }
        public int IdProducto { get; set; }
        public DateTime Fecha { get; set; }
        public char Tipo { get; set; }           // 'E','S','A'
        public int Cantidad { get; set; }
        public string Origen { get; set; } = ""; // Venta, Pedido, Ajuste
        public int? OrigenId { get; set; }
        public string Observacion { get; set; } = "";
    }
}
