using Dapper;
using Proyecto_Taller_2.Data.Repositories.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using UsuarioModel = Proyecto_Taller_2.Domain.Models.Usuario;

namespace Proyecto_Taller_2.Data.Repositories.DapperImpl
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly ISqlConnectionFactory _factory;
        public UsuarioRepository(ISqlConnectionFactory factory) => _factory = factory;

        public async Task<UsuarioModel?> GetByEmailAsync(string email)
        {
            using IDbConnection cn = _factory.Create();
            const string sql = @"SELECT TOP 1 * FROM Usuario WHERE Email=@Email";
            return await cn.QueryFirstOrDefaultAsync<UsuarioModel>(sql, new { Email = email });
        }

        public async Task<UsuarioModel?> GetByIdAsync(int idUsuario)
        {
            using IDbConnection cn = _factory.Create();
            const string sql = @"SELECT * FROM Usuario WHERE IdUsuario=@IdUsuario";
            return await cn.QueryFirstOrDefaultAsync<UsuarioModel>(sql, new { IdUsuario = idUsuario });
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            using IDbConnection cn = _factory.Create();
            const string sql = @"SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM Usuario WHERE Email=@Email) THEN 1 ELSE 0 END AS bit)";
            return await cn.ExecuteScalarAsync<bool>(sql, new { Email = email });
        }

        public async Task<IEnumerable<UsuarioModel>> ListAsync(bool soloActivos = false)
        {
            using IDbConnection cn = _factory.Create();
            string sql = @"SELECT u.*, r.NombreRol AS RolNombre
                           FROM Usuario u JOIN Rol r ON r.IdRol = u.IdRol" +
                         (soloActivos ? " WHERE u.Activo=1" : "") +
                         " ORDER BY u.Apellido, u.Nombre";
            return await cn.QueryAsync<UsuarioModel>(sql);
        }

        public async Task<int> CreateAsync(UsuarioModel u)
        {
            using IDbConnection cn = _factory.Create();
            const string sql = @"
INSERT INTO Usuario (IdRol, Nombre, Apellido, Email, Telefono, PasswordHash, FechaAlta, Activo, Dni)
VALUES (@IdRol, @Nombre, @Apellido, @Email, @Telefono, @PasswordHash, @FechaAlta, @Activo, @Dni);";
            return await cn.ExecuteAsync(sql, u);
        }

        public async Task<int> UpdateAsync(UsuarioModel u)
        {
            using IDbConnection cn = _factory.Create();
            string sql = string.IsNullOrWhiteSpace(u.Password)
                ? @"UPDATE Usuario SET IdRol=@IdRol, Nombre=@Nombre, Apellido=@Apellido,
                                       Email=@Email, Telefono=@Telefono, Activo=@Activo, Dni=@Dni
                    WHERE IdUsuario=@IdUsuario"
                : @"UPDATE Usuario SET IdRol=@IdRol, Nombre=@Nombre, Apellido=@Apellido,
                                       Email=@Email, Telefono=@Telefono, Activo=@Activo, Dni=@Dni,
                                       PasswordHash=@PasswordHash
                    WHERE IdUsuario=@IdUsuario";
            return await cn.ExecuteAsync(sql, u);
        }

        public async Task<int> SetActivoAsync(int idUsuario, bool activo)
        {
            using IDbConnection cn = _factory.Create();
            const string sql = @"UPDATE Usuario SET Activo=@Activo WHERE IdUsuario=@IdUsuario";
            return await cn.ExecuteAsync(sql, new { Activo = activo, IdUsuario = idUsuario });
        }
    }
}
