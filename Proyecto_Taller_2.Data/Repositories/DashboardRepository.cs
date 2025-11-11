using Proyecto_Taller_2.Data.Repositories.Interfaces;
using Proyecto_Taller_2.Domain.Models.Dtos;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Proyecto_Taller_2.Data.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly string _connectionString;

        public DashboardRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<DashboardHomeDto> ObtenerDatosHomeAsync()
        {
            var data = new DashboardHomeDto();
            var inicioMesActual = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var inicioMesAnterior = inicioMesActual.AddMonths(-1);
            var finMesAnterior = inicioMesActual.AddSeconds(-1);

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // 1. KPIs PRINCIPALES (Ventas, Órdenes, Ticket)
                string sqlKpis = @"
                    SELECT
                        -- Ventas
                        ISNULL(SUM(CASE WHEN FechaVenta >= @InicioMes THEN Total ELSE 0 END), 0) as VentasActual,
                        ISNULL(SUM(CASE WHEN FechaVenta BETWEEN @InicioMesAnt AND @FinMesAnt THEN Total ELSE 0 END), 0) as VentasAnterior,
                        -- Órdenes Activas (Pendientes)
                        COUNT(CASE WHEN FechaVenta >= @InicioMes AND Estado = 'Pendiente' THEN 1 END) as OrdenesActual,
                        COUNT(CASE WHEN FechaVenta BETWEEN @InicioMesAnt AND @FinMesAnt AND Estado = 'Pendiente' THEN 1 END) as OrdenesAnterior,
                        -- Cantidad Ventas (para Ticket Promedio)
                        COUNT(CASE WHEN FechaVenta >= @InicioMes AND Estado = 'Completada' THEN 1 END) as CantidadVentasActual,
                        COUNT(CASE WHEN FechaVenta BETWEEN @InicioMesAnt AND @FinMesAnt AND Estado = 'Completada' THEN 1 END) as CantidadVentasAnterior
                    FROM Venta";

                using (var cmd = new SqlCommand(sqlKpis, conn))
                {
                    cmd.Parameters.AddWithValue("@InicioMes", inicioMesActual);
                    cmd.Parameters.AddWithValue("@InicioMesAnt", inicioMesAnterior);
                    cmd.Parameters.AddWithValue("@FinMesAnt", finMesAnterior);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            data.VentasTotales = reader.GetDecimal(0);
                            decimal ventasAnt = reader.GetDecimal(1);
                            data.PorcentajeVentasVsAnterior = CalcularVariacion(data.VentasTotales, ventasAnt);

                            data.OrdenesActivas = reader.GetInt32(2);
                            int ordenesAnt = reader.GetInt32(3);
                            data.PorcentajeOrdenesVsAnterior = CalcularVariacion(data.OrdenesActivas, ordenesAnt);

                            int cantVentasActual = reader.GetInt32(4);
                            int cantVentasAnt = reader.GetInt32(5);

                            data.TicketPromedio = cantVentasActual > 0 ? data.VentasTotales / cantVentasActual : 0;
                            decimal ticketAnt = cantVentasAnt > 0 ? ventasAnt / cantVentasAnt : 0;
                            data.PorcentajeTicketVsAnterior = CalcularVariacion(data.TicketPromedio, ticketAnt);
                        }
                    }
                }

                // 2. KPI VENDEDORES
                // Asumo que tienes una tabla Usuario con roles. Ajusta 'IdRol' si es necesario para identificar vendedores.
                string sqlVendedores = @"
                    SELECT
                        (SELECT COUNT(*) FROM Usuario WHERE Activo = 1) as TotalUsuarios,
                        (SELECT COUNT(DISTINCT IdUsuario) FROM Venta WHERE FechaVenta >= @InicioMes) as VendedoresActivos";
                using (var cmd = new SqlCommand(sqlVendedores, conn))
                {
                    cmd.Parameters.AddWithValue("@InicioMes", inicioMesActual);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            data.TotalVendedores = reader.GetInt32(0);
                            data.VendedoresActivos = reader.GetInt32(1);
                            if (data.VendedoresActivos > 0)
                                data.ProductividadPromedio = data.VentasTotales / data.VendedoresActivos;
                        }
                    }
                }

                // 3. GRÁFICO EVOLUCIÓN (Últimos 6 meses)
                for (int i = 5; i >= 0; i--)
                {
                    var mes = DateTime.Now.AddMonths(-i);
                    var inicio = new DateTime(mes.Year, mes.Month, 1);
                    var fin = inicio.AddMonths(1).AddSeconds(-1);

                    string sqlMes = "SELECT ISNULL(SUM(Total), 0) FROM Venta WHERE FechaVenta BETWEEN @Inicio AND @Fin AND Estado = 'Completada'";
                    using (var cmd = new SqlCommand(sqlMes, conn))
                    {
                        cmd.Parameters.AddWithValue("@Inicio", inicio);
                        cmd.Parameters.AddWithValue("@Fin", fin);
                        decimal totalMes = (decimal)await cmd.ExecuteScalarAsync();
                        data.EvolucionVentas.Add(new VentaMensualDto { Mes = mes.ToString("MMM"), TotalVenta = totalMes });
                    }
                }

                // 4. TOP VENDEDORES
                string sqlTop = @"
                    SELECT TOP 3 u.Nombre + ' ' + u.Apellido as Vendedor, COUNT(v.IdVenta) as Cantidad, ISNULL(SUM(v.Total), 0) as Total
                    FROM Usuario u
                    LEFT JOIN Venta v ON u.IdUsuario = v.IdUsuario AND v.FechaVenta >= @InicioMes AND v.Estado = 'Completada'
                    WHERE u.Activo = 1 -- Opcional: filtrar por rol de vendedor si tienes
                    GROUP BY u.IdUsuario, u.Nombre, u.Apellido
                    ORDER BY Total DESC";
                using (var cmd = new SqlCommand(sqlTop, conn))
                {
                    cmd.Parameters.AddWithValue("@InicioMes", inicioMesActual);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            data.TopVendedores.Add(new TopVendedorDto
                            {
                                Nombre = reader.GetString(0),
                                CantidadVentas = reader.GetInt32(1),
                                TotalFacturado = reader.GetDecimal(2)
                            });
                        }
                    }
                }

                // 5. INVENTARIO POR CATEGORÍA & STOCK BAJO
                string sqlStock = @"
                    SELECT c.Nombre, SUM(p.Stock) as StockActual
                    FROM Categoria c
                    LEFT JOIN Producto p ON c.IdCategoria = p.IdCategoria AND p.Activo = 1
                    WHERE c.Activo = 1
                    GROUP BY c.IdCategoria, c.Nombre";
                using (var cmd = new SqlCommand(sqlStock, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int stock = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                            data.InventarioPorCategoria.Add(new InventarioCategoriaDto
                            {
                                NombreCategoria = reader.GetString(0),
                                StockActual = stock,
                                StockEsperado = Math.Max(stock + 50, 100) // Meta simulada: un poco más de lo que hay
                            });
                        }
                    }
                }

                string sqlAlertas = "SELECT COUNT(*) FROM Producto WHERE Stock <= Minimo AND Activo = 1";
                using (var cmd = new SqlCommand(sqlAlertas, conn))
                {
                    data.CantidadStockBajo = (int)await cmd.ExecuteScalarAsync();
                }
            }

            return data;
        }

        private decimal CalcularVariacion(decimal actual, decimal anterior)
        {
            if (anterior == 0) return actual > 0 ? 100 : 0;
            return ((actual - anterior) / anterior) * 100;
        }
    }
}