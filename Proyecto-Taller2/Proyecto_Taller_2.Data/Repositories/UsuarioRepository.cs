using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Proyecto_Taller_2.Domain.Models;
// Paquete NuGet: BCrypt.Net-Next

namespace Proyecto_Taller_2.Data.Repositories
{
    public class UsuarioRepository
    {
        private readonly string _connectionString =
            System.Configuration.ConfigurationManager.ConnectionStrings["ERP"].ConnectionString;

        // === LISTAR USUARIOS ===
        public List<Usuario> ObtenerUsuarios(bool soloActivos = false)
        {
            var list = new List<Usuario>();
            using (var cn = BDGeneral.GetConnection())
            using (var cmd = cn.CreateCommand())
            {
                var sql = @"
                SELECT 
                    u.IdUsuario      AS IdUsuario,
                    u.Dni            AS Dni,
                    u.Nombre         AS Nombre,
                    u.Apellido       AS Apellido,
                    u.Email          AS Email,
                    u.Telefono       AS Telefono,
                    u.Activo         AS Estado,
                    u.FechaAlta      AS FechaNacimiento,
                    u.IdRol          AS IdRol,
                    r.NombreRol      AS RolNombre   -- ?? corregido
                FROM Usuario u
                JOIN Rol r ON r.IdRol = u.IdRol";

                if (soloActivos) sql += " WHERE u.Activo = 1";
                sql += " ORDER BY u.Apellido, u.Nombre;";

                cmd.CommandText = sql;
                using (var rd = cmd.ExecuteReader())
                {
                    int oIdUsuario = rd.GetOrdinal("IdUsuario");
                    int oDni = rd.GetOrdinal("Dni");
                    int oNom = rd.GetOrdinal("Nombre");
                    int oApe = rd.GetOrdinal("Apellido");
                    int oMail = rd.GetOrdinal("Email");
                    int oTel = rd.GetOrdinal("Telefono");
                    int oEst = rd.GetOrdinal("Estado");
                    int oFec = rd.GetOrdinal("FechaNacimiento");
                    int oRol = rd.GetOrdinal("IdRol");
                    int oRolN = rd.GetOrdinal("RolNombre");

                    while (rd.Read())
                    {
                        list.Add(new Usuario
                        {
                            IdUsuario = rd.GetInt32(oIdUsuario),
                            Dni = rd.GetInt32(oDni),
                            Nombre = rd.GetString(oNom),
                            Apellido = rd.GetString(oApe),
                            Email = rd.GetString(oMail),
                            Telefono = rd.IsDBNull(oTel) ? null : rd.GetString(oTel),
                            Estado = rd.GetBoolean(oEst),
                            FechaNacimiento = rd.IsDBNull(oFec) ? (DateTime?)null : rd.GetDateTime(oFec),
                            IdRol = rd.GetInt32(oRol),
                            RolNombre = rd.GetString(oRolN)
                        });
                    }
                }
            }
            return list;
        }

        // === AGREGAR USUARIO ===
        public int AgregarUsuario(Usuario usuario)
        {
            // Validar unicidad por DNI
            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM Usuario WHERE Dni = @Dni";
                cmd.Parameters.AddWithValue("@Dni", usuario.Dni);
                cn.Open();
                if ((int)cmd.ExecuteScalar() > 0)
                    throw new Exception("Ya existe un usuario con ese DNI.");
            }

            // Validar unicidad por Email
            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM Usuario WHERE Email = @Email";
                cmd.Parameters.AddWithValue("@Email", usuario.Email);
                cn.Open();
                if ((int)cmd.ExecuteScalar() > 0)
                    throw new Exception("Ya existe un usuario con ese email.");
            }

            // Generar hash con BCrypt
            string passwordHash = null;
            if (!string.IsNullOrWhiteSpace(usuario.Password))
            {
                passwordHash = BCrypt.Net.BCrypt.HashPassword(usuario.Password, workFactor: 11);
            }

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO Usuario
                        (Dni, Nombre, Apellido, Email, Telefono, PasswordHash, FechaAlta, Activo, IdRol)
                    VALUES
                        (@Dni, @Nombre, @Apellido, @Email, @Telefono, @PasswordHash, @FechaAlta, @Activo, @IdRol)";
                cmd.Parameters.AddWithValue("@Dni", usuario.Dni);
                cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", usuario.Apellido);
                cmd.Parameters.AddWithValue("@Email", usuario.Email);
                cmd.Parameters.AddWithValue("@Telefono", (object)usuario.Telefono ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PasswordHash", (object)passwordHash ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaAlta", usuario.FechaNacimiento ?? DateTime.Now);
                cmd.Parameters.AddWithValue("@Activo", usuario.Estado);
                cmd.Parameters.AddWithValue("@IdRol", usuario.IdRol);
                cn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        // === ROLES PARA COMBO ===
        public List<(int, string)> ObtenerRoles()
        {
            var roles = new List<(int, string)>();
            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = "SELECT IdRol, NombreRol FROM Rol";
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                    while (dr.Read())
                        roles.Add((dr.GetInt32(0), dr.GetString(1)));
            }
            return roles;
        }

        // === ACTUALIZAR ESTADO USUARIO ===
        public void ActualizarEstadoUsuario(int dni, bool estado)
        {
            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = "UPDATE Usuario SET Activo = @Activo WHERE Dni = @Dni";
                cmd.Parameters.AddWithValue("@Activo", estado);
                cmd.Parameters.AddWithValue("@Dni", dni);
                cn.Open();
                var rows = cmd.ExecuteNonQuery();
                if (rows == 0)
                {
                    cmd.CommandText = "UPDATE Usuario SET Activo = @Activo WHERE IdUsuario = @Dni";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@Activo", estado);
                    cmd.Parameters.AddWithValue("@Dni", dni);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // === ACTUALIZAR USUARIO ===
        public void ActualizarUsuario(Usuario usuario)
        {
            string nuevoHash = null;
            if (!string.IsNullOrWhiteSpace(usuario.Password))
                nuevoHash = BCrypt.Net.BCrypt.HashPassword(usuario.Password, workFactor: 11);

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = @"
                    UPDATE Usuario
                    SET
                        Nombre       = @Nombre,
                        Apellido     = @Apellido,
                        Email        = @Email,
                        Telefono     = @Telefono,
                        PasswordHash = COALESCE(@PasswordHash, PasswordHash),
                        FechaAlta    = @FechaNacimiento,
                        Activo       = @Estado,
                        IdRol        = @IdRol
                    WHERE Dni = @Dni";
                cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", usuario.Apellido);
                cmd.Parameters.AddWithValue("@Email", usuario.Email);
                cmd.Parameters.AddWithValue("@Telefono", (object)usuario.Telefono ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PasswordHash", (object)nuevoHash ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaNacimiento", (object)usuario.FechaNacimiento ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Estado", usuario.Estado);
                cmd.Parameters.AddWithValue("@IdRol", usuario.IdRol);
                cmd.Parameters.AddWithValue("@Dni", usuario.Dni);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // === LOGIN (BCrypt) ===
        public Usuario Login(string email, string password)
        {
            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT 
                        u.IdUsuario, u.Dni, u.Nombre, u.Apellido, u.Email, u.Telefono,
                        u.Activo, u.FechaAlta, u.IdRol, r.NombreRol,
                        u.PasswordHash AS PwdHash
                    FROM Usuario u
                    JOIN Rol r ON r.IdRol = u.IdRol
                    WHERE u.Email = @Email";
                cmd.Parameters.AddWithValue("@Email", email);

                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    if (!dr.Read()) return null;

                    string storedHash = dr.IsDBNull(dr.GetOrdinal("PwdHash"))
                        ? null
                        : dr.GetString(dr.GetOrdinal("PwdHash"));

                    if (string.IsNullOrEmpty(storedHash) || !BCrypt.Net.BCrypt.Verify(password, storedHash))
                        return null;

                    return new Usuario
                    {
                        IdUsuario = dr.IsDBNull(dr.GetOrdinal("IdUsuario")) ? 0 : dr.GetInt32(dr.GetOrdinal("IdUsuario")),
                        Dni = dr.IsDBNull(dr.GetOrdinal("Dni")) ? 0 : dr.GetInt32(dr.GetOrdinal("Dni")),
                        Nombre = dr.IsDBNull(dr.GetOrdinal("Nombre")) ? string.Empty : dr.GetString(dr.GetOrdinal("Nombre")),
                        Apellido = dr.IsDBNull(dr.GetOrdinal("Apellido")) ? string.Empty : dr.GetString(dr.GetOrdinal("Apellido")),
                        Email = dr.IsDBNull(dr.GetOrdinal("Email")) ? string.Empty : dr.GetString(dr.GetOrdinal("Email")),
                        Telefono = dr.IsDBNull(dr.GetOrdinal("Telefono")) ? null : dr.GetString(dr.GetOrdinal("Telefono")),
                        Estado = dr.IsDBNull(dr.GetOrdinal("Activo")) ? false : dr.GetBoolean(dr.GetOrdinal("Activo")),
                        FechaNacimiento = dr.IsDBNull(dr.GetOrdinal("FechaAlta")) ? (DateTime?)null : dr.GetDateTime(dr.GetOrdinal("FechaAlta")),
                        IdRol = dr.IsDBNull(dr.GetOrdinal("IdRol")) ? 0 : dr.GetInt32(dr.GetOrdinal("IdRol")),
                        RolNombre = dr.IsDBNull(dr.GetOrdinal("NombreRol")) ? string.Empty : dr.GetString(dr.GetOrdinal("NombreRol"))
                    };
                }
            }
        }
    }
}
