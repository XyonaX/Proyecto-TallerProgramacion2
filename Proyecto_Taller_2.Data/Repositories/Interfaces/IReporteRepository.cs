using Proyecto_Taller_2.Domain.Models.Dtos;
using System;
using System.Threading.Tasks;

namespace Proyecto_Taller_2.Data.Repositories.Interfaces
{
    public interface IReporteRepository
    {
        Task<DashboardReporteDto> ObtenerDatosDashboardAsync(DateTime fechaInicio, DateTime fechaFin);

        // AGREGA ESTOS TRES MÉTODOS:
        Task<string> ObtenerStockBajoAsync();
        Task<string> ObtenerTopProductosAsync();
        Task<string> ObtenerVentasHoyAsync();
    }
}