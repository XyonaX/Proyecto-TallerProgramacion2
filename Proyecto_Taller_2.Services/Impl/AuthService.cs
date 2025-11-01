using System;
using System.Threading.Tasks;
using Proyecto_Taller_2.Domain.Models;               // <-- usar Models, no Entities
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
            if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash)) return null;

            // En Models.PasswordHash es string (bcrypt)
            bool ok = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            return ok ? user : null;
        }


        public async Task<int> RegisterAsync(string nombre, string apellido, string email, string password, int idRol)
        {
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
                PasswordHash = hash,      // <-- string, no byte[]
                Activo = true,
                FechaAlta = DateTime.Now  // opcional, si tu INSERT no lo pone
            };

            return await _repo.CreateAsync(u);
        }
    }
}
