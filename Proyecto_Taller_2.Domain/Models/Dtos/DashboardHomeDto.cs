using System;
using System.Collections.Generic;

namespace Proyecto_Taller_2.Domain.Models.Dtos
{
    public class DashboardHomeDto
    {
        // KPIs Superiores
        public decimal VentasTotales { get; set; }
        public decimal PorcentajeVentasVsAnterior { get; set; }
        public int OrdenesActivas { get; set; } // Ventas 'Pendiente' este mes
        public decimal PorcentajeOrdenesVsAnterior { get; set; }
        public decimal TicketPromedio { get; set; }
        public decimal PorcentajeTicketVsAnterior { get; set; }
        public int VendedoresActivos { get; set; } // Vendedores con al menos 1 venta este mes
        public int TotalVendedores { get; set; }
        public decimal ProductividadPromedio { get; set; }

        // Gráficos y Listas
        public List<VentaMensualDto> EvolucionVentas { get; set; }
        public List<TopVendedorDto> TopVendedores { get; set; }
        public List<InventarioCategoriaDto> InventarioPorCategoria { get; set; }
        public int CantidadStockBajo { get; set; }

        public DashboardHomeDto()
        {
            EvolucionVentas = new List<VentaMensualDto>();
            TopVendedores = new List<TopVendedorDto>();
            InventarioPorCategoria = new List<InventarioCategoriaDto>();
        }
    }

    public class VentaMensualDto
    {
        public string Mes { get; set; }
        public decimal TotalVenta { get; set; }
    }

    public class TopVendedorDto
    {
        public string Nombre { get; set; }
        public int CantidadVentas { get; set; }
        public decimal TotalFacturado { get; set; }
    }

    public class InventarioCategoriaDto
    {
        public string NombreCategoria { get; set; }
        public int StockActual { get; set; }
        public int StockEsperado { get; set; } // Una meta estimada para la barra de progreso
    }
}