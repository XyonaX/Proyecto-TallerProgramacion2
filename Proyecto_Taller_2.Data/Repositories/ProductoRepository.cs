using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Proyecto_Taller_2.Domain.Models;

namespace Proyecto_Taller_2.Data.Repositories
{
    public class ProductoRepository
    {
        private readonly string _connStr;
        public ProductoRepository(string connStr) => _connStr = connStr;

        public List<Producto> Listar(string texto = null, bool? soloBajo = null, bool? activo = null, int? categoriaId = null)
        {
            var lista = new List<Producto>();
            using var cn = new SqlConnection(_connStr);

            var sql = @"
SELECT 
    p.IdProducto,
    p.Sku,
    p.Nombre,
    p.Descripcion,
    p.IdCategoria,
    c.Nombre AS CategoriaNombre,
    p.Ubicacion,
    p.Stock,
    p.Minimo,
    p.Precio,
    p.Proveedor,
    p.Activo,
    p.FechaAlta,
    p.Actualizado
FROM Producto p
    INNER JOIN Categoria c ON p.IdCategoria = c.IdCategoria
WHERE 1=1";

            using var cmd = new SqlCommand();
            cmd.Connection = cn;

            if (!string.IsNullOrWhiteSpace(texto))
            {
                sql += " AND (p.Sku LIKE @q OR p.Nombre LIKE @q OR p.Descripcion LIKE @q OR p.Proveedor LIKE @q)";
                cmd.Parameters.Add("@q", SqlDbType.VarChar, 200).Value = $"%{texto}%";
            }
            if (activo.HasValue)
            {
                sql += " AND p.Activo = @act";
                cmd.Parameters.Add("@act", SqlDbType.Bit).Value = activo.Value;
            }
            if (categoriaId.HasValue)
            {
                sql += " AND p.IdCategoria = @cat";
                cmd.Parameters.Add("@cat", SqlDbType.Int).Value = categoriaId.Value;
            }

            // filtro "solo bajo"
            if (soloBajo == true)
                sql += " AND p.Stock <= p.Minimo";

            sql += " ORDER BY p.Nombre";

            cmd.CommandText = sql;
            cn.Open();

            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                lista.Add(new Producto
                {
                    IdProducto = dr.GetInt32(dr.GetOrdinal("IdProducto")),
                    Sku = dr.IsDBNull(dr.GetOrdinal("Sku")) ? "" : dr.GetString(dr.GetOrdinal("Sku")),
                    Nombre = dr.GetString(dr.GetOrdinal("Nombre")),
                    Descripcion = dr.IsDBNull(dr.GetOrdinal("Descripcion")) ? "" : dr.GetString(dr.GetOrdinal("Descripcion")),
                    IdCategoria = dr.GetInt32(dr.GetOrdinal("IdCategoria")),
                    CategoriaNombre = dr.GetString(dr.GetOrdinal("CategoriaNombre")),
                    Ubicacion = dr.IsDBNull(dr.GetOrdinal("Ubicacion")) ? "" : dr.GetString(dr.GetOrdinal("Ubicacion")),
                    Stock = dr.GetInt32(dr.GetOrdinal("Stock")),
                    Minimo = dr.GetInt32(dr.GetOrdinal("Minimo")),
                    Precio = dr.GetDecimal(dr.GetOrdinal("Precio")),
                    Proveedor = dr.IsDBNull(dr.GetOrdinal("Proveedor")) ? "" : dr.GetString(dr.GetOrdinal("Proveedor")),
                    Activo = dr.GetBoolean(dr.GetOrdinal("Activo")),
                    FechaAlta = dr.GetDateTime(dr.GetOrdinal("FechaAlta")),
                    Actualizado = dr.GetDateTime(dr.GetOrdinal("Actualizado"))
                });
            }
            return lista;
        }

        public Producto ObtenerPorId(int id)
        {
            using var cn = new SqlConnection(_connStr);
            var sql = @"
SELECT 
    p.IdProducto,
    p.Sku,
    p.Nombre,
    p.Descripcion,
    p.IdCategoria,
    c.Nombre AS CategoriaNombre,
    p.Ubicacion,
    p.Stock,
    p.Minimo,
    p.Precio,
    p.Proveedor,
    p.Activo,
    p.FechaAlta,
    p.Actualizado
FROM Producto p
    INNER JOIN Categoria c ON p.IdCategoria = c.IdCategoria
WHERE p.IdProducto = @id";

            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@id", id);
            cn.Open();

            using var dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                return new Producto
                {
                    IdProducto = dr.GetInt32(dr.GetOrdinal("IdProducto")),
                    Sku = dr.IsDBNull(dr.GetOrdinal("Sku")) ? "" : dr.GetString(dr.GetOrdinal("Sku")),
                    Nombre = dr.GetString(dr.GetOrdinal("Nombre")),
                    Descripcion = dr.IsDBNull(dr.GetOrdinal("Descripcion")) ? "" : dr.GetString(dr.GetOrdinal("Descripcion")),
                    IdCategoria = dr.GetInt32(dr.GetOrdinal("IdCategoria")),
                    CategoriaNombre = dr.GetString(dr.GetOrdinal("CategoriaNombre")),
                    Ubicacion = dr.IsDBNull(dr.GetOrdinal("Ubicacion")) ? "" : dr.GetString(dr.GetOrdinal("Ubicacion")),
                    Stock = dr.GetInt32(dr.GetOrdinal("Stock")),
                    Minimo = dr.GetInt32(dr.GetOrdinal("Minimo")),
                    Precio = dr.GetDecimal(dr.GetOrdinal("Precio")),
                    Proveedor = dr.IsDBNull(dr.GetOrdinal("Proveedor")) ? "" : dr.GetString(dr.GetOrdinal("Proveedor")),
                    Activo = dr.GetBoolean(dr.GetOrdinal("Activo")),
                    FechaAlta = dr.GetDateTime(dr.GetOrdinal("FechaAlta")),
                    Actualizado = dr.GetDateTime(dr.GetOrdinal("Actualizado"))
                };
            }

            return null;
        }

        public int Agregar(Producto p)
        {
            using var cn = new SqlConnection(_connStr);
            const string sql = @"
INSERT INTO Producto (Sku, Nombre, Descripcion, IdCategoria, Ubicacion, Stock, Minimo, Precio, Proveedor, Activo, FechaAlta, Actualizado)
VALUES (@Sku, @Nombre, @Descripcion, @IdCategoria, @Ubicacion, @Stock, @Minimo, @Precio, @Proveedor, @Activo, @FechaAlta, @Actualizado);
SELECT SCOPE_IDENTITY();";
            
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.Add("@Sku", SqlDbType.VarChar, 50).Value = (object)p.Sku ?? DBNull.Value;
            cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 200).Value = p.Nombre;
            cmd.Parameters.Add("@Descripcion", SqlDbType.VarChar, 1000).Value = (object)p.Descripcion ?? DBNull.Value;
            cmd.Parameters.Add("@IdCategoria", SqlDbType.Int).Value = p.IdCategoria;
            cmd.Parameters.Add("@Ubicacion", SqlDbType.VarChar, 100).Value = (object)p.Ubicacion ?? DBNull.Value;
            cmd.Parameters.Add("@Stock", SqlDbType.Int).Value = p.Stock;
            cmd.Parameters.Add("@Minimo", SqlDbType.Int).Value = p.Minimo;
            cmd.Parameters.Add("@Precio", SqlDbType.Decimal).Value = p.Precio;
            cmd.Parameters.Add("@Proveedor", SqlDbType.VarChar, 200).Value = (object)p.Proveedor ?? DBNull.Value;
            cmd.Parameters.Add("@Activo", SqlDbType.Bit).Value = p.Activo;
            cmd.Parameters.Add("@FechaAlta", SqlDbType.DateTime).Value = p.FechaAlta;
            cmd.Parameters.Add("@Actualizado", SqlDbType.DateTime).Value = p.Actualizado;
            
            cn.Open();
            var id = Convert.ToInt32(cmd.ExecuteScalar());
            return id;
        }

        public int Actualizar(Producto p)
        {
            using var cn = new SqlConnection(_connStr);
            const string sql = @"
UPDATE Producto SET
    Sku=@Sku, Nombre=@Nombre, Descripcion=@Descripcion, IdCategoria=@IdCategoria, 
    Ubicacion=@Ubicacion, Stock=@Stock, Minimo=@Minimo, Precio=@Precio, 
    Proveedor=@Proveedor, Activo=@Activo, Actualizado=@Actualizado
WHERE IdProducto=@Id";
            
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = p.IdProducto;
            cmd.Parameters.Add("@Sku", SqlDbType.VarChar, 50).Value = (object)p.Sku ?? DBNull.Value;
            cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 200).Value = p.Nombre;
            cmd.Parameters.Add("@Descripcion", SqlDbType.VarChar, 1000).Value = (object)p.Descripcion ?? DBNull.Value;
            cmd.Parameters.Add("@IdCategoria", SqlDbType.Int).Value = p.IdCategoria;
            cmd.Parameters.Add("@Ubicacion", SqlDbType.VarChar, 100).Value = (object)p.Ubicacion ?? DBNull.Value;
            cmd.Parameters.Add("@Stock", SqlDbType.Int).Value = p.Stock;
            cmd.Parameters.Add("@Minimo", SqlDbType.Int).Value = p.Minimo;
            cmd.Parameters.Add("@Precio", SqlDbType.Decimal).Value = p.Precio;
            cmd.Parameters.Add("@Proveedor", SqlDbType.VarChar, 200).Value = (object)p.Proveedor ?? DBNull.Value;
            cmd.Parameters.Add("@Activo", SqlDbType.Bit).Value = p.Activo;
            cmd.Parameters.Add("@Actualizado", SqlDbType.DateTime).Value = DateTime.Now;
            
            cn.Open();
            return cmd.ExecuteNonQuery();
        }

        public void Eliminar(int id)
        {
            using var cn = new SqlConnection(_connStr);
            const string sql = "UPDATE Producto SET Activo = 0, Actualizado = @actualizado WHERE IdProducto = @id";
            
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@actualizado", DateTime.Now);
            
            cn.Open();
            cmd.ExecuteNonQuery();
        }

        public bool ExisteSku(string sku, int? excluirId = null)
        {
            if (string.IsNullOrWhiteSpace(sku)) return false;

            using var cn = new SqlConnection(_connStr);
            var sql = "SELECT COUNT(*) FROM Producto WHERE Sku = @sku";
            
            if (excluirId.HasValue)
                sql += " AND IdProducto != @excluirId";

            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@sku", sku);
            if (excluirId.HasValue)
                cmd.Parameters.AddWithValue("@excluirId", excluirId.Value);

            cn.Open();
            return (int)cmd.ExecuteScalar() > 0;
        }

        public List<Producto> ObtenerPorCategoria(int categoriaId)
        {
            return Listar(categoriaId: categoriaId);
        }

        public List<Producto> ObtenerConBajoStock()
        {
            return Listar(soloBajo: true);
        }

        public int ContarPorCategoria(int categoriaId)
        {
            using var cn = new SqlConnection(_connStr);
            const string sql = "SELECT COUNT(*) FROM Producto WHERE IdCategoria = @categoriaId AND Activo = 1";
            
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@categoriaId", categoriaId);
            
            cn.Open();
            return (int)cmd.ExecuteScalar();
        }
    }
}
