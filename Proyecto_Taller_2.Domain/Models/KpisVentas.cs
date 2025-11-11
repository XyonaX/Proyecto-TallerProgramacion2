using System.Collections.Generic;

namespace Proyecto_Taller_2.Domain.Models
{
    /// <summary>
    /// Clase para KPIs específicos de ventas
    /// </summary>
    public class KpisVentas
    {
        public decimal VentasDelMes { get; set; }
        public int TotalOrdenes { get; set; }
        public decimal TicketPromedio { get; set; }
        public int TotalCotizaciones { get; set; }
        public int CotizacionesPendientes { get; set; }
        public decimal PorcentajeVsAnterior { get; set; }
        public decimal PorcentajeOrdenesAnterior { get; set; }
        public decimal PorcentajeTicketAnterior { get; set; }
        public decimal PorcentajeCotizacionesAnterior { get; set; }
    }

    /// <summary>
    /// Clase para KPIs globales (solo administradores)
    /// </summary>
    public class KpisGlobales
    {
        public int VendedoresActivos { get; set; }
        public int ClientesUnicos { get; set; }
        public decimal VentasTotales { get; set; }
        public int NumeroVentas { get; set; }
        public int TotalVendedores { get; set; }
        public decimal ProductividadPromedio { get; set; }
        public List<TopVendedor> TopVendedores { get; set; } = new List<TopVendedor>();
    }

    /// <summary>
    /// Clase para el ranking de vendedores
    /// </summary>
    public class TopVendedor
    {
        public string Nombre { get; set; } = "";
        public decimal TotalVentas { get; set; }
        public int NumeroVentas { get; set; }
        public decimal TicketPromedio => NumeroVentas > 0 ? TotalVentas / NumeroVentas : 0;
    }
}