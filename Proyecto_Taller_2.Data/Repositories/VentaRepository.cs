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

                // KPIs del período actual
                var sql = @"
                SELECT 
                    COUNT(*) as TotalVentas,
                    ISNULL(SUM(CASE WHEN Estado = 'Completada' THEN Total ELSE 0 END), 0) as VentasCompletadas,
                    ISNULL(AVG(CASE WHEN Estado = 'Completada' THEN Total ELSE NULL END), 0) as TicketPromedio,
                    COUNT(CASE WHEN Tipo = 'Cotización' THEN 1 END) as TotalCotizaciones,
                    COUNT(CASE WHEN Tipo = 'Cotización' AND Estado = 'Pendiente' THEN 1 END) as CotizacionesPendientes
                FROM Venta v
                WHERE 1=1";

                if (idUsuario.HasValue)
                    sql += " AND v.IdUsuario = @IdUsuario";
                
                if (fechaDesde.HasValue)
                    sql += " AND v.FechaVenta >= @FechaDesde";
                    
                if (fechaHasta.HasValue)
                    sql += " AND v.FechaVenta <= @FechaHasta";

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
                        if (reader.Read())
                        {
                            kpis.TotalOrdenes = reader.GetInt32(reader.GetOrdinal("TotalVentas"));
                            kpis.VentasDelMes = reader.GetDecimal(reader.GetOrdinal("VentasCompletadas"));
                            kpis.TicketPromedio = reader.GetDecimal(reader.GetOrdinal("TicketPromedio"));
                            kpis.TotalCotizaciones = reader.GetInt32(reader.GetOrdinal("TotalCotizaciones"));
                            kpis.CotizacionesPendientes = reader.GetInt32(reader.GetOrdinal("CotizacionesPendientes"));
                        }
                    }
                }

                // Calcular comparaciones con período anterior (simulado)
                kpis.PorcentajeVsAnterior = CalcularPorcentajeCrecimiento(15.5m); // Ejemplo
                kpis.PorcentajeOrdenesAnterior = CalcularPorcentajeCrecimiento(8.2m);
                kpis.PorcentajeTicketAnterior = CalcularPorcentajeCrecimiento(12.1m);
                kpis.PorcentajeCotizacionesAnterior = CalcularPorcentajeCrecimiento(-5.3m);
            }

            return kpis;
        }

        private decimal CalcularPorcentajeCrecimiento(decimal porcentaje)
        {
            return Math.Round(porcentaje, 1);
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
                        // Generar número de venta
                        if (string.IsNullOrEmpty(venta.NumeroVenta))
                        {
                            venta.NumeroVenta = GenerarNumeroVenta(cn, transaction, venta.Tipo);
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

                        // Insertar detalles si existen
                        if (venta.Detalles != null)
                        {
                            foreach (var detalle in venta.Detalles)
                            {
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
                            }
                        }

                        transaction.Commit();
                        return idVenta;
                    }
                    catch
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
    }

    public class KpisVentas
    {
        public decimal VentasDelMes { get; set; }
        public int TotalOrdenes { get; set; }
        public decimal TicketPromedio { get; set; }
        public int TotalCotizaciones { get; set; }
        public int CotizacionesPendientes { get; set; }
        public decimal PorcentajeVsAnterior { get; set; }
        public decimal PorcentajeOrdenesAnterior { get; set; }
        public decimal PorcentajeTicketAnterior { get; set; }
        public decimal PorcentajeCotizacionesAnterior { get; set; }
    }
}