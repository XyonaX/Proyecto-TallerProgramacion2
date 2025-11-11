using Proyecto_Taller_2.Data.Repositories.Interfaces;
using Proyecto_Taller_2.Domain.Models.Dtos;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Taller_2.Data.Repositories
{
    public class ReporteRepository : IReporteRepository
    {
        private readonly string _connectionString;

        public ReporteRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<DashboardReporteDto> ObtenerDatosDashboardAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var dashboard = new DashboardReporteDto();
            TimeSpan duracionPeriodo = fechaFin - fechaInicio;
            DateTime fechaInicioAnterior = fechaInicio.AddMonths(-1);
            DateTime fechaFinAnterior = fechaInicio.AddSeconds(-1);

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // KPI 1: VENTAS
                string sqlVentas = @"SELECT ISNULL(SUM(CASE WHEN FechaVenta BETWEEN @FechaInicio AND @FechaFin THEN Total ELSE 0 END),0), ISNULL(SUM(CASE WHEN FechaVenta BETWEEN @FechaInicioAnt AND @FechaFinAnt THEN Total ELSE 0 END),0), COUNT(CASE WHEN FechaVenta BETWEEN @FechaInicio AND @FechaFin THEN 1 END) FROM Venta WHERE Estado = 'Completada'";
                using (var cmd = new SqlCommand(sqlVentas, connection))
                {
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin);
                    cmd.Parameters.AddWithValue("@FechaInicioAnt", fechaInicioAnterior);
                    cmd.Parameters.AddWithValue("@FechaFinAnt", fechaFinAnterior);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            dashboard.IngresosMensuales = reader.GetDecimal(0);
                            dashboard.TotalVentasPeriodo = dashboard.IngresosMensuales;
                            decimal ventasAnterior = reader.GetDecimal(1);
                            dashboard.CantidadVentasPeriodo = reader.GetInt32(2);
                            dashboard.PorcentajeVariacionIngreso = ventasAnterior > 0 ? ((dashboard.IngresosMensuales - ventasAnterior) / ventasAnterior) * 100 : (dashboard.IngresosMensuales > 0 ? 100 : 0);
                        }
                    }
                }

                // KPI 2: MARGEN BRUTO (Cálculo directo en SQL para mayor precisión)
                string sqlMargen = @"
                    SELECT
                        ISNULL(SUM(dv.Cantidad * dv.PrecioUnitario), 0) as TotalVenta,
                        ISNULL(SUM(dv.Cantidad * p.Costo), 0) as TotalCosto,
                        CASE WHEN SUM(dv.Cantidad * dv.PrecioUnitario) > 0
                             THEN ((SUM(dv.Cantidad * dv.PrecioUnitario) - ISNULL(SUM(dv.Cantidad * p.Costo), 0)) / SUM(dv.Cantidad * dv.PrecioUnitario)) * 100
                             ELSE 0
                        END as MargenPorcentaje
                    FROM DetalleVenta dv
                    INNER JOIN Venta v ON dv.IdVenta = v.IdVenta
                    INNER JOIN Producto p ON dv.IdProducto = p.IdProducto
                    WHERE v.FechaVenta BETWEEN @FechaInicio AND @FechaFin AND v.Estado = 'Completada'";

                using (var cmd = new SqlCommand(sqlMargen, connection))
                {
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            // Leemos directamente la 3ra columna (índice 2) que ya tiene el porcentaje calculado por SQL
                            dashboard.MargenBruto = reader.GetDecimal(2);
                        }
                    }
                }

                // KPI 3: CLIENTES
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Cliente WHERE Activo = 1", connection))
                {
                    dashboard.ClientesActivos = (int)await cmd.ExecuteScalarAsync();
                }

                string sqlNuevos = "SELECT COUNT(*) FROM Cliente WHERE Activo = 1 AND FechaAlta BETWEEN @FechaInicio AND @FechaFin";
                try
                {
                    using (var cmd = new SqlCommand(sqlNuevos, connection))
                    {
                        cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                        cmd.Parameters.AddWithValue("@FechaFin", fechaFin);
                        dashboard.NuevosClientesEsteMes = (int)await cmd.ExecuteScalarAsync();
                    }
                }
                catch { dashboard.NuevosClientesEsteMes = 0; }

                // KPI 4: INVENTARIO
                using (var cmd = new SqlCommand(@"SELECT ISNULL(SUM(Stock * Costo),0), COUNT(*), COUNT(CASE WHEN Stock <= Minimo THEN 1 END) FROM Producto WHERE Activo = 1", connection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            dashboard.ValorTotalInventario = reader.GetDecimal(0);
                            dashboard.CantidadProductosActivos = reader.GetInt32(1);
                            dashboard.StockCritico = reader.GetInt32(2);
                        }
                    }
                }

                // LISTADO ÚLTIMAS VENTAS
                using (var cmd = new SqlCommand(@"SELECT TOP 10 FechaVenta, Estado, NumeroVenta FROM Venta WHERE FechaVenta BETWEEN @FechaInicio AND @FechaFin ORDER BY FechaVenta DESC", connection))
                {
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            dashboard.ReportesRecientes.Add(new ReporteRecienteDto
                            {
                                Fecha = reader.GetDateTime(0).ToShortDateString(),
                                Estado = reader.GetString(1),
                                FechaGeneracion = $"Venta #{reader.GetString(2)}"
                            });
                        }
                    }
                }

                // Metas simuladas
                dashboard.MetaMensual = 50000m;
                if (dashboard.MetaMensual > 0)
                    dashboard.PorcentajeMeta = (dashboard.IngresosMensuales / dashboard.MetaMensual) * 100;
            }
            return dashboard;
        }

        public async Task<string> ObtenerStockBajoAsync()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("SELECT Nombre, Stock, Minimo FROM Producto WHERE Stock <= Minimo AND Activo = 1", conn);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    var sb = new StringBuilder("PRODUCTOS CON STOCK BAJO:\n\n");
                    bool hayDatos = false;
                    while (await reader.ReadAsync())
                    {
                        sb.AppendLine($"- {reader["Nombre"]}: {reader["Stock"]} unidades (Mínimo: {reader["Minimo"]})");
                        hayDatos = true;
                    }
                    return hayDatos ? sb.ToString() : "No hay productos con stock bajo.";
                }
            }
        }

        public async Task<string> ObtenerTopProductosAsync()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string sql = @"SELECT TOP 5 p.Nombre, SUM(dv.Cantidad) as TotalVendido FROM DetalleVenta dv INNER JOIN Venta v ON dv.IdVenta = v.IdVenta INNER JOIN Producto p ON dv.IdProducto = p.IdProducto WHERE v.FechaVenta >= @InicioMes AND v.Estado = 'Completada' GROUP BY p.Nombre ORDER BY TotalVendido DESC";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@InicioMes", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1));
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        var sb = new StringBuilder($"TOP 5 PRODUCTOS ({DateTime.Now:MMMM yyyy}):\n\n");
                        bool hayDatos = false;
                        while (await reader.ReadAsync())
                        {
                            sb.AppendLine($"{reader["TotalVendido"]}x - {reader["Nombre"]}");
                            hayDatos = true;
                        }
                        return hayDatos ? sb.ToString() : "No hay ventas suficientes este mes.";
                    }
                }
            }
        }

        public async Task<string> ObtenerVentasHoyAsync()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string sql = @"SELECT NumeroVenta, Total, Estado FROM Venta WHERE CAST(FechaVenta AS DATE) = CAST(GETDATE() AS DATE)";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        var sb = new StringBuilder($"VENTAS DE HOY ({DateTime.Now:dd/MM/yyyy}):\n\n");
                        decimal totalDia = 0;
                        int count = 0;
                        while (await reader.ReadAsync())
                        {
                            sb.AppendLine($"#{reader["NumeroVenta"]} - ${reader.GetDecimal(1):N2} ({reader["Estado"]})");
                            if (reader["Estado"].ToString() == "Completada") totalDia += reader.GetDecimal(1);
                            count++;
                        }
                        sb.AppendLine($"\nTotal Completado Hoy: ${totalDia:N2}");
                        return count > 0 ? sb.ToString() : "No se han registrado ventas hoy.";
                    }
                }
            }
        }
    }
}