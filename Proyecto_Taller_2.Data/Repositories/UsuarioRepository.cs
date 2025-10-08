using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Proyecto_Taller_2.Domain.Models;
using BC = BCrypt.Net.BCrypt;

namespace Proyecto_Taller_2.Data.Repositories
{
    public class UsuarioRepository
    {
        // ============= LISTAR USUARIOS =============
        public List<Usuario> ObtenerUsuarios(bool soloActivos = false)
        {
            var list = new List<Usuario>();

            using (var cn = BDGeneral.GetConnection())
            {
                cn.Open(); // 👈 ABRIR CONEXIÓN

                string sql = @"
SELECT
    u.IdUsuario, u.Dni, u.Nombre, u.Apellido, u.Email, u.Telefono,
    u.PasswordHash, u.FechaAlta, u.Activo, u.IdRol,
    r.NombreRol AS RolNombre
FROM Usuario u
JOIN Rol r ON r.IdRol = u.IdRol";

                if (soloActivos) sql += " WHERE u.Activo = 1";
                sql += " ORDER BY u.Apellido, u.Nombre;";

                using (var cmd = new SqlCommand(sql, cn))
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new Usuario
                        {
                            IdUsuario = rd.GetInt32(rd.GetOrdinal("IdUsuario")),
                            Dni = rd.GetInt32(rd.GetOrdinal("Dni")),
                            Nombre = rd.GetString(rd.GetOrdinal("Nombre")),
                            Apellido = rd.GetString(rd.GetOrdinal("Apellido")),
                            Email = rd.GetString(rd.GetOrdinal("Email")),
                            Telefono = rd.IsDBNull(rd.GetOrdinal("Telefono")) ? null : rd.GetString(rd.GetOrdinal("Telefono")),
                            PasswordHash = rd.IsDBNull(rd.GetOrdinal("PasswordHash")) ? null : rd.GetString(rd.GetOrdinal("PasswordHash")),
                            FechaAlta = rd.IsDBNull(rd.GetOrdinal("FechaAlta")) ? (DateTime?)null : rd.GetDateTime(rd.GetOrdinal("FechaAlta")),
                            Activo = rd.GetBoolean(rd.GetOrdinal("Activo")),
                            IdRol = rd.GetInt32(rd.GetOrdinal("IdRol")),
                            RolNombre = rd.GetString(rd.GetOrdinal("RolNombre"))
                        });
                    }
                }
            }
            return list;
        }

        // ============= AGREGAR USUARIO =============
        public int AgregarUsuario(Usuario u)
        {
            using (var cn = BDGeneral.GetConnection())
            {
                cn.Open(); // 👈 ABRIR CONEXIÓN

                // Verificar duplicados
                const string checkSql = "SELECT COUNT(*) FROM Usuario WHERE Dni = @Dni OR Email = @Email;";
                using (var checkCmd = new SqlCommand(checkSql, cn))
                {
                    checkCmd.Parameters.AddWithValue("@Dni", u.Dni);
                    checkCmd.Parameters.AddWithValue("@Email", u.Email);

                    int existe = (int)checkCmd.ExecuteScalar();
                    if (existe > 0)
                        throw new InvalidOperationException("Ya existe un usuario con el mismo DNI o Email.");
                }

                // Insertar
                const string sql = @"
INSERT INTO Usuario (IdRol, Nombre, Apellido, Email, PasswordHash, FechaAlta, Activo, Telefono, Dni)
VALUES (@IdRol, @Nombre, @Apellido, @Email, @PasswordHash, @FechaAlta, @Activo, @Telefono, @Dni);";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@IdRol", SqlDbType.Int).Value = u.IdRol;
                    cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 200).Value = u.Nombre;
                    cmd.Parameters.Add("@Apellido", SqlDbType.VarChar, 200).Value = u.Apellido;
                    cmd.Parameters.Add("@Email", SqlDbType.VarChar, 200).Value = u.Email;

                    string hash = null;
                    if (!string.IsNullOrWhiteSpace(u.Password))
                        hash = BC.HashPassword(u.Password);

                    cmd.Parameters.Add("@PasswordHash", SqlDbType.VarChar, -1).Value = (object)hash ?? DBNull.Value;
                    cmd.Parameters.Add("@FechaAlta", SqlDbType.DateTime).Value = u.FechaAlta ?? DateTime.Now;
                    cmd.Parameters.Add("@Activo", SqlDbType.Bit).Value = u.Activo;
                    cmd.Parameters.Add("@Telefono", SqlDbType.VarChar, 50).Value = (object)u.Telefono ?? DBNull.Value;
                    cmd.Parameters.Add("@Dni", SqlDbType.Int).Value = u.Dni;

                    return cmd.ExecuteNonQuery();
                }
            }
        }

        // ============= ACTUALIZAR USUARIO =============
        public int ActualizarUsuario(Usuario u)
        {
            using (var cn = BDGeneral.GetConnection())
            {
                cn.Open(); // 👈 ABRIR CONEXIÓN

                // Verificar duplicados
                const string checkSql = "SELECT COUNT(*) FROM Usuario WHERE (Dni = @Dni OR Email = @Email) AND IdUsuario <> @IdUsuario;";
                using (var checkCmd = new SqlCommand(checkSql, cn))
                {
                    checkCmd.Parameters.AddWithValue("@Dni", u.Dni);
                    checkCmd.Parameters.AddWithValue("@Email", u.Email);
                    checkCmd.Parameters.AddWithValue("@IdUsuario", u.IdUsuario);

                    int existe = (int)checkCmd.ExecuteScalar();
                    if (existe > 0)
                        throw new InvalidOperationException("Ya existe otro usuario con el mismo DNI o Email.");
                }

                string sql;
                if (string.IsNullOrWhiteSpace(u.Password))
                {
                    sql = @"
UPDATE Usuario
SET IdRol=@IdRol, Nombre=@Nombre, Apellido=@Apellido,
    Email=@Email, Telefono=@Telefono, Activo=@Activo, Dni=@Dni
WHERE IdUsuario=@IdUsuario;";
                }
                else
                {
                    sql = @"
UPDATE Usuario
SET IdRol=@IdRol, Nombre=@Nombre, Apellido=@Apellido,
    Email=@Email, Telefono=@Telefono, Activo=@Activo, Dni=@Dni, PasswordHash=@PasswordHash
WHERE IdUsuario=@IdUsuario;";
                }

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = u.IdUsuario;
                    cmd.Parameters.Add("@IdRol", SqlDbType.Int).Value = u.IdRol;
                    cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 200).Value = u.Nombre;
                    cmd.Parameters.Add("@Apellido", SqlDbType.VarChar, 200).Value = u.Apellido;
                    cmd.Parameters.Add("@Email", SqlDbType.VarChar, 200).Value = u.Email;
                    cmd.Parameters.Add("@Telefono", SqlDbType.VarChar, 50).Value = (object)u.Telefono ?? DBNull.Value;
                    cmd.Parameters.Add("@Activo", SqlDbType.Bit).Value = u.Activo;
                    cmd.Parameters.Add("@Dni", SqlDbType.Int).Value = u.Dni;

                    if (!string.IsNullOrWhiteSpace(u.Password))
                    {
                        string newHash = BC.HashPassword(u.Password);
                        cmd.Parameters.Add("@PasswordHash", SqlDbType.VarChar, -1).Value = newHash;
                    }

                    return cmd.ExecuteNonQuery();
                }
            }
        }

        // ============= CAMBIO DE ESTADO =============
        public int ActualizarEstadoUsuario(int idUsuario, bool activo)
        {
            using (var cn = BDGeneral.GetConnection())
            {
                cn.Open(); // 👈 ABRIR CONEXIÓN

                using (var cmd = new SqlCommand("UPDATE Usuario SET Activo=@Activo WHERE IdUsuario=@IdUsuario;", cn))
                {
                    cmd.Parameters.Add("@Activo", SqlDbType.Bit).Value = activo;
                    cmd.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario;
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        // ============= ROLES =============
        public List<(int, string)> ObtenerRoles()
        {
            var roles = new List<(int, string)>();
            using (var cn = BDGeneral.GetConnection())
            {
                cn.Open(); // 👈 ABRIR CONEXIÓN

                using (var cmd = new SqlCommand("SELECT IdRol, NombreRol FROM Rol ORDER BY NombreRol;", cn))
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        roles.Add((rd.GetInt32(0), rd.GetString(1)));
                }
            }
            return roles;
        }

        // ============= LOGIN =============
        public (bool ok, Usuario usuario, string error) Login(string email, string password)
        {
            using (var cn = BDGeneral.GetConnection())
            {
                cn.Open(); // 👈 ABRIR CONEXIÓN

                const string sql = @"
SELECT TOP 1 u.*, r.NombreRol
FROM Usuario u
JOIN Rol r ON r.IdRol = u.IdRol
WHERE u.Email = @Email;";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@Email", SqlDbType.VarChar, 200).Value = email;

                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read())
                            return (false, null, "Usuario no encontrado");

                        var user = new Usuario
                        {
                            IdUsuario = rd.GetInt32(rd.GetOrdinal("IdUsuario")),
                            Dni = rd.GetInt32(rd.GetOrdinal("Dni")),
                            Nombre = rd.GetString(rd.GetOrdinal("Nombre")),
                            Apellido = rd.GetString(rd.GetOrdinal("Apellido")),
                            Email = rd.GetString(rd.GetOrdinal("Email")),
                            Telefono = rd.IsDBNull(rd.GetOrdinal("Telefono")) ? null : rd.GetString(rd.GetOrdinal("Telefono")),
                            PasswordHash = rd.IsDBNull(rd.GetOrdinal("PasswordHash")) ? null : rd.GetString(rd.GetOrdinal("PasswordHash")),
                            Activo = rd.GetBoolean(rd.GetOrdinal("Activo")),
                            IdRol = rd.GetInt32(rd.GetOrdinal("IdRol")),
                            RolNombre = rd.GetString(rd.GetOrdinal("NombreRol"))
                        };

                        if (!user.Activo) return (false, null, "Usuario inactivo");
                        if (string.IsNullOrWhiteSpace(user.PasswordHash)) return (false, null, "Usuario sin contraseña");

                        bool ok = BC.Verify(password, user.PasswordHash);
                        return ok ? (true, user, null) : (false, null, "Contraseña incorrecta");
                    }
                }
            }
        }
    }
}
