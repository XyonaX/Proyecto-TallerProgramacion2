using Microsoft.Data.SqlClient;
using Proyecto_Taller_2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Proyecto_Taller_2.Data.Repositories
{
    public class ClienteRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public ClienteRepository()
        {
            _connectionFactory = new SqlConnectionFactory("ERP"); // Lee App.config
        }

        /// <summary>
        /// Obtiene la lista completa de clientes, aplicando filtros opcionales.
        /// </summary>
        public async Task<List<Cliente>> GetAllClientesCompletosAsync(string filtroTexto = null, string filtroSegmento = null, string filtroEstado = null)
        {
            var clientes = new Dictionary<int, Cliente>();
            var parameters = new List<SqlParameter>();
            var whereClauses = new List<string>();

            // --- Construcción dinámica del WHERE ---
            if (!string.IsNullOrWhiteSpace(filtroTexto))
            {
                whereClauses.Add(@"(
                    c.NombreCliente LIKE @FiltroTexto OR c.ApellidoCliente LIKE @FiltroTexto OR
                    c.RazonSocial LIKE @FiltroTexto OR
                    EXISTS (SELECT 1 FROM ClienteEmail ce WHERE ce.IdCliente = c.IdCliente AND ce.Email LIKE @FiltroTexto)
                )");
                parameters.Add(new SqlParameter("@FiltroTexto", $"%{filtroTexto}%"));
            }
            if (!string.IsNullOrWhiteSpace(filtroEstado) && filtroEstado != "Todos los estados")
            {
                whereClauses.Add("c.Activo = @Activo");
                parameters.Add(new SqlParameter("@Activo", filtroEstado.Equals("Activo", StringComparison.OrdinalIgnoreCase)));
            }
            string sqlBase = @"SELECT c.* FROM Cliente c ";
            string sqlWhere = whereClauses.Count > 0 ? " WHERE " + string.Join(" AND ", whereClauses) : "";
            string sqlClientes = sqlBase + sqlWhere;
            // --- Fin Construcción dinámica del WHERE ---

            using (var cn = _connectionFactory.Create())
            {
                // --- 1. Obtener los Clientes FILTRADOS ---
                using (var cmdClientes = new SqlCommand(sqlClientes, cn))
                {
                    cmdClientes.Parameters.AddRange(parameters.ToArray());
                    using (var reader = await cmdClientes.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var cliente = MapCliente(reader);
                            if (!clientes.ContainsKey(cliente.IdCliente)) { clientes.Add(cliente.IdCliente, cliente); }
                        }
                    }
                }

                if (!clientes.Any()) return new List<Cliente>();
                var clienteIds = string.Join(",", clientes.Keys);

                // --- 2. Obtener Emails, Teléfonos y Ventas SOLO para los clientes encontrados ---
                var cmdEmails = new SqlCommand($"SELECT * FROM ClienteEmail WHERE IdCliente IN ({clienteIds})", cn);
                using (var reader = await cmdEmails.ExecuteReaderAsync()) { while (await reader.ReadAsync()) { var email = MapEmail(reader); if (clientes.TryGetValue(email.IdCliente, out var cliente)) cliente.Emails.Add(email); } }
                var cmdTelefonos = new SqlCommand($"SELECT * FROM ClienteTelefono WHERE IdCliente IN ({clienteIds})", cn);
                using (var reader = await cmdTelefonos.ExecuteReaderAsync()) { while (await reader.ReadAsync()) { var telefono = MapTelefono(reader); if (clientes.TryGetValue(telefono.IdCliente, out var cliente)) cliente.Telefonos.Add(telefono); } }
                var cmdVentas = new SqlCommand($"SELECT * FROM Venta WHERE IdCliente IN ({clienteIds})", cn);
                using (var reader = await cmdVentas.ExecuteReaderAsync()) { while (await reader.ReadAsync()) { var venta = MapVenta(reader); if (clientes.TryGetValue(venta.IdCliente, out var cliente)) cliente.Ventas.Add(venta); } }

            } // Conexión cerrada

            // --- Filtro por Segmento (post-procesamiento) ---
            List<Cliente> clientesFiltradosFinal = clientes.Values.ToList();
            if (!string.IsNullOrWhiteSpace(filtroSegmento) && filtroSegmento != "Todos los segmentos")
            {
                clientesFiltradosFinal = clientesFiltradosFinal.Where(c => {
                    decimal total = c.Ventas.Sum(v => v.Total); string segmentoCalculado = total > 15000 ? "VIP" : total > 2000 ? "Regular" : "Premium";
                    return segmentoCalculado.Equals(filtroSegmento, StringComparison.OrdinalIgnoreCase);
                }).ToList();
            }
            return clientesFiltradosFinal;
        }


        // ==========================================================
        // === MAPPERS (Convierten datos crudos en objetos)       ===
        // ==========================================================

        private Cliente MapCliente(SqlDataReader reader)
        {
            return new Cliente
            {
                IdCliente = reader["IdCliente"] is DBNull ? 0 : (int)reader["IdCliente"],
                Tipo = reader["Tipo"] as string ?? "",
                NombreCliente = reader["NombreCliente"] is DBNull ? null : (string)reader["NombreCliente"],
                ApellidoCliente = reader["ApellidoCliente"] is DBNull ? null : (string)reader["ApellidoCliente"],
                RazonSocial = reader["RazonSocial"] is DBNull ? null : (string)reader["RazonSocial"],
                Direccion = reader["Direccion"] is DBNull ? null : (string)reader["Direccion"],
                CUIT = reader["CUIT"] is DBNull ? null : (string)reader["CUIT"],
                CUIL = reader["CUIL"] is DBNull ? null : (string)reader["CUIL"],
                FechaAlta = reader["FechaAlta"] is DBNull ? DateTime.MinValue : (DateTime)reader["FechaAlta"],
                Activo = reader["Activo"] is DBNull ? false : (bool)reader["Activo"]
            };
        }
        private ClienteEmail MapEmail(SqlDataReader reader)
        {
            return new ClienteEmail
            {
                IdCliente = reader["IdCliente"] is DBNull ? 0 : (int)reader["IdCliente"],
                Email = reader["Email"] as string ?? "",
                EsPrincipal = reader["EsPrincipal"] is DBNull ? false : (bool)reader["EsPrincipal"]
            };
        }
        private ClienteTelefono MapTelefono(SqlDataReader reader)
        {
            return new ClienteTelefono
            {
                IdCliente = reader["IdCliente"] is DBNull ? 0 : (int)reader["IdCliente"],
                Telefono = reader["Telefono"] as string ?? "",
                Tipo = reader["Tipo"] as string ?? "Otro",
                EsPrincipal = reader["EsPrincipal"] is DBNull ? false : (bool)reader["EsPrincipal"]
            };
        }
        private Venta MapVenta(SqlDataReader reader)
        {
            return new Venta
            {
                IdVenta = reader["IdVenta"] is DBNull ? 0 : (int)reader["IdVenta"],
                IdUsuario = reader["IdUsuario"] is DBNull ? 0 : (int)reader["IdUsuario"],
                IdCliente = reader["IdCliente"] is DBNull ? 0 : (int)reader["IdCliente"],
                FechaVenta = reader["FechaVenta"] is DBNull ? DateTime.MinValue : (DateTime)reader["FechaVenta"],
                NumeroVenta = reader["NumeroVenta"] as string ?? "",
                Tipo = reader["Tipo"] as string ?? "",
                Estado = reader["Estado"] as string ?? "",
                Total = reader["Total"] is DBNull ? 0 : (decimal)reader["Total"],
                Observaciones = reader["Observaciones"] is DBNull ? null : (string)reader["Observaciones"],
                FechaCreacion = reader["FechaCreacion"] is DBNull ? DateTime.MinValue : (DateTime)reader["FechaCreacion"],
                FechaActualizacion = reader["FechaActualizacion"] is DBNull ? null : (DateTime?)reader["FechaActualizacion"]
            };
        }

        // ==========================================================
        // === MÉTODOS PARA MODIFICAR DATOS (Insert, Update, Activo) ===
        // ==========================================================

        public async Task SetActivoAsync(int idCliente, bool nuevoEstado)
        {
            using (var cn = _connectionFactory.Create())
            {
                string sql = "UPDATE Cliente SET Activo = @Activo WHERE IdCliente = @IdCliente";
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@Activo", nuevoEstado);
                    cmd.Parameters.AddWithValue("@IdCliente", idCliente);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Actualiza un cliente existente, incluyendo su email y teléfono principal.
        /// Utiliza una transacción para asegurar la atomicidad.
        /// </summary>
        public async Task UpdateClienteAsync(Cliente cliente, string nuevoEmail, string nuevoTelefono)
        {
            using (var cn = _connectionFactory.Create())
            using (var tran = cn.BeginTransaction()) // Iniciar Transacción
            {
                try
                {
                    // --- 1. Actualizar Tabla Cliente ---
                    string sqlCliente = @"UPDATE Cliente
                                          SET NombreCliente = @Nombre, ApellidoCliente = @Apellido,
                                              RazonSocial = @RazonSocial, Direccion = @Direccion,
                                              CUIL = @CUIL, CUIT = @CUIT
                                          WHERE IdCliente = @IdCliente";
                    using (var cmd = new SqlCommand(sqlCliente, cn, tran))
                    {
                        cmd.Parameters.AddWithValue("@Nombre", (object)cliente.NombreCliente ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Apellido", (object)cliente.ApellidoCliente ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@RazonSocial", (object)cliente.RazonSocial ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Direccion", (object)cliente.Direccion ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CUIL", (object)cliente.CUIL ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CUIT", (object)cliente.CUIT ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IdCliente", cliente.IdCliente);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // --- 2. Actualizar/Insertar Email Principal ---
                    await ActualizarOInsertarContactoPrincipalAsync(cn, tran, cliente.IdCliente, "ClienteEmail", "Email", nuevoEmail);

                    // --- 3. Actualizar/Insertar Teléfono Principal ---
                    await ActualizarOInsertarContactoPrincipalAsync(cn, tran, cliente.IdCliente, "ClienteTelefono", "Telefono", nuevoTelefono, true); // Indicar que es Teléfono


                    // --- 4. Confirmar Transacción ---
                    tran.Commit(); // Si todo fue bien, guardar cambios
                }
                catch (Exception)
                {
                    tran.Rollback(); // Si algo falló, deshacer todo
                    throw; // Re-lanzar la excepción
                }
            } // Conexión y transacción cerradas/descartadas
        }

        /// <summary>
        /// Método auxiliar para actualizar o insertar el email o teléfono principal.
        /// </summary>
        private async Task ActualizarOInsertarContactoPrincipalAsync(SqlConnection cn, SqlTransaction tran, int idCliente, string tabla, string columnaValor, string nuevoValor, bool esTelefono = false)
        {
            // --- Lógica para quitar el principal actual si el nuevo valor es vacío ---
            if (string.IsNullOrWhiteSpace(nuevoValor))
            {
                string sqlUnset = $"UPDATE {tabla} SET EsPrincipal = 0 WHERE IdCliente = @IdCliente AND EsPrincipal = 1";
                using (var cmdUnset = new SqlCommand(sqlUnset, cn, tran))
                {
                    cmdUnset.Parameters.AddWithValue("@IdCliente", idCliente);
                    await cmdUnset.ExecuteNonQueryAsync();
                }
                return; // No hacer más nada si el nuevo valor es vacío
            }


            // Buscar contacto principal actual
            string sqlFind = $"SELECT {columnaValor} FROM {tabla} WHERE IdCliente = @IdCliente AND EsPrincipal = 1";
            string valorActual = null;
            using (var cmdFind = new SqlCommand(sqlFind, cn, tran))
            {
                cmdFind.Parameters.AddWithValue("@IdCliente", idCliente);
                var result = await cmdFind.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                {
                    valorActual = (string)result;
                }
            }

            // Desmarcar cualquier otro principal ANTES de continuar (simplifica la lógica)
            string sqlUnsetOthers = $"UPDATE {tabla} SET EsPrincipal = 0 WHERE IdCliente = @IdCliente AND EsPrincipal = 1 AND {columnaValor} != @NuevoValor";
            using (var cmdUnset = new SqlCommand(sqlUnsetOthers, cn, tran))
            {
                cmdUnset.Parameters.AddWithValue("@IdCliente", idCliente);
                cmdUnset.Parameters.AddWithValue("@NuevoValor", nuevoValor); // No desmarcar el que vamos a marcar/insertar
                await cmdUnset.ExecuteNonQueryAsync();
            }


            // Verificar si el nuevo valor ya existe para este cliente
            string sqlCheckExists = $"SELECT COUNT(1) FROM {tabla} WHERE IdCliente = @IdCliente AND {columnaValor} = @NuevoValor";
            int existe = 0;
            using (var cmdCheck = new SqlCommand(sqlCheckExists, cn, tran))
            {
                cmdCheck.Parameters.AddWithValue("@IdCliente", idCliente);
                cmdCheck.Parameters.AddWithValue("@NuevoValor", nuevoValor);
                existe = (int)await cmdCheck.ExecuteScalarAsync();
            }

            if (existe > 0)
            {
                // Si ya existe, simplemente marcarlo como principal
                string sqlMarkPrincipal = $"UPDATE {tabla} SET EsPrincipal = 1 WHERE IdCliente = @IdCliente AND {columnaValor} = @NuevoValor";
                using (var cmdMark = new SqlCommand(sqlMarkPrincipal, cn, tran))
                {
                    cmdMark.Parameters.AddWithValue("@IdCliente", idCliente);
                    cmdMark.Parameters.AddWithValue("@NuevoValor", nuevoValor);
                    await cmdMark.ExecuteNonQueryAsync();
                }
            }
            else
            {
                // Si no existe, insertarlo como nuevo y principal
                string columnasAdicionales = esTelefono ? ", Tipo" : "";
                string valoresAdicionales = esTelefono ? ", @Tipo" : "";
                string sqlInsert = $"INSERT INTO {tabla} (IdCliente, {columnaValor}, EsPrincipal{columnasAdicionales}) VALUES (@IdCliente, @NuevoValor, 1{valoresAdicionales})";
                using (var cmdInsert = new SqlCommand(sqlInsert, cn, tran))
                {
                    cmdInsert.Parameters.AddWithValue("@IdCliente", idCliente);
                    cmdInsert.Parameters.AddWithValue("@NuevoValor", nuevoValor);
                    if (esTelefono) cmdInsert.Parameters.AddWithValue("@Tipo", "Móvil"); // Asumir tipo Móvil
                    await cmdInsert.ExecuteNonQueryAsync();
                }
            }
        }


        public async Task InsertClienteAsync(Cliente cliente, string email, string telefono)
        {
            using (var cn = _connectionFactory.Create())
            using (var tran = cn.BeginTransaction())
            {
                try
                {
                    string sqlCliente = @"INSERT INTO Cliente (Tipo, NombreCliente, ApellidoCliente, RazonSocial, Direccion, CUIT, CUIL, FechaAlta, Activo)
                                          OUTPUT INSERTED.IdCliente
                                          VALUES (@Tipo, @Nombre, @Apellido, @RazonSocial, @Direccion, @CUIT, @CUIL, @FechaAlta, @Activo)";
                    int nuevoClienteId;
                    using (var cmd = new SqlCommand(sqlCliente, cn, tran))
                    {
                        cmd.Parameters.AddWithValue("@Tipo", cliente.Tipo);
                        cmd.Parameters.AddWithValue("@Nombre", (object)cliente.NombreCliente ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Apellido", (object)cliente.ApellidoCliente ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@RazonSocial", (object)cliente.RazonSocial ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Direccion", (object)cliente.Direccion ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CUIT", (object)cliente.CUIT ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CUIL", (object)cliente.CUIL ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@FechaAlta", DateTime.Now);
                        cmd.Parameters.AddWithValue("@Activo", true);
                        nuevoClienteId = (int)await cmd.ExecuteScalarAsync();
                    }
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        string sqlEmail = "INSERT INTO ClienteEmail (IdCliente, Email, EsPrincipal) VALUES (@IdCliente, @Email, @EsPrincipal)";
                        using (var cmd = new SqlCommand(sqlEmail, cn, tran))
                        {
                            cmd.Parameters.AddWithValue("@IdCliente", nuevoClienteId);
                            cmd.Parameters.AddWithValue("@Email", email);
                            cmd.Parameters.AddWithValue("@EsPrincipal", true);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(telefono))
                    {
                        string sqlTel = "INSERT INTO ClienteTelefono (IdCliente, Telefono, Tipo, EsPrincipal) VALUES (@IdCliente, @Telefono, @Tipo, @EsPrincipal)";
                        using (var cmd = new SqlCommand(sqlTel, cn, tran))
                        {
                            cmd.Parameters.AddWithValue("@IdCliente", nuevoClienteId);
                            cmd.Parameters.AddWithValue("@Telefono", telefono);
                            cmd.Parameters.AddWithValue("@Tipo", "Móvil");
                            cmd.Parameters.AddWithValue("@EsPrincipal", true);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                    tran.Commit();
                }
                catch (Exception)
                {
                    tran.Rollback();
                    throw;
                }
            }
        }


        // ==========================================================
        // === MÉTODOS DE VALIDACIÓN                              ===
        // ==========================================================

        public async Task<bool> CheckDniExistsAsync(string cuil, string cuit, int clienteIdActual)
        {
            if (string.IsNullOrWhiteSpace(cuil) && string.IsNullOrWhiteSpace(cuit)) return false;
            using (var cn = _connectionFactory.Create())
            {
                string sql = "SELECT COUNT(1) FROM Cliente WHERE (CUIL = @CUIL OR CUIT = @CUIT) AND IdCliente != @IdCliente";
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@CUIL", string.IsNullOrWhiteSpace(cuil) ? (object)DBNull.Value : cuil);
                    cmd.Parameters.AddWithValue("@CUIT", string.IsNullOrWhiteSpace(cuit) ? (object)DBNull.Value : cuit);
                    cmd.Parameters.AddWithValue("@IdCliente", clienteIdActual);
                    int count = (int)await cmd.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }

        public async Task<bool> CheckEmailExistsAsync(string email, int clienteIdActual)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            using (var cn = _connectionFactory.Create())
            {
                string sql = "SELECT COUNT(1) FROM ClienteEmail WHERE Email = @Email AND IdCliente != @IdCliente";
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@IdCliente", clienteIdActual);
                    int count = (int)await cmd.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }
    }
}