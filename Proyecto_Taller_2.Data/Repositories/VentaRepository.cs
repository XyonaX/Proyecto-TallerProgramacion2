using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Proyecto_Taller_2.Data.Repositories;
using Proyecto_Taller_2.Domain.Models;

namespace Proyecto_Taller_2.Data.Repositories
{
    public class VentaRepository
    {
        private readonly string _connectionString;

        public VentaRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Venta> ObtenerTodas(int? idUsuario = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var ventas = new List<Venta>();

            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                
                var sql = @"
                SELECT 
                    v.IdVenta,
                    v.NumeroVenta,
                    v.IdUsuario,
                    u.Nombre + ' ' + u.Apellido AS NombreVendedor,
                    v.IdCliente,
                    CASE 
                        WHEN c.Tipo = 'PF' THEN ISNULL(c.NombreCliente, '') + ' ' + ISNULL(c.ApellidoCliente, '')
                        WHEN c.Tipo = 'PJ' THEN ISNULL(c.RazonSocial, '')
                        ELSE 'Cliente #' + CAST(c.IdCliente AS VARCHAR(10))
                    END AS NombreCliente,
                    CASE 
                        WHEN c.Tipo = 'PJ' THEN ISNULL(c.RazonSocial, '')
                        ELSE ''
                    END AS EmpresaCliente,
                    v.FechaVenta,
                    v.Tipo,
                    v.Estado,
                    v.Total,
                    v.Observaciones,
                    v.FechaCreacion,
                    v.FechaActualizacion
                FROM Venta v
                INNER JOIN Usuario u ON v.IdUsuario = u.IdUsuario
                LEFT JOIN Cliente c ON v.IdCliente = c.IdCliente
                WHERE 1=1";

                if (idUsuario.HasValue)
                    sql += " AND v.IdUsuario = @IdUsuario";
                
                if (fechaDesde.HasValue)
                    sql += " AND v.FechaVenta >= @FechaDesde";
                    
                if (fechaHasta.HasValue)
                    sql += " AND v.FechaVenta <= @FechaHasta";

                sql += " ORDER BY v.FechaVenta DESC, v.IdVenta DESC";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    if (idUsuario.HasValue)
                        cmd.Parameters.AddWithValue("@IdUsuario", idUsuario.Value);
                    if (fechaDesde.HasValue)
                        cmd.Parameters.AddWithValue("@FechaDesde", fechaDesde.Value);
                    if (fechaHasta.HasValue)
                        cmd.Parameters.AddWithValue("@FechaHasta", fechaHasta.Value);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ventas.Add(new Venta
                            {
                                IdVenta = reader.GetInt32(reader.GetOrdinal("IdVenta")),
                                NumeroVenta = reader.IsDBNull(reader.GetOrdinal("NumeroVenta")) ? "" : reader.GetString(reader.GetOrdinal("NumeroVenta")),
                                IdUsuario = reader.GetInt32(reader.GetOrdinal("IdUsuario")),
                                NombreVendedor = reader.IsDBNull(reader.GetOrdinal("NombreVendedor")) ? "" : reader.GetString(reader.GetOrdinal("NombreVendedor")),
                                IdCliente = reader.GetInt32(reader.GetOrdinal("IdCliente")),
                                NombreCliente = reader.IsDBNull(reader.GetOrdinal("NombreCliente")) ? "" : reader.GetString(reader.GetOrdinal("NombreCliente")),
                                EmpresaCliente = reader.IsDBNull(reader.GetOrdinal("EmpresaCliente")) ? "" : reader.GetString(reader.GetOrdinal("EmpresaCliente")),
                                FechaVenta = reader.GetDateTime(reader.GetOrdinal("FechaVenta")),
                                Tipo = reader.IsDBNull(reader.GetOrdinal("Tipo")) ? "" : reader.GetString(reader.GetOrdinal("Tipo")),
                                Estado = reader.IsDBNull(reader.GetOrdinal("Estado")) ? "" : reader.GetString(reader.GetOrdinal("Estado")),
                                Total = reader.GetDecimal(reader.GetOrdinal("Total")),
                                Observaciones = reader.IsDBNull(reader.GetOrdinal("Observaciones")) ? "" : reader.GetString(reader.GetOrdinal("Observaciones")),
                                FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion")),
                                FechaActualizacion = reader.IsDBNull(reader.GetOrdinal("FechaActualizacion")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("FechaActualizacion"))
                            });
                        }
                    }
                }
            }

            return ventas;
        }

        public KpisVentas ObtenerKpis(int? idUsuario = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var kpis = new KpisVentas();

            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();

                // Si no se especifica rango de fechas, usar mes actual
                if (!fechaDesde.HasValue)
                {
                    fechaDesde = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                }
                if (!fechaHasta.HasValue)
                {
                    fechaHasta = fechaDesde.Value.AddMonths(1).AddDays(-1);
                }

                // Calcular fechas del periodo anterior para comparacion
                var diasEnPeriodo = (fechaHasta.Value - fechaDesde.Value).Days + 1;
                var fechaAnteriorDesde = fechaDesde.Value.AddDays(-diasEnPeriodo);
                var fechaAnteriorHasta = fechaDesde.Value.AddDays(-1);

                // KPIs del periodo actual
                var sqlActual = @"
                SELECT 
                    COUNT(CASE WHEN Tipo IN ('Venta', 'Devolucion') THEN 1 END) as TotalOrdenes,
                    ISNULL(SUM(CASE WHEN Estado = 'Completada' AND Tipo = 'Venta' THEN Total ELSE 0 END), 0) as VentasCompletadas,
                    ISNULL(AVG(CASE WHEN Estado = 'Completada' AND Tipo = 'Venta' THEN Total ELSE NULL END), 0) as TicketPromedio,
                    COUNT(CASE WHEN Tipo = 'Cotización' THEN 1 END) as TotalCotizaciones,
                    COUNT(CASE WHEN Tipo = 'Cotización' AND Estado = 'Pendiente' THEN 1 END) as CotizacionesPendientes
                FROM Venta v
                WHERE v.FechaVenta >= @FechaDesde AND v.FechaVenta <= @FechaHasta";

                if (idUsuario.HasValue)
                    sqlActual += " AND v.IdUsuario = @IdUsuario";

                using (var cmd = new SqlCommand(sqlActual, cn))
                {
                    cmd.Parameters.AddWithValue("@FechaDesde", fechaDesde.Value);
                    cmd.Parameters.AddWithValue("@FechaHasta", fechaHasta.Value);
                    if (idUsuario.HasValue)
                        cmd.Parameters.AddWithValue("@IdUsuario", idUsuario.Value);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            kpis.TotalOrdenes = reader.GetInt32(reader.GetOrdinal("TotalOrdenes"));
                            kpis.VentasDelMes = reader.GetDecimal(reader.GetOrdinal("VentasCompletadas"));
                            kpis.TicketPromedio = reader.GetDecimal(reader.GetOrdinal("TicketPromedio"));
                            kpis.TotalCotizaciones = reader.GetInt32(reader.GetOrdinal("TotalCotizaciones"));
                            kpis.CotizacionesPendientes = reader.GetInt32(reader.GetOrdinal("CotizacionesPendientes"));
                        }
                    }
                }

                // KPIs del periodo anterior para comparacion
                var sqlAnterior = @"
                SELECT 
                    COUNT(CASE WHEN Tipo IN ('Venta', 'Devolucion') THEN 1 END) as TotalOrdenesAnterior,
                    ISNULL(SUM(CASE WHEN Estado = 'Completada' AND Tipo = 'Venta' THEN Total ELSE 0 END), 0) as VentasAnterior,
                    ISNULL(AVG(CASE WHEN Estado = 'Completada' AND Tipo = 'Venta' THEN Total ELSE NULL END), 0) as TicketAnterior,
                    COUNT(CASE WHEN Tipo = 'Cotización' THEN 1 END) as CotizacionesAnterior
                FROM Venta v
                WHERE v.FechaVenta >= @FechaAnteriorDesde AND v.FechaVenta <= @FechaAnteriorHasta";

                if (idUsuario.HasValue)
                    sqlAnterior += " AND v.IdUsuario = @IdUsuario";

                using (var cmd = new SqlCommand(sqlAnterior, cn))
                {
                    cmd.Parameters.AddWithValue("@FechaAnteriorDesde", fechaAnteriorDesde);
                    cmd.Parameters.AddWithValue("@FechaAnteriorHasta", fechaAnteriorHasta);
                    if (idUsuario.HasValue)
                        cmd.Parameters.AddWithValue("@IdUsuario", idUsuario.Value);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var ordenesAnterior = reader.GetInt32(reader.GetOrdinal("TotalOrdenesAnterior"));
                            var ventasAnterior = reader.GetDecimal(reader.GetOrdinal("VentasAnterior"));
                            var ticketAnterior = reader.GetDecimal(reader.GetOrdinal("TicketAnterior"));
                            var cotizacionesAnterior = reader.GetInt32(reader.GetOrdinal("CotizacionesAnterior"));

                            // Calcular porcentajes de cambio
                            kpis.PorcentajeVsAnterior = CalcularPorcentajeCrecimiento(kpis.VentasDelMes, ventasAnterior);
                            kpis.PorcentajeOrdenesAnterior = CalcularPorcentajeCrecimiento(kpis.TotalOrdenes, ordenesAnterior);
                            kpis.PorcentajeTicketAnterior = CalcularPorcentajeCrecimiento(kpis.TicketPromedio, ticketAnterior);
                            kpis.PorcentajeCotizacionesAnterior = CalcularPorcentajeCrecimiento(kpis.TotalCotizaciones, cotizacionesAnterior);
                        }
                        else
                        {
                            // Si no hay datos del periodo anterior
                            kpis.PorcentajeVsAnterior = kpis.VentasDelMes > 0 ? 100 : 0;
                            kpis.PorcentajeOrdenesAnterior = kpis.TotalOrdenes > 0 ? 100 : 0;
                            kpis.PorcentajeTicketAnterior = kpis.TicketPromedio > 0 ? 100 : 0;
                            kpis.PorcentajeCotizacionesAnterior = kpis.TotalCotizaciones > 0 ? 100 : 0;
                        }
                    }
                }
            }

            return kpis;
        }

        private decimal CalcularPorcentajeCrecimiento(decimal valorActual, decimal valorAnterior)
        {
            if (valorAnterior == 0)
            {
                return valorActual > 0 ? 100 : 0;
            }

            var porcentaje = ((valorActual - valorAnterior) / valorAnterior) * 100;
            return Math.Round(porcentaje, 1);
        }

        // Metodo auxiliar para obtener metricas globales (solo para administradores)
        public KpisGlobales ObtenerKpisGlobales(DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var kpis = new KpisGlobales();

            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();

                // Si no se especifica rango de fechas, usar mes actual
                if (!fechaDesde.HasValue)
                {
                    fechaDesde = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                }
                if (!fechaHasta.HasValue)
                {
                    fechaHasta = fechaDesde.Value.AddMonths(1).AddDays(-1);
                }

                // Metricas generales
                var sql = @"
                SELECT 
                    COUNT(DISTINCT v.IdUsuario) as VendedoresActivos,
                    COUNT(DISTINCT v.IdCliente) as ClientesUnicos,
                    ISNULL(SUM(CASE WHEN v.Estado = 'Completada' AND v.Tipo = 'Venta' THEN v.Total ELSE 0 END), 0) as VentasTotales,
                    COUNT(CASE WHEN v.Estado = 'Completada' AND v.Tipo = 'Venta' THEN 1 END) as NumeroVentas,
                    (
                        SELECT COUNT(*) 
                        FROM Usuario u 
                        WHERE u.Activo = 1 AND u.IdRol IN (1, 2)
                    ) as TotalVendedores
                FROM Venta v
                WHERE v.FechaVenta >= @FechaDesde AND v.FechaVenta <= @FechaHasta";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@FechaDesde", fechaDesde.Value);
                    cmd.Parameters.AddWithValue("@FechaHasta", fechaHasta.Value);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            kpis.VendedoresActivos = reader.GetInt32(reader.GetOrdinal("VendedoresActivos"));
                            kpis.ClientesUnicos = reader.GetInt32(reader.GetOrdinal("ClientesUnicos"));
                            kpis.VentasTotales = reader.GetDecimal(reader.GetOrdinal("VentasTotales"));
                            kpis.NumeroVentas = reader.GetInt32(reader.GetOrdinal("NumeroVentas"));
                            kpis.TotalVendedores = reader.GetInt32(reader.GetOrdinal("TotalVendedores"));
                            
                            // Calcular productividad
                            kpis.ProductividadPromedio = kpis.TotalVendedores > 0 ? 
                                Math.Round(kpis.VentasTotales / kpis.TotalVendedores, 2) : 0;
                        }
                    }
                }

                // Top vendedores del periodo
                var sqlTop = @"
                SELECT TOP 5
                    u.Nombre + ' ' + u.Apellido as NombreVendedor,
                    ISNULL(SUM(CASE WHEN v.Estado = 'Completada' AND v.Tipo = 'Venta' THEN v.Total ELSE 0 END), 0) as TotalVentas,
                    COUNT(CASE WHEN v.Estado = 'Completada' AND v.Tipo = 'Venta' THEN 1 END) as NumeroVentas
                FROM Usuario u
                LEFT JOIN Venta v ON u.IdUsuario = v.IdUsuario 
                    AND v.FechaVenta >= @FechaDesde 
                    AND v.FechaVenta <= @FechaHasta
                WHERE u.Activo = 1 AND u.IdRol IN (1, 2)
                GROUP BY u.IdUsuario, u.Nombre, u.Apellido
                ORDER BY TotalVentas DESC";

                using (var cmd = new SqlCommand(sqlTop, cn))
                {
                    cmd.Parameters.AddWithValue("@FechaDesde", fechaDesde.Value);
                    cmd.Parameters.AddWithValue("@FechaHasta", fechaHasta.Value);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            kpis.TopVendedores.Add(new TopVendedor
                            {
                                Nombre = reader.GetString(reader.GetOrdinal("NombreVendedor")),
                                TotalVentas = reader.GetDecimal(reader.GetOrdinal("TotalVentas")),
                                NumeroVentas = reader.GetInt32(reader.GetOrdinal("NumeroVentas"))
                            });
                        }
                    }
                }
            }

            return kpis;
        }

        public int Agregar(Venta venta)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                using (var transaction = cn.BeginTransaction())
                {
                    try
                    {
                        // Generar numero de venta
                        if (string.IsNullOrEmpty(venta.NumeroVenta))
                        {
                            venta.NumeroVenta = GenerarNumeroVenta(cn, transaction, venta.Tipo);
                        }

                        // Validar stock disponible antes de proceder (solo para ventas, no cotizaciones)
                        if (venta.Tipo == "Venta" && venta.Estado == "Completada" && venta.Detalles != null)
                        {
                            foreach (var detalle in venta.Detalles)
                            {
                                var sqlStock = "SELECT Stock FROM Producto WHERE IdProducto = @IdProducto";
                                using (var cmdStock = new SqlCommand(sqlStock, cn, transaction))
                                {
                                    cmdStock.Parameters.AddWithValue("@IdProducto", detalle.IdProducto);
                                    var stockActual = Convert.ToInt32(cmdStock.ExecuteScalar() ?? 0);
                                    
                                    if (stockActual < detalle.Cantidad)
                                    {
                                        // Obtener nombre del producto para el error
                                        var sqlNombre = "SELECT Nombre FROM Producto WHERE IdProducto = @IdProducto";
                                        using (var cmdNombre = new SqlCommand(sqlNombre, cn, transaction))
                                        {
                                            cmdNombre.Parameters.AddWithValue("@IdProducto", detalle.IdProducto);
                                            var nombreProducto = cmdNombre.ExecuteScalar()?.ToString() ?? "Producto desconocido";
                                            throw new InvalidOperationException($"Stock insuficiente para {nombreProducto}. Stock disponible: {stockActual}, cantidad solicitada: {detalle.Cantidad}");
                                        }
                                    }
                                }
                            }
                        }

                        // Insertar venta
                        var sql = @"
                        INSERT INTO Venta (NumeroVenta, IdUsuario, IdCliente, FechaVenta, Tipo, Estado, Total, Observaciones, FechaCreacion)
                        VALUES (@NumeroVenta, @IdUsuario, @IdCliente, @FechaVenta, @Tipo, @Estado, @Total, @Observaciones, @FechaCreacion);
                        SELECT SCOPE_IDENTITY();";

                        int idVenta;
                        using (var cmd = new SqlCommand(sql, cn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@NumeroVenta", venta.NumeroVenta);
                            cmd.Parameters.AddWithValue("@IdUsuario", venta.IdUsuario);
                            cmd.Parameters.AddWithValue("@IdCliente", venta.IdCliente);
                            cmd.Parameters.AddWithValue("@FechaVenta", venta.FechaVenta);
                            cmd.Parameters.AddWithValue("@Tipo", venta.Tipo);
                            cmd.Parameters.AddWithValue("@Estado", venta.Estado);
                            cmd.Parameters.AddWithValue("@Total", venta.Total);
                            cmd.Parameters.AddWithValue("@Observaciones", venta.Observaciones ?? "");
                            cmd.Parameters.AddWithValue("@FechaCreacion", DateTime.Now);

                            idVenta = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // Insertar detalles y actualizar stock si existen
                        if (venta.Detalles != null)
                        {
                            foreach (var detalle in venta.Detalles)
                            {
                                // Insertar detalle de venta
                                var sqlDetalle = @"
                                INSERT INTO DetalleVenta (IdVenta, IdProducto, Cantidad, PrecioUnitario)
                                VALUES (@IdVenta, @IdProducto, @Cantidad, @PrecioUnitario)";

                                using (var cmdDetalle = new SqlCommand(sqlDetalle, cn, transaction))
                                {
                                    cmdDetalle.Parameters.AddWithValue("@IdVenta", idVenta);
                                    cmdDetalle.Parameters.AddWithValue("@IdProducto", detalle.IdProducto);
                                    cmdDetalle.Parameters.AddWithValue("@Cantidad", detalle.Cantidad);
                                    cmdDetalle.Parameters.AddWithValue("@PrecioUnitario", detalle.PrecioUnitario);
                                    cmdDetalle.ExecuteNonQuery();
                                }

                                // Actualizar stock del producto (solo para ventas completadas, no para cotizaciones)
                                if (venta.Tipo == "Venta" && venta.Estado == "Completada")
                                {
                                    // Primero verificar que el producto existe
                                    var sqlVerificar = "SELECT COUNT(*) FROM Producto WHERE IdProducto = @IdProducto";
                                    using (var cmdVerificar = new SqlCommand(sqlVerificar, cn, transaction))
                                    {
                                        cmdVerificar.Parameters.AddWithValue("@IdProducto", detalle.IdProducto);
                                        var existe = Convert.ToInt32(cmdVerificar.ExecuteScalar()) > 0;
                                        
                                        if (!existe)
                                        {
                                            throw new InvalidOperationException($"El producto con ID {detalle.IdProducto} no existe en la base de datos");
                                        }
                                    }

                                    var sqlActualizarStock = @"
                                    UPDATE Producto 
                                    SET Stock = Stock - @CantidadVendida,
                                        Actualizado = @FechaActualizacion
                                    WHERE IdProducto = @IdProducto";

                                    using (var cmdStock = new SqlCommand(sqlActualizarStock, cn, transaction))
                                    {
                                        cmdStock.Parameters.AddWithValue("@CantidadVendida", detalle.Cantidad);
                                        cmdStock.Parameters.AddWithValue("@IdProducto", detalle.IdProducto);
                                        cmdStock.Parameters.AddWithValue("@FechaActualizacion", DateTime.Now);
                                        
                                        var filasActualizadas = cmdStock.ExecuteNonQuery();
                                        if (filasActualizadas == 0)
                                        {
                                            throw new InvalidOperationException($"No se pudo actualizar el stock del producto con ID: {detalle.IdProducto}");
                                        }
                                    }

                                    // Registrar movimiento de stock solo si la tabla existe
                                    try
                                    {
                                        // Primero verificar si la tabla MovimientoStock existe
                                        var sqlExisteTabla = @"
                                        SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                                        WHERE TABLE_NAME = 'MovimientoStock'";
                                        
                                        using (var cmdExisteTabla = new SqlCommand(sqlExisteTabla, cn, transaction))
                                        {
                                            var tablaExiste = Convert.ToInt32(cmdExisteTabla.ExecuteScalar()) > 0;
                                            
                                            if (tablaExiste)
                                            {
                                                var sqlMovimiento = @"
                                                INSERT INTO MovimientoStock (IdProducto, Fecha, Tipo, Cantidad, Origen, OrigenId, Observacion)
                                                VALUES (@IdProducto, @Fecha, @Tipo, @Cantidad, @Origen, @OrigenId, @Observacion)";

                                                using (var cmdMovimiento = new SqlCommand(sqlMovimiento, cn, transaction))
                                                {
                                                    cmdMovimiento.Parameters.AddWithValue("@IdProducto", detalle.IdProducto);
                                                    cmdMovimiento.Parameters.AddWithValue("@Fecha", DateTime.Now);
                                                    cmdMovimiento.Parameters.AddWithValue("@Tipo", "S"); // S = Salida de stock
                                                    cmdMovimiento.Parameters.AddWithValue("@Cantidad", detalle.Cantidad);
                                                    cmdMovimiento.Parameters.AddWithValue("@Origen", "Venta");
                                                    cmdMovimiento.Parameters.AddWithValue("@OrigenId", idVenta);
                                                    cmdMovimiento.Parameters.AddWithValue("@Observacion", $"Venta #{venta.NumeroVenta}");
                                                    
                                                    cmdMovimiento.ExecuteNonQuery();
                                                }
                                            }
                                            // Si la tabla no existe, simplemente continuamos sin registrar el movimiento
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // Log del error pero no rompemos la transacci�n
                                        System.Diagnostics.Debug.WriteLine($"Warning: No se pudo registrar el movimiento de stock: {ex.Message}");
                                        // Continuamos sin registrar el movimiento de stock
                                    }
                                }
                            }
                        }

                        transaction.Commit();
                        return idVenta;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private string GenerarNumeroVenta(SqlConnection cn, SqlTransaction transaction, string tipo)
        {
            var prefijo = tipo == "Cotización" ? "COT" : "SALE";
            var year = DateTime.Now.Year;
            
            var sql = $@"
            SELECT ISNULL(MAX(CAST(RIGHT(NumeroVenta, 3) AS INT)), 0) + 1 
            FROM Venta 
            WHERE NumeroVenta LIKE '{prefijo}-{year}-%'";

            using (var cmd = new SqlCommand(sql, cn, transaction))
            {
                var siguiente = Convert.ToInt32(cmd.ExecuteScalar());
                return $"{prefijo}-{year}-{siguiente:D3}";
            }
        }

        public List<Usuario> ObtenerVendedores()
        {
            var vendedores = new List<Usuario>();

            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                
                var sql = @"
                SELECT 
                    u.IdUsuario,
                    u.Nombre,
                    u.Apellido,
                    u.Email,
                    u.IdRol,
                    r.NombreRol as RolNombre
                FROM Usuario u
                INNER JOIN Rol r ON u.IdRol = r.IdRol
                WHERE u.Activo = 1 AND (u.IdRol = 1 OR u.IdRol = 2)
                ORDER BY u.Apellido, u.Nombre";

                using (var cmd = new SqlCommand(sql, cn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        vendedores.Add(new Usuario
                        {
                            IdUsuario = reader.GetInt32(reader.GetOrdinal("IdUsuario")),
                            Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                            Apellido = reader.GetString(reader.GetOrdinal("Apellido")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            IdRol = reader.GetInt32(reader.GetOrdinal("IdRol")),
                            RolNombre = reader.GetString(reader.GetOrdinal("RolNombre"))
                        });
                    }
                }
            }

            return vendedores;
        }

        public List<Cliente> ObtenerClientes(bool soloActivos = true)
        {
            var clientes = new List<Cliente>();

            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                
                var sql = @"
                SELECT 
                    c.IdCliente,
                    c.Tipo,
                    ISNULL(c.NombreCliente, '') as NombreCliente,
                    ISNULL(c.ApellidoCliente, '') as ApellidoCliente,
                    ISNULL(c.RazonSocial, '') as RazonSocial,
                    ISNULL(c.Direccion, '') as Direccion,
                    ISNULL(c.CUIT, '') as CUIT,
                    ISNULL(c.CUIL, '') as CUIL,
                    c.FechaAlta,
                    c.Activo,
                    ISNULL(ce.Email, '') as Email,
                    ISNULL(ct.Telefono, '') as Telefono
                FROM Cliente c
                LEFT JOIN ClienteEmail ce ON c.IdCliente = ce.IdCliente AND ce.EsPrincipal = 1
                LEFT JOIN ClienteTelefono ct ON c.IdCliente = ct.IdCliente AND ct.EsPrincipal = 1
                WHERE 1=1";

                if (soloActivos)
                    sql += " AND c.Activo = 1";

                sql += " ORDER BY c.FechaAlta DESC";

                using (var cmd = new SqlCommand(sql, cn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var cliente = new Cliente
                        {
                            IdCliente = reader.GetInt32(reader.GetOrdinal("IdCliente")),
                            Tipo = reader.GetString(reader.GetOrdinal("Tipo")),
                            NombreCliente = reader.GetString(reader.GetOrdinal("NombreCliente")),
                            ApellidoCliente = reader.GetString(reader.GetOrdinal("ApellidoCliente")),
                            RazonSocial = reader.GetString(reader.GetOrdinal("RazonSocial")),
                            Direccion = reader.GetString(reader.GetOrdinal("Direccion")),
                            CUIT = reader.GetString(reader.GetOrdinal("CUIT")),
                            CUIL = reader.GetString(reader.GetOrdinal("CUIL")),
                            FechaAlta = reader.GetDateTime(reader.GetOrdinal("FechaAlta")),
                            Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
                            EmailPrincipal = reader.GetString(reader.GetOrdinal("Email")),
                            TelefonoPrincipal = reader.GetString(reader.GetOrdinal("Telefono"))
                        };

                        clientes.Add(cliente);
                    }
                }
            }

            return clientes;
        }

        public Venta ObtenerPorId(int idVenta)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                
                var sql = @"
                SELECT 
                    v.IdVenta,
                    v.NumeroVenta,
                    v.IdUsuario,
                    u.Nombre + ' ' + u.Apellido AS NombreVendedor,
                    v.IdCliente,
                    CASE 
                        WHEN c.Tipo = 'PF' THEN ISNULL(c.NombreCliente, '') + ' ' + ISNULL(c.ApellidoCliente, '')
                        WHEN c.Tipo = 'PJ' THEN ISNULL(c.RazonSocial, '')
                        ELSE 'Cliente #' + CAST(c.IdCliente AS VARCHAR(10))
                    END AS NombreCliente,
                    CASE 
                        WHEN c.Tipo = 'PJ' THEN ISNULL(c.RazonSocial, '')
                        ELSE ''
                    END AS EmpresaCliente,
                    v.FechaVenta,
                    v.Tipo,
                    v.Estado,
                    v.Total,
                    v.Observaciones,
                    v.FechaCreacion,
                    v.FechaActualizacion
                FROM Venta v
                INNER JOIN Usuario u ON v.IdUsuario = u.IdUsuario
                LEFT JOIN Cliente c ON v.IdCliente = c.IdCliente
                WHERE v.IdVenta = @IdVenta";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@IdVenta", idVenta);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Venta
                            {
                                IdVenta = reader.GetInt32(reader.GetOrdinal("IdVenta")),
                                NumeroVenta = reader.IsDBNull(reader.GetOrdinal("NumeroVenta")) ? "" : reader.GetString(reader.GetOrdinal("NumeroVenta")),
                                IdUsuario = reader.GetInt32(reader.GetOrdinal("IdUsuario")),
                                NombreVendedor = reader.IsDBNull(reader.GetOrdinal("NombreVendedor")) ? "" : reader.GetString(reader.GetOrdinal("NombreVendedor")),
                                IdCliente = reader.GetInt32(reader.GetOrdinal("IdCliente")),
                                NombreCliente = reader.IsDBNull(reader.GetOrdinal("NombreCliente")) ? "" : reader.GetString(reader.GetOrdinal("NombreCliente")),
                                EmpresaCliente = reader.IsDBNull(reader.GetOrdinal("EmpresaCliente")) ? "" : reader.GetString(reader.GetOrdinal("EmpresaCliente")),
                                FechaVenta = reader.GetDateTime(reader.GetOrdinal("FechaVenta")),
                                Tipo = reader.IsDBNull(reader.GetOrdinal("Tipo")) ? "" : reader.GetString(reader.GetOrdinal("Tipo")),
                                Estado = reader.IsDBNull(reader.GetOrdinal("Estado")) ? "" : reader.GetString(reader.GetOrdinal("Estado")),
                                Total = reader.GetDecimal(reader.GetOrdinal("Total")),
                                Observaciones = reader.IsDBNull(reader.GetOrdinal("Observaciones")) ? "" : reader.GetString(reader.GetOrdinal("Observaciones")),
                                FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion")),
                                FechaActualizacion = reader.IsDBNull(reader.GetOrdinal("FechaActualizacion")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("FechaActualizacion"))
                            };
                        }
                    }
                }
            }
            return null;
        }

        public bool Actualizar(Venta venta)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                
                var sql = @"
                UPDATE Venta 
                SET 
                    IdCliente = @IdCliente,
                    FechaVenta = @FechaVenta,
                    Tipo = @Tipo,
                    Estado = @Estado,
                    Total = @Total,
                    Observaciones = @Observaciones,
                    FechaActualizacion = @FechaActualizacion
                WHERE IdVenta = @IdVenta";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@IdVenta", venta.IdVenta);
                    cmd.Parameters.AddWithValue("@IdCliente", venta.IdCliente);
                    cmd.Parameters.AddWithValue("@FechaVenta", venta.FechaVenta);
                    cmd.Parameters.AddWithValue("@Tipo", venta.Tipo);
                    cmd.Parameters.AddWithValue("@Estado", venta.Estado);
                    cmd.Parameters.AddWithValue("@Total", venta.Total);
                    cmd.Parameters.AddWithValue("@Observaciones", venta.Observaciones ?? "");
                    cmd.Parameters.AddWithValue("@FechaActualizacion", DateTime.Now);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public List<DetalleVenta> ObtenerDetalles(int idVenta)
        {
            var detalles = new List<DetalleVenta>();

            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                
                var sql = @"
                SELECT 
                    dv.IdVenta,
                    dv.IdProducto,
                    dv.Cantidad,
                    dv.PrecioUnitario,
                    p.Nombre as NombreProducto,
                    p.Sku as SkuProducto
                FROM DetalleVenta dv
                INNER JOIN Producto p ON dv.IdProducto = p.IdProducto
                WHERE dv.IdVenta = @IdVenta
                ORDER BY dv.IdProducto";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@IdVenta", idVenta);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            detalles.Add(new DetalleVenta
                            {
                                IdVenta = reader.GetInt32(reader.GetOrdinal("IdVenta")),
                                IdProducto = reader.GetInt32(reader.GetOrdinal("IdProducto")),
                                Cantidad = reader.GetInt32(reader.GetOrdinal("Cantidad")),
                                PrecioUnitario = reader.GetDecimal(reader.GetOrdinal("PrecioUnitario")),
                                NombreProducto = reader.IsDBNull(reader.GetOrdinal("NombreProducto")) ? "" : reader.GetString(reader.GetOrdinal("NombreProducto")),
                                SkuProducto = reader.IsDBNull(reader.GetOrdinal("SkuProducto")) ? "" : reader.GetString(reader.GetOrdinal("SkuProducto"))
                            });
                        }
                    }
                }
            }

            return detalles;
        }
    }
}