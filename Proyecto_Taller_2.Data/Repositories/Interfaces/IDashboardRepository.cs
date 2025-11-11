using Proyecto_Taller_2.Domain.Models.Dtos;
using System.Threading.Tasks;

namespace Proyecto_Taller_2.Data.Repositories.Interfaces
{
    public interface IDashboardRepository
    {
        Task<DashboardHomeDto> ObtenerDatosHomeAsync();
    }
}