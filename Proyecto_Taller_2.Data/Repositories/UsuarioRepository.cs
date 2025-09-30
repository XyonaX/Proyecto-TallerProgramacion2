using Microsoft.Data.SqlClient;
using Proyecto_Taller_2.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace Proyecto_Taller_2.Data.Repositories
{
    public class UsuarioRepository
    {
        // === LISTAR USUARIOS ===
        public List<Usuario> ObtenerUsuarios(bool soloActivos = false)
        {
            using var cn = BDGeneral.GetConnection();

            var sql = @"
                SELECT 
                    u.IdUsuario      AS Dni,
                    u.Nombre         AS Nombre,
                    u.Apellido       AS Apellido,
                    u.Email          AS Email,
                    u.Telefono       AS Telefono,
                    u.Activo         AS Estado,
                    u.FechaAlta      AS FechaNacimiento,
                    u.IdRol          AS IdRol,
                    r.NombreRol      AS RolNombre
                FROM usuario u
                JOIN Rol r ON r.IdRol = u.IdRol";

            if (soloActivos) sql += " WHERE u.Activo = 1";
            sql += " ORDER BY u.Apellido, u.Nombre;";

            using var cmd = new SqlCommand(sql, cn);
            using var rd = cmd.ExecuteReader();

            var list = new List<Usuario>();

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

            return list;
        }

        // === AGREGAR USUARIO ===
        public int AgregarUsuario(Usuario usuario)
        {
            using var cn = BDGeneral.GetConnection();

            const string sql = @"
                INSERT INTO usuario (Nombre, Apellido, Email, Telefono, PasswordHash, FechaAlta, IdRol, Activo)
                VALUES (@Nombre, @Apellido, @Email, @Telefono, @PasswordHash, @FechaAlta, @IdRol, @Activo);";

            using var cmd = new SqlCommand(sql, cn);

            cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 200).Value = usuario.Nombre;
            cmd.Parameters.Add("@Apellido", SqlDbType.VarChar, 200).Value = usuario.Apellido;
            cmd.Parameters.Add("@Email", SqlDbType.VarChar, 200).Value = usuario.Email;
            cmd.Parameters.Add("@Telefono", SqlDbType.VarChar, 50).Value = (object?)usuario.Telefono ?? DBNull.Value;
            cmd.Parameters.Add("@PasswordHash", SqlDbType.VarBinary, -1).Value = Array.Empty<byte>();
            cmd.Parameters.Add("@FechaAlta", SqlDbType.DateTime).Value = usuario.FechaNacimiento ?? DateTime.Now;
            cmd.Parameters.Add("@IdRol", SqlDbType.Int).Value = usuario.IdRol;
            cmd.Parameters.Add("@Activo", SqlDbType.Bit).Value = usuario.Estado;

            return cmd.ExecuteNonQuery();
        }

        // === ROLES PARA COMBO (lo que pide UcUsuarios) ===
        // Devuelve tuplas (Id, Nombre) para poder usar DisplayMember=Item2 / ValueMember=Item1
        public List<(int, string)> ObtenerRoles()
        {
            using var cn = BDGeneral.GetConnection();

            const string sql = @"SELECT r.IdRol, r.NombreRol FROM Rol r ORDER BY r.NombreRol;";
            using var cmd = new SqlCommand(sql, cn);
            using var rd = cmd.ExecuteReader();

            var roles = new List<(int, string)>();
            int oId = rd.GetOrdinal("IdRol");
            int oNm = rd.GetOrdinal("NombreRol");

            while (rd.Read())
                roles.Add((rd.GetInt32(oId), rd.GetString(oNm)));

            return roles;
        }

        // === ACTUALIZAR ESTADO USUARIO ===
        public void ActualizarEstadoUsuario(int dni, bool estado)
        {
            using var cn = BDGeneral.GetConnection();
            const string sql = @"UPDATE usuario SET Activo = @Activo WHERE IdUsuario = @Dni;";
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.Add("@Activo", SqlDbType.Bit).Value = estado;
            cmd.Parameters.Add("@Dni", SqlDbType.Int).Value = dni;
            cmd.ExecuteNonQuery();
        }
    }
}
