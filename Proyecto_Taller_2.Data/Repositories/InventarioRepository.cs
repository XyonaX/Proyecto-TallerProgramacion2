using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Proyecto_Taller_2.Domain.Models;
using Proyecto_Taller_2.Data; // BDGeneral

namespace Proyecto_Taller_2.Data.Repositories
{
    public class InventarioRepository
    {
        private readonly string _cs;

        public InventarioRepository()
        {
            _cs = BDGeneral.ConnectionString; // o ConfigurationManager.ConnectionStrings["ERP"].ConnectionString;
        }

        public List<InventarioItem> Listar(bool soloBajo, string? texto, bool? activo)
        {
            var list = new List<InventarioItem>();

            using var cn = new SqlConnection(_cs);
            using var cmd = cn.CreateCommand();

            var sql = @"
SELECT
    p.IdProducto,
    p.Sku,
    p.NombreProducto,
    ISNULL(p.DescripcionProducto,'') AS DescripcionProducto,
    p.CategoriaId,
    ISNULL(p.Ubicacion,'') AS Ubicacion,
    p.Stock,
    p.Minimo,
    p.PrecioProducto,
    ISNULL(p.ProveedorProducto,'') AS ProveedorProducto,
    p.Activo,
    ISNULL(p.Actualizado, p.FechaAlta) AS Actualizado
FROM dbo.Producto p
WHERE 1=1";

            if (!string.IsNullOrWhiteSpace(texto))
            {
                sql += " AND (p.Sku LIKE @q OR p.NombreProducto LIKE @q OR p.DescripcionProducto LIKE @q OR p.ProveedorProducto LIKE @q)";
                cmd.Parameters.Add("@q", SqlDbType.VarChar, 300).Value = $"%{texto}%";
            }

            if (activo.HasValue)
            {
                sql += " AND p.Activo = @act";
                cmd.Parameters.Add("@act", SqlDbType.Bit).Value = activo.Value;
            }

            if (soloBajo)
                sql += " AND p.Stock < p.Minimo";

            sql += " ORDER BY p.NombreProducto";
            cmd.CommandText = sql;

            cn.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new InventarioItem
                {
                    IdProducto = dr.GetInt32(0),
                    Sku = dr.IsDBNull(1) ? "" : dr.GetString(1),
                    Nombre = dr.IsDBNull(2) ? "" : dr.GetString(2),
                    Descripcion = dr.IsDBNull(3) ? "" : dr.GetString(3),
                    CategoriaId = dr.IsDBNull(4) ? (byte)1 : dr.GetByte(4),
                    Ubicacion = dr.IsDBNull(5) ? "" : dr.GetString(5),
                    Stock = dr.IsDBNull(6) ? 0 : dr.GetInt32(6),
                    Minimo = dr.IsDBNull(7) ? 0 : dr.GetInt32(7),
                    Precio = dr.IsDBNull(8) ? 0m : dr.GetDecimal(8),
                    Proveedor = dr.IsDBNull(9) ? "" : dr.GetString(9),
                    Activo = !dr.IsDBNull(10) && dr.GetBoolean(10),
                    Actualizado = dr.IsDBNull(11) ? DateTime.Now : dr.GetDateTime(11)
                });
            }
            return list;
        }

        public int CrearProducto(Producto p)
        {
            using var cn = new SqlConnection(_cs);
            using var cmd = cn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO dbo.Producto
(Sku, NombreProducto, DescripcionProducto, CategoriaId, Ubicacion, Stock, Minimo, PrecioProducto, ProveedorProducto, Activo, FechaAlta, Actualizado)
VALUES
(@Sku, @Nombre, @Desc, @Cat, @Ubic, 0, @Min, @Precio, @Prov, @Activo, GETDATE(), GETDATE());
SELECT SCOPE_IDENTITY();";

            cmd.Parameters.Add("@Sku", SqlDbType.VarChar, 50).Value = (object?)p.Sku ?? DBNull.Value;
            cmd.Parameters.Add("@Nombre", SqlDbType.NVarChar, 300).Value = p.Nombre ?? "";
            cmd.Parameters.Add("@Desc", SqlDbType.NVarChar, 500).Value = (object?)p.Descripcion ?? DBNull.Value;
            cmd.Parameters.Add("@Cat", SqlDbType.TinyInt).Value = (byte)p.CategoriaId;
            cmd.Parameters.Add("@Ubic", SqlDbType.VarChar, 200).Value = (object?)p.Ubicacion ?? DBNull.Value;
            cmd.Parameters.Add("@Min", SqlDbType.Int).Value = p.Minimo;
            cmd.Parameters.Add("@Precio", SqlDbType.Decimal).Value = p.Precio;
            cmd.Parameters.Add("@Prov", SqlDbType.VarChar, 200).Value = (object?)p.Proveedor ?? DBNull.Value;
            cmd.Parameters.Add("@Activo", SqlDbType.Bit).Value = p.Activo;

            cn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public int ActualizarProducto(Producto p)
        {
            using var cn = new SqlConnection(_cs);
            using var cmd = cn.CreateCommand();
            cmd.CommandText = @"
UPDATE dbo.Producto SET
    Sku=@Sku,
    NombreProducto=@Nombre,
    DescripcionProducto=@Desc,
    CategoriaId=@Cat,
    Ubicacion=@Ubic,
    Minimo=@Min,
    PrecioProducto=@Precio,
    ProveedorProducto=@Prov,
    Activo=@Activo,
    Actualizado=GETDATE()
WHERE IdProducto=@Id;";

            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = p.IdProducto;
            cmd.Parameters.Add("@Sku", SqlDbType.VarChar, 50).Value = (object?)p.Sku ?? DBNull.Value;
            cmd.Parameters.Add("@Nombre", SqlDbType.NVarChar, 300).Value = p.Nombre ?? "";
            cmd.Parameters.Add("@Desc", SqlDbType.NVarChar, 500).Value = (object?)p.Descripcion ?? DBNull.Value;
            cmd.Parameters.Add("@Cat", SqlDbType.TinyInt).Value = (byte)p.CategoriaId;
            cmd.Parameters.Add("@Ubic", SqlDbType.VarChar, 200).Value = (object?)p.Ubicacion ?? DBNull.Value;
            cmd.Parameters.Add("@Min", SqlDbType.Int).Value = p.Minimo;
            cmd.Parameters.Add("@Precio", SqlDbType.Decimal).Value = p.Precio;
            cmd.Parameters.Add("@Prov", SqlDbType.VarChar, 200).Value = (object?)p.Proveedor ?? DBNull.Value;
            cmd.Parameters.Add("@Activo", SqlDbType.Bit).Value = p.Activo;

            cn.Open();
            return cmd.ExecuteNonQuery();
        }

        /// <summary>E=Entrada (+), S=Salida (-), A=Ajuste (delta libre positivo o negativo)</summary>
        public void Movimiento(int idProducto, char tipo, int cantidad, string? observacion, string? origen, int? origenId)
        {
            using var cn = new SqlConnection(_cs);
            cn.Open();
            using var tx = cn.BeginTransaction();

            try
            {
                using (var cmd = cn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = @"
INSERT INTO dbo.MovimientoStock (IdProducto, Fecha, Tipo, Cantidad, Origen, OrigenId, Observacion)
VALUES (@IdProducto, GETDATE(), @Tipo, @Cantidad, @Origen, @OrigenId, @Obs);";
                    cmd.Parameters.Add("@IdProducto", SqlDbType.Int).Value = idProducto;
                    cmd.Parameters.Add("@Tipo", SqlDbType.Char, 1).Value = tipo;
                    cmd.Parameters.Add("@Cantidad", SqlDbType.Int).Value = cantidad; // en Ajuste puede ser negativo
                    cmd.Parameters.Add("@Origen", SqlDbType.VarChar, 100).Value = (object?)origen ?? DBNull.Value;
                    cmd.Parameters.Add("@OrigenId", SqlDbType.Int).Value = (object?)origenId ?? DBNull.Value;
                    cmd.Parameters.Add("@Obs", SqlDbType.NVarChar, 200).Value = (object?)observacion ?? DBNull.Value;
                    cmd.ExecuteNonQuery();
                }

                int delta = tipo == 'E' ? cantidad : tipo == 'S' ? -cantidad : cantidad;

                using (var cmd = cn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = @"
UPDATE dbo.Producto
   SET Stock = Stock + @Delta,
       Actualizado = GETDATE()
 WHERE IdProducto = @IdProducto;";
                    cmd.Parameters.Add("@Delta", SqlDbType.Int).Value = delta;
                    cmd.Parameters.Add("@IdProducto", SqlDbType.Int).Value = idProducto;
                    cmd.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }
}
