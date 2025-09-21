using System;
using System.Text;
using System.Threading.Tasks;
using Proyecto_Taller_2.Domain.Entities;
using Proyecto_Taller_2.Services.Interfaces;
using Proyecto_Taller_2.Data.Repositories.Interfaces;

namespace Proyecto_Taller_2.Services.Impl
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _repo;
        public AuthService(IUsuarioRepository repo) => _repo = repo;

        public async Task<Usuario?> LoginAsync(string email, string password)
        {
            var user = await _repo.GetByEmailAsync(email);
            if (user is null || user.PasswordHash is null) return null;

            var stored = Encoding.UTF8.GetString(user.PasswordHash); // hash bcrypt como texto
            var ok = BCrypt.Net.BCrypt.Verify(password, stored);
            return ok ? user : null;
        }

        public async Task<int> RegisterAsync(string nombre, string apellido, string email, string password, int idRol)
        {
            // Requiere que IUsuarioRepository tenga EmailExistsAsync
            if (await _repo.EmailExistsAsync(email))
                throw new InvalidOperationException("El email ya está en uso.");

            var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

            var u = new Usuario
            {
                Nombre = nombre,
                Apellido = apellido,
                Email = email,
                IdRol = idRol,
                PasswordHash = Encoding.UTF8.GetBytes(hash),
                Activo = true
            };

            return await _repo.CreateAsync(u);
        }
    }
}
