using System.Collections.Generic;
using System.Threading.Tasks;

namespace Proyecto_Taller_2.Data.Repositories.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Proyecto_Taller_2.Domain.Models.Usuario?> GetByEmailAsync(string email);
        Task<Proyecto_Taller_2.Domain.Models.Usuario?> GetByIdAsync(int idUsuario);
        Task<bool> EmailExistsAsync(string email);
        Task<IEnumerable<Proyecto_Taller_2.Domain.Models.Usuario>> ListAsync(bool soloActivos = false);
        Task<int> CreateAsync(Proyecto_Taller_2.Domain.Models.Usuario u);
        Task<int> UpdateAsync(Proyecto_Taller_2.Domain.Models.Usuario u);
        Task<int> SetActivoAsync(int idUsuario, bool activo);
    }
}
