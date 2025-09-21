using Proyecto_Taller_2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Taller_2.Data.Repositories.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<int> CreateAsync(Usuario u);
        Task<Usuario?> GetByIdAsync(int id);
    }
}
