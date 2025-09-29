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
            if (user is null || string.IsNullOrEmpty(user.PasswordHash))
                return null;

            // BCrypt compara directamente
            bool ok = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            return ok ? user : null;
        }


        public async Task<int> RegisterAsync(string nombre, string apellido, string email, string password, int idRol)
        {
            // Requiere que IUsuarioRepository tenga EmailExistsAsync
            if (await _repo.EmailExistsAsync(email))
                throw new InvalidOperationException("El email ya está en uso.");

            // Generar hash al registrar usuario
            string hash = BCrypt.Net.BCrypt.HashPassword(password);

            var u = new Usuario
            {
                Nombre = nombre,
                Apellido = apellido,
                Email = email,
                IdRol = idRol,
                PasswordHash = hash,   // <-- string directo
                Activo = true
            };

            return await _repo.CreateAsync(u);
        }
    }
}
