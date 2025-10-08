using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Proyecto_Taller_2.Domain.Enums;
using Proyecto_Taller_2.Domain.Models;

namespace Proyecto_Taller_2.Data.Repositories
{
    public class ProductoRepository
    {
        private readonly string _connStr;
        public ProductoRepository(string connStr) => _connStr = connStr;

        public List<Producto> Listar(string? texto = null, bool? soloBajo = null, bool? activo = null, byte? categoriaId = null)
        {
            var lista = new List<Producto>();
            using var cn = new SqlConnection(_connStr);

            var sql = @"
SELECT 
    p.IdProducto,
    p.Sku,
    p.Nombre,
    p.CategoriaId,      -- ¡IMPORTANTE: aquí viene la categoría correcta!
    p.Ubicacion,
    p.Stock,            -- Stock real
    p.Minimo,           -- Stock mínimo
    p.Precio,
    p.Proveedor,
    p.Activo,
    ISNULL(p.Actualizado, p.FechaAlta) AS Actualizado
FROM Producto p
WHERE 1=1";

            using var cmd = new SqlCommand();
            cmd.Connection = cn;

            if (!string.IsNullOrWhiteSpace(texto))
            {
                sql += " AND (p.Sku LIKE @q OR p.Nombre LIKE @q OR p.Proveedor LIKE @q)";
                cmd.Parameters.Add("@q", SqlDbType.VarChar, 200).Value = $"%{texto}%";
            }
            if (activo.HasValue)
            {
                sql += " AND p.Activo = @act";
                cmd.Parameters.Add("@act", SqlDbType.Bit).Value = activo.Value;
            }
            if (categoriaId.HasValue)
            {
                sql += " AND p.CategoriaId = @cat";
                cmd.Parameters.Add("@cat", SqlDbType.TinyInt).Value = categoriaId.Value;
            }

            // filtro “solo bajo”
            if (soloBajo == true)
                sql += " AND p.Stock < p.Minimo";

            sql += " ORDER BY p.Nombre";

            cmd.CommandText = sql;
            cn.Open();

            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                lista.Add(new Producto
                {
                    IdProducto = dr.GetInt32(0),
                    Sku = dr.GetString(1),
                    Nombre = dr.GetString(2),
                    CategoriaId = (CategoriaProducto)dr.GetByte(3),
                    Ubicacion = dr.IsDBNull(4) ? "" : dr.GetString(4),
                    Stock = dr.GetInt32(5),
                    Minimo = dr.GetInt32(6),
                    Precio = dr.GetDecimal(7),
                    Proveedor = dr.IsDBNull(8) ? "" : dr.GetString(8),
                    Activo = dr.GetBoolean(9),
                    Actualizado = dr.GetDateTime(10)
                });
            }
            return lista;
        }

        public int Agregar(Producto p)
        {
            using var cn = new SqlConnection(_connStr);
            const string sql = @"
INSERT INTO Producto (Sku, Nombre, CategoriaId, Ubicacion, Stock, Minimo, Precio, Proveedor, Activo, FechaAlta, Actualizado)
VALUES (@Sku, @Nombre, @CategoriaId, @Ubicacion, @Stock, @Minimo, @Precio, @Proveedor, @Activo, GETDATE(), GETDATE());
SELECT SCOPE_IDENTITY();";
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.Add("@Sku", SqlDbType.VarChar, 50).Value = p.Sku;
            cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 200).Value = p.Nombre;
            cmd.Parameters.Add("@CategoriaId", SqlDbType.TinyInt).Value = (byte)p.CategoriaId;
            cmd.Parameters.Add("@Ubicacion", SqlDbType.VarChar, 200).Value = (object?)p.Ubicacion ?? DBNull.Value;
            cmd.Parameters.Add("@Stock", SqlDbType.Int).Value = p.Stock;
            cmd.Parameters.Add("@Minimo", SqlDbType.Int).Value = p.Minimo;
            cmd.Parameters.Add("@Precio", SqlDbType.Money).Value = p.Precio;
            cmd.Parameters.Add("@Proveedor", SqlDbType.VarChar, 200).Value = (object?)p.Proveedor ?? DBNull.Value;
            cmd.Parameters.Add("@Activo", SqlDbType.Bit).Value = p.Activo;
            cn.Open();
            var id = Convert.ToInt32(cmd.ExecuteScalar());
            return id;
        }

        public int Actualizar(Producto p)
        {
            using var cn = new SqlConnection(_connStr);
            const string sql = @"
UPDATE Producto SET
    Sku=@Sku, Nombre=@Nombre, CategoriaId=@CategoriaId, Ubicacion=@Ubicacion,
    Stock=@Stock, Minimo=@Minimo, Precio=@Precio, Proveedor=@Proveedor,
    Activo=@Activo, Actualizado=GETDATE()
WHERE IdProducto=@Id;";
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = p.IdProducto;
            cmd.Parameters.Add("@Sku", SqlDbType.VarChar, 50).Value = p.Sku;
            cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 200).Value = p.Nombre;
            cmd.Parameters.Add("@CategoriaId", SqlDbType.TinyInt).Value = (byte)p.CategoriaId;
            cmd.Parameters.Add("@Ubicacion", SqlDbType.VarChar, 200).Value = (object?)p.Ubicacion ?? DBNull.Value;
            cmd.Parameters.Add("@Stock", SqlDbType.Int).Value = p.Stock;
            cmd.Parameters.Add("@Minimo", SqlDbType.Int).Value = p.Minimo;
            cmd.Parameters.Add("@Precio", SqlDbType.Money).Value = p.Precio;
            cmd.Parameters.Add("@Proveedor", SqlDbType.VarChar, 200).Value = (object?)p.Proveedor ?? DBNull.Value;
            cmd.Parameters.Add("@Activo", SqlDbType.Bit).Value = p.Activo;
            cn.Open();
            return cmd.ExecuteNonQuery();
        }
    }
}
