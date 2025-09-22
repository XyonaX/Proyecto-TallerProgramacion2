using System.Linq;
using System.Threading.Tasks;
using System.Data;
using Dapper;  
using Proyecto_Taller_2.Data;                      
using Proyecto_Taller_2.Data.Repositories.Interfaces; 
using Proyecto_Taller_2.Domain.Entities;             

namespace Proyecto_Taller_2.Data.Repositories.DapperImpl
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly ISqlConnectionFactory _factory;
        public UsuarioRepository(ISqlConnectionFactory factory) => _factory = factory;

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            using IDbConnection cn = _factory.Create();
            const string sql = @"SELECT TOP 1 IdUsuario, IdRol, Nombre, Apellido, Email, PasswordHash, Activo
                                 FROM Usuario
                                 WHERE Email = @Email AND Activo = 1";
            return (await cn.QueryAsync<Usuario>(sql, new { Email = email })).FirstOrDefault();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            using IDbConnection cn = _factory.Create();
            const string sql = "SELECT 1 FROM Usuario WHERE Email = @Email";
            var r = await cn.QueryFirstOrDefaultAsync<int?>(sql, new { Email = email });
            return r.HasValue;
        }

        public async Task<int> CreateAsync(Usuario u)
        {
            using IDbConnection cn = _factory.Create();
            const string sql = @"INSERT INTO Usuario(IdRol,Nombre,Apellido,Email,PasswordHash,Activo)
                                 VALUES(@IdRol,@Nombre,@Apellido,@Email,@PasswordHash,1);
                                 SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return await cn.ExecuteScalarAsync<int>(sql, u);
        }

        public async Task<Usuario?> GetByIdAsync(int id)
        {
            using IDbConnection cn = _factory.Create();
            const string sql = @"SELECT IdUsuario, IdRol, Nombre, Apellido, Email, PasswordHash, Activo
                                 FROM Usuario
                                 WHERE IdUsuario = @Id";
            return (await cn.QueryAsync<Usuario>(sql, new { Id = id })).FirstOrDefault();
        }
    }
}
