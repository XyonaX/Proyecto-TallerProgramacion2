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
                    u.Activo         AS Activo,          -- <- OK
                    u.FechaAlta      AS FechaAlta,       -- <- OK
                    u.IdRol          AS IdRol,
                    r.NombreRol      AS RolNombre
                FROM Usuario u
                JOIN Rol r ON r.IdRol = u.IdRol";

                if (soloActivos) sql += " WHERE u.Activo = 1";
                sql += " ORDER BY u.Apellido, u.Nombre;";

                cmd.CommandText = sql;

                if (cn.State != ConnectionState.Open) cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    int oIdUsuario = rd.GetOrdinal("IdUsuario");
                    int oDni = rd.GetOrdinal("Dni");
                    int oNom = rd.GetOrdinal("Nombre");
                    int oApe = rd.GetOrdinal("Apellido");
                    int oMail = rd.GetOrdinal("Email");
                    int oTel = rd.GetOrdinal("Telefono");
                    int oActivo = rd.GetOrdinal("Activo");        // <- definido
                    int oFAlta = rd.GetOrdinal("FechaAlta");     // <- definido
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
                            Activo = rd.GetBoolean(oActivo),                      // <- ahora sí
                            FechaAlta = rd.GetDateTime(oFAlta),                      // <- ahora sí
                            IdRol = rd.GetInt32(oRol),
                            RolNombre = rd.IsDBNull(oRolN) ? null : rd.GetString(oRolN)
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

            // Generar hash con BCrypt (opcional pero recomendado en alta)
            string passwordHash = null;
            if (!string.IsNullOrWhiteSpace(usuario.Password))
            {
                passwordHash = BCrypt.Net.BCrypt.HashPassword(usuario.Password, workFactor: 11);
            }

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = cn.CreateCommand())
            {
                // Dejamos que la DB setee FechaAlta con GETDATE()
                cmd.CommandText = @"
                    INSERT INTO Usuario
                        (Dni, Nombre, Apellido, Email, Telefono, PasswordHash, FechaAlta, Activo, IdRol)
                    VALUES
                        (@Dni, @Nombre, @Apellido, @Email, @Telefono, @PasswordHash, GETDATE(), @Activo, @IdRol);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                cmd.Parameters.AddWithValue("@Dni", usuario.Dni);
                cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", usuario.Apellido);
                cmd.Parameters.AddWithValue("@Email", usuario.Email);
                cmd.Parameters.AddWithValue("@Telefono", (object)usuario.Telefono ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PasswordHash", (object)passwordHash ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Activo", usuario.Activo);
                cmd.Parameters.AddWithValue("@IdRol", usuario.IdRol);

                cn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        // === ROLES PARA COMBO ===
        public List<(int, string)> ObtenerRoles()
        {
            var roles = new List<(int, string)>();
            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = "SELECT IdRol, NombreRol FROM Rol ORDER BY IdRol";
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                        roles.Add((dr.GetInt32(0), dr.GetString(1)));
                }
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

                // fallback por IdUsuario si te pasan un id en 'dni'
                if (rows == 0)
                {
                    cmd.CommandText = "UPDATE Usuario SET Activo = @Activo WHERE IdUsuario = @IdUsuario";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@Activo", estado);
                    cmd.Parameters.AddWithValue("@IdUsuario", dni);
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
                // Usamos IdUsuario si viene (>0). Si no, cae por Dni (compatibilidad).
                bool usaId = usuario.IdUsuario > 0;

                cmd.CommandText = @"
                    UPDATE Usuario
                    SET
                        Nombre       = @Nombre,
                        Apellido     = @Apellido,
                        Email        = @Email,
                        Telefono     = @Telefono,
                        PasswordHash = COALESCE(@PasswordHash, PasswordHash),
                        -- FechaAlta NO se toca en edición
                        Activo       = @Activo,
                        IdRol        = @IdRol
                    WHERE " + (usaId ? "IdUsuario = @Key" : "Dni = @Key");

                cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", usuario.Apellido);
                cmd.Parameters.AddWithValue("@Email", usuario.Email);
                cmd.Parameters.AddWithValue("@Telefono", (object)usuario.Telefono ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PasswordHash", (object)nuevoHash ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Activo", usuario.Activo);
                cmd.Parameters.AddWithValue("@IdRol", usuario.IdRol);
                cmd.Parameters.AddWithValue("@Key", usaId ? usuario.IdUsuario : usuario.Dni);

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

                    int oPwd = dr.GetOrdinal("PwdHash");
                    string storedHash = dr.IsDBNull(oPwd) ? null : dr.GetString(oPwd);

                    if (string.IsNullOrEmpty(storedHash) || !BCrypt.Net.BCrypt.Verify(password, storedHash))
                        return null;

                    return new Usuario
                    {
                        IdUsuario = dr.GetInt32(dr.GetOrdinal("IdUsuario")),
                        Dni = dr.GetInt32(dr.GetOrdinal("Dni")),
                        Nombre = dr.GetString(dr.GetOrdinal("Nombre")),
                        Apellido = dr.GetString(dr.GetOrdinal("Apellido")),
                        Email = dr.GetString(dr.GetOrdinal("Email")),
                        Telefono = dr.IsDBNull(dr.GetOrdinal("Telefono")) ? null : dr.GetString(dr.GetOrdinal("Telefono")),
                        Activo = dr.GetBoolean(dr.GetOrdinal("Activo")),
                        FechaAlta = dr.GetDateTime(dr.GetOrdinal("FechaAlta")),
                        IdRol = dr.GetInt32(dr.GetOrdinal("IdRol")),
                        RolNombre = dr.IsDBNull(dr.GetOrdinal("NombreRol")) ? null : dr.GetString(dr.GetOrdinal("NombreRol"))
                    };
                }
            }
        }
    }
}
