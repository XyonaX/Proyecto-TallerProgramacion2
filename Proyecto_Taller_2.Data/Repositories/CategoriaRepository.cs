using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Proyecto_Taller_2.Domain.Models;

namespace Proyecto_Taller_2.Data.Repositories
{
    public class CategoriaRepository
    {
        private readonly string _connectionString;

        public CategoriaRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Categoria> ObtenerTodas(bool soloActivas = false)
        {
            var categorias = new List<Categoria>();

            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                var sql = @"
                    SELECT IdCategoria, Nombre, Descripcion, Activo, FechaCreacion
                    FROM Categoria";
                
                if (soloActivas)
                    sql += " WHERE Activo = 1";
                
                sql += " ORDER BY Nombre";

                using (var cmd = new SqlCommand(sql, cn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categorias.Add(new Categoria
                        {
                            IdCategoria = reader.GetInt32(reader.GetOrdinal("IdCategoria")),
                            Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                            Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? "" : reader.GetString(reader.GetOrdinal("Descripcion")),
                            Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
                            FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion"))
                        });
                    }
                }
            }

            return categorias;
        }

        public Categoria ObtenerPorId(int id)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                var sql = @"
                    SELECT IdCategoria, Nombre, Descripcion, Activo, FechaCreacion
                    FROM Categoria
                    WHERE IdCategoria = @id";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Categoria
                            {
                                IdCategoria = reader.GetInt32(reader.GetOrdinal("IdCategoria")),
                                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                                Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? "" : reader.GetString(reader.GetOrdinal("Descripcion")),
                                Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
                                FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion"))
                            };
                        }
                    }
                }
            }

            return null;
        }

        public int Agregar(Categoria categoria)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                var sql = @"
                    INSERT INTO Categoria (Nombre, Descripcion, Activo, FechaCreacion)
                    OUTPUT INSERTED.IdCategoria
                    VALUES (@nombre, @descripcion, @activo, @fechaCreacion)";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@nombre", categoria.Nombre);
                    cmd.Parameters.AddWithValue("@descripcion", categoria.Descripcion ?? "");
                    cmd.Parameters.AddWithValue("@activo", categoria.Activo);
                    cmd.Parameters.AddWithValue("@fechaCreacion", categoria.FechaCreacion);

                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        public void Actualizar(Categoria categoria)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                var sql = @"
                    UPDATE Categoria 
                    SET Nombre = @nombre, 
                        Descripcion = @descripcion, 
                        Activo = @activo
                    WHERE IdCategoria = @id";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@id", categoria.IdCategoria);
                    cmd.Parameters.AddWithValue("@nombre", categoria.Nombre);
                    cmd.Parameters.AddWithValue("@descripcion", categoria.Descripcion ?? "");
                    cmd.Parameters.AddWithValue("@activo", categoria.Activo);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Eliminar(int id)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                
                // Verificar si hay productos asociados
                var checkSql = "SELECT COUNT(*) FROM Producto WHERE IdCategoria = @id";
                using (var checkCmd = new SqlCommand(checkSql, cn))
                {
                    checkCmd.Parameters.AddWithValue("@id", id);
                    var count = (int)checkCmd.ExecuteScalar();
                    
                    if (count > 0)
                    {
                        throw new InvalidOperationException($"No se puede eliminar la categoría porque tiene {count} productos asociados.");
                    }
                }

                // Eliminar categoría
                var sql = "DELETE FROM Categoria WHERE IdCategoria = @id";
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool ExisteNombre(string nombre, int? excluirId = null)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                var sql = "SELECT COUNT(*) FROM Categoria WHERE Nombre = @nombre";
                
                if (excluirId.HasValue)
                    sql += " AND IdCategoria != @excluirId";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    if (excluirId.HasValue)
                        cmd.Parameters.AddWithValue("@excluirId", excluirId.Value);

                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }
    }
}