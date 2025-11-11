using Microsoft.Data.SqlClient;
using Proyecto_Taller_2.Domain.Entities; // Para Producto (Entidad), MovimientoStock
using Proyecto_Taller_2.Domain.Models; // Para InventarioItem, Categoria
using Proyecto_Taller_2.Data; // Para BDGeneral y Factories
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ProductoEntity = Proyecto_Taller_2.Domain.Entities.Producto;

namespace Proyecto_Taller_2.Data.Repositories
{
    public class InventarioRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;
        private readonly string _cs;

        public InventarioRepository()
        {
            _connectionFactory = new SqlConnectionFactory("ERP");
            _cs = BDGeneral.ConnectionString;
        }

        public async Task<List<InventarioItem>> ListarAsync(bool soloBajo, string texto, bool? activo, int? categoriaId = null)
        {
            var list = new List<InventarioItem>();
            var parameters = new List<SqlParameter>();
            var sql = @"SELECT p.IdProducto, p.Sku, p.Nombre, ISNULL(p.Descripcion,'') AS Descripcion,
                               p.IdCategoria, cat.Nombre AS CategoriaNombre,
                               ISNULL(p.Ubicacion,'') AS Ubicacion, p.Stock, p.Minimo, p.Precio,
                               ISNULL(p.Proveedor,'') AS Proveedor, p.Activo,
                               ISNULL(p.Actualizado, p.FechaAlta) AS Actualizado
                        FROM dbo.Producto p
                        LEFT JOIN dbo.Categoria cat ON p.IdCategoria = cat.IdCategoria
                        WHERE 1=1";

            if (!string.IsNullOrWhiteSpace(texto))
            {
                sql += " AND (p.Sku LIKE @q OR p.Nombre LIKE @q OR p.Descripcion LIKE @q OR p.Proveedor LIKE @q)";
                parameters.Add(new SqlParameter("@q", SqlDbType.VarChar, 300) { Value = $"%{texto}%" });
            }
            if (activo.HasValue) { sql += " AND p.Activo = @act"; parameters.Add(new SqlParameter("@act", SqlDbType.Bit) { Value = activo.Value }); }
            if (categoriaId.HasValue && categoriaId > 0) { sql += " AND p.IdCategoria = @catId"; parameters.Add(new SqlParameter("@catId", SqlDbType.Int) { Value = categoriaId.Value }); }
            if (soloBajo) { sql += " AND p.Stock < p.Minimo"; }
            sql += " ORDER BY p.Nombre";

            using (var cn = _connectionFactory.Create())
            using (var cmd = new SqlCommand(sql, cn))
            {
                if (parameters.Any()) cmd.Parameters.AddRange(parameters.ToArray());
                using (var dr = await cmd.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        list.Add(new InventarioItem
                        {
                            IdProducto = (int)dr["IdProducto"],
                            Sku = dr["Sku"] as string ?? "",
                            NombreProducto = dr["Nombre"] as string ?? "", // Mapea Columna Nombre a Modelo Nombre
                            DescripcionProducto = dr["Descripcion"] as string ?? "",
                            IdCategoria = dr["IdCategoria"] is DBNull ? 0 : (int)dr["IdCategoria"],
                            Categoria = dr["CategoriaNombre"] as string ?? "Sin Categoría",
                            Ubicacion = dr["Ubicacion"] as string ?? "",
                            Stock = dr["Stock"] is DBNull ? 0 : (int)dr["Stock"],
                            Minimo = dr["Minimo"] is DBNull ? 0 : (int)dr["Minimo"],
                            PrecioProducto = dr["Precio"] is DBNull ? 0m : (decimal)dr["Precio"], // Mapea Columna Precio a Modelo Precio
                            Proveedor = dr["Proveedor"] as string ?? "",
                            Activo = dr["Activo"] is DBNull ? false : (bool)dr["Activo"],
                            Actualizado = dr["Actualizado"] is DBNull ? DateTime.MinValue : (DateTime)dr["Actualizado"]
                        });
                    }
                }
            }
            return list;
        }

        // --- CORREGIDO: CrearProducto usa los nombres de Producto (Entidad) ---
        public int CrearProducto(ProductoEntity p) // Acepta Producto (Entidad)
        {
            using (var cn = _connectionFactory.Create())
            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = @"
INSERT INTO dbo.Producto (Sku, Nombre, Descripcion, IdCategoria, Ubicacion, Stock, Minimo, Precio, Proveedor, Activo, FechaAlta, Actualizado)
VALUES (@Sku, @Nombre, @Desc, @Cat, @Ubic, 0, @Min, @Precio, @Prov, @Activo, GETDATE(), GETDATE());
SELECT SCOPE_IDENTITY();";

                // Mapea desde Producto (Entidad) a parámetros SQL
                cmd.Parameters.AddWithValue("@Sku", (object)p.Sku ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Nombre", p.Nombre ?? ""); // Usa p.Nombre
                cmd.Parameters.AddWithValue("@Desc", (object)p.Descripcion ?? DBNull.Value); // Usa p.Descripcion
                cmd.Parameters.AddWithValue("@Cat", p.IdCategoria);
                cmd.Parameters.AddWithValue("@Ubic", (object)p.Ubicacion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Min", p.Minimo);
                cmd.Parameters.AddWithValue("@Precio", p.Precio); // Usa p.Precio
                cmd.Parameters.AddWithValue("@Prov", (object)p.Proveedor ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Activo", p.Activo);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // --- CORREGIDO: ActualizarProducto usa los nombres de Producto (Entidad) ---
        public int ActualizarProducto(ProductoEntity p) // Acepta Producto (Entidad)
        {
            using (var cn = _connectionFactory.Create())
            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = @"
UPDATE dbo.Producto SET Sku=@Sku, Nombre=@Nombre, Descripcion=@Desc, IdCategoria=@Cat,
                         Ubicacion=@Ubic, Minimo=@Min, Precio=@Precio, Proveedor=@Prov,
                         Activo=@Activo, Actualizado=GETDATE()
WHERE IdProducto=@Id;";

                cmd.Parameters.AddWithValue("@Id", p.IdProducto);
                cmd.Parameters.AddWithValue("@Sku", (object)p.Sku ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Nombre", p.Nombre ?? ""); // Usa p.Nombre
                cmd.Parameters.AddWithValue("@Desc", (object)p.Descripcion ?? DBNull.Value); // Usa p.Descripcion
                cmd.Parameters.AddWithValue("@Cat", p.IdCategoria);
                cmd.Parameters.AddWithValue("@Ubic", (object)p.Ubicacion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Min", p.Minimo);
                cmd.Parameters.AddWithValue("@Precio", p.Precio); // Usa p.Precio
                cmd.Parameters.AddWithValue("@Prov", (object)p.Proveedor ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Activo", p.Activo);

                return cmd.ExecuteNonQuery();
            }
        }

        public void Movimiento(int idProducto, char tipo, int cantidad, string observacion, string origen, int? origenId)
        {
            using (var cn = _connectionFactory.Create())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    using (var cmd = cn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = @"INSERT INTO dbo.MovimientoStock (IdProducto, Fecha, Tipo, Cantidad, Origen, OrigenId, Observacion)
                                            VALUES (@IdProducto, GETDATE(), @Tipo, @Cantidad, @Origen, @OrigenId, @Obs);";
                        cmd.Parameters.AddWithValue("@IdProducto", idProducto);
                        cmd.Parameters.AddWithValue("@Tipo", tipo.ToString());
                        cmd.Parameters.AddWithValue("@Cantidad", cantidad);
                        cmd.Parameters.AddWithValue("@Origen", (object)origen ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@OrigenId", (object)origenId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Obs", (object)observacion ?? DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                    int delta = tipo == 'E' ? cantidad : tipo == 'S' ? -cantidad : cantidad;
                    using (var cmd = cn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = @"UPDATE dbo.Producto SET Stock = Stock + @Delta, Actualizado = GETDATE()
                                            WHERE IdProducto = @IdProducto;";
                        cmd.Parameters.AddWithValue("@Delta", delta);
                        cmd.Parameters.AddWithValue("@IdProducto", idProducto);
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
                catch { tx.Rollback(); throw; }
            }
        }

        public async Task<List<Categoria>> ObtenerCategoriasAsync()
        {
            var categorias = new List<Categoria>();
            string sql = "SELECT IdCategoria, Nombre FROM dbo.Categoria ORDER BY Nombre";
            using (var cn = _connectionFactory.Create())
            using (var cmd = new SqlCommand(sql, cn))
            using (var dr = await cmd.ExecuteReaderAsync())
            {
                while (await dr.ReadAsync())
                {
                    categorias.Add(new Categoria
                    {
                        IdCategoria = dr["IdCategoria"] is DBNull ? 0 : (int)dr["IdCategoria"],
                        Nombre = dr["Nombre"] as string ?? ""
                    });
                }
            }
            return categorias;
        }

        public async Task<List<MovimientoStock>> GetHistorialMovimientosAsync(int idProducto)
        {
            var historial = new List<MovimientoStock>();
            string sql = @"SELECT IdMov, IdProducto, Fecha, Tipo, Cantidad, Origen, OrigenId, Observacion
                   FROM dbo.MovimientoStock WHERE IdProducto = @IdProducto ORDER BY Fecha DESC";

            using (var cn = _connectionFactory.Create())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@IdProducto", idProducto);
                using (var dr = await cmd.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        historial.Add(new MovimientoStock
                        {
                            IdMov = dr["IdMov"] is DBNull ? 0L : Convert.ToInt64(dr["IdMov"]),
                            IdProducto = dr["IdProducto"] is DBNull ? 0 : Convert.ToInt32(dr["IdProducto"]),
                            Fecha = dr["Fecha"] is DBNull ? DateTime.MinValue : Convert.ToDateTime(dr["Fecha"]),
                            Tipo = (dr["Tipo"] as string ?? "A").FirstOrDefault(),
                            Cantidad = dr["Cantidad"] is DBNull ? 0 : Convert.ToInt32(dr["Cantidad"]),
                            Origen = dr["Origen"] as string ?? "",
                            OrigenId = dr["OrigenId"] is DBNull ? null : Convert.ToInt32(dr["OrigenId"]),
                            Observacion = dr["Observacion"] as string ?? ""
                        });
                    }
                }
            }
            return historial;
        }
    }
}