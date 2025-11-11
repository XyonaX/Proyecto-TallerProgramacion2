using System;
using System.Collections.Generic;

namespace Proyecto_Taller_2.Domain.Models.Dtos
{
    public class DashboardReporteDto
    {
        // KPIs Principales
        public decimal IngresosMensuales { get; set; }
        public decimal PorcentajeVariacionIngreso { get; set; }
        public decimal MetaMensual { get; set; }
        public decimal PorcentajeMeta { get; set; }
        public int ClientesActivos { get; set; }
        public int StockCritico { get; set; }

        // KPIs Medios
        public decimal TotalVentasPeriodo { get; set; }
        public int CantidadVentasPeriodo { get; set; } // <--- ESTA ES LA NUEVA
        public decimal TasaConversion { get; set; }
        public decimal MargenBruto { get; set; }
        public decimal ValorTotalInventario { get; set; }
        public int CantidadProductosActivos { get; set; }
        public int NuevosClientesEsteMes { get; set; }

        // Lista de reportes
        public List<ReporteRecienteDto> ReportesRecientes { get; set; }

        public DashboardReporteDto()
        {
            ReportesRecientes = new List<ReporteRecienteDto>();
        }
    }

    public class ReporteRecienteDto
    {
        public string FechaGeneracion { get; set; }
        public string Fecha { get; set; }
        public string Estado { get; set; }
    }
}