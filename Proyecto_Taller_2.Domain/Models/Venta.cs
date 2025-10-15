using System;
using System.Collections.Generic;

namespace Proyecto_Taller_2.Domain.Models
{
    public class Venta
    {
        public int IdVenta { get; set; }
        public string NumeroVenta { get; set; } = "";
        public int IdUsuario { get; set; }
        public string NombreVendedor { get; set; } = "";
        public int IdCliente { get; set; }
        public string NombreCliente { get; set; } = "";
        public string EmpresaCliente { get; set; } = "";
        public DateTime FechaVenta { get; set; }
        public string Tipo { get; set; } = ""; // "Venta", "Cotización", "Devolución"
        public string Estado { get; set; } = ""; // "Pendiente", "Completada", "Cancelada"
        public decimal Total { get; set; }
        public string Observaciones { get; set; } = "";
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }

        public List<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    }
}