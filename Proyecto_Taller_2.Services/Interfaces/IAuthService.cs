using Proyecto_Taller_2.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Taller_2.Services.Interfaces
{
    public interface IAuthService
    {
        Task<Usuario?> LoginAsync(string email, string password);
        Task<int> RegisterAsync(string nombre, string apellido, string email, string password, int idRol);
    }
}
