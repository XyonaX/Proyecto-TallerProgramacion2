using Proyecto_Taller_2.Data;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Proyecto_Taller_2.Services
{
    public static class BackupService
    {
        private const string BACKUP_INFO_FILE = "last_backup.txt";

        /// <summary>
        /// Obtiene la carpeta predeterminada de SQL Server para backups
        /// </summary>
        public static string ObtenerCarpetaPredeterminadaSQLServer()
        {
            try
            {
                using (var conn = new SqlConnection(BDGeneral.ConnectionString))
                {
                    conn.Open();
                    var sql = @"
                        DECLARE @defaultBackupPath NVARCHAR(512)
                        EXEC master.dbo.xp_instance_regread 
                            N'HKEY_LOCAL_MACHINE',
                            N'Software\Microsoft\MSSQLServer\MSSQLServer',
                            N'BackupDirectory',
                            @defaultBackupPath OUTPUT
                        SELECT @defaultBackupPath";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            var path = result.ToString();
                            if (Directory.Exists(path))
                            {
                                return path;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Si falla, intentar con la ubicación común de SQL Server
            }

            // Ubicación predeterminada común de SQL Server
            var commonPath = @"C:\Program Files\Microsoft SQL Server\MSSQL15.SQLEXPRESS\MSSQL\Backup";
            if (Directory.Exists(commonPath))
                return commonPath;

            // Otras ubicaciones comunes según versión
            var possiblePaths = new[]
            {
                @"C:\Program Files\Microsoft SQL Server\MSSQL14.SQLEXPRESS\MSSQL\Backup",
                @"C:\Program Files\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSQL\Backup",
                @"C:\Program Files\Microsoft SQL Server\MSSQL12.SQLEXPRESS\MSSQL\Backup",
            };

            foreach (var path in possiblePaths)
            {
                if (Directory.Exists(path))
                    return path;
            }

            return null;
        }

        /// <summary>
        /// Obtiene una carpeta recomendada para backups que SQL Server pueda acceder
        /// </summary>
        public static string ObtenerCarpetaRecomendada()
        {
            // Primero intentar obtener la carpeta predeterminada de SQL Server
            var sqlServerPath = ObtenerCarpetaPredeterminadaSQLServer();
            if (!string.IsNullOrEmpty(sqlServerPath))
            {
                var customFolder = Path.Combine(sqlServerPath, "Proyecto_Taller_2");
                try
                {
                    if (!Directory.Exists(customFolder))
                        Directory.CreateDirectory(customFolder);
                    return customFolder;
                }
                catch
                {
                    // Si no se puede crear, continuar con otras opciones
                }
            }

            // Opción 2: Carpeta en C:\Backups
            var backupsRoot = @"C:\Backups\Proyecto_Taller_2";
            try
            {
                if (!Directory.Exists(backupsRoot))
                    Directory.CreateDirectory(backupsRoot);
                return backupsRoot;
            }
            catch
            {
                // Si falla, intentar con AppData
            }

            // Opción 3: AppData del usuario (siempre debería funcionar)
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Proyecto_Taller_2",
                "Backups");

            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);

            return appDataPath;
        }

        /// <summary>
        /// Valida si la carpeta de backup es adecuada para SQL Server
        /// </summary>
        public static (bool esValida, string mensaje) ValidarCarpetaBackup(string carpeta)
        {
            if (string.IsNullOrEmpty(carpeta))
            {
                return (false, "No se ha especificado una carpeta.");
            }

            // Detectar si es una carpeta de OneDrive o similar
            var carpetaSuperior = carpeta.ToLower();
            if (carpetaSuperior.Contains("onedrive") || 
                carpetaSuperior.Contains("dropbox") || 
                carpetaSuperior.Contains("google drive") ||
                carpetaSuperior.Contains("icloud"))
            {
                var carpetaRecomendada = ObtenerCarpetaRecomendada();
                return (false, 
                    "⚠️ ADVERTENCIA: La carpeta seleccionada está en un servicio de sincronización en la nube.\n\n" +
                    "SQL Server no puede crear backups directamente en estas carpetas.\n\n" +
                    $"💡 SOLUCIÓN AUTOMÁTICA:\n" +
                    $"Haz clic en 'Usar Carpeta Recomendada' y se configurará automáticamente:\n" +
                    $"{carpetaRecomendada}\n\n" +
                    "O selecciona manualmente una carpeta local como C:\\Backups");
            }

            // Verificar permisos de escritura
            try
            {
                if (!Directory.Exists(carpeta))
                {
                    Directory.CreateDirectory(carpeta);
                }

                var archivoTest = Path.Combine(carpeta, $"test_{Guid.NewGuid()}.tmp");
                File.WriteAllText(archivoTest, "test");
                File.Delete(archivoTest);
            }
            catch (Exception ex)
            {
                var carpetaRecomendada = ObtenerCarpetaRecomendada();
                return (false, 
                    $"❌ No se tienen permisos de escritura en la carpeta.\n\n" +
                    $"Error: {ex.Message}\n\n" +
                    $"💡 SOLUCIÓN:\n" +
                    $"Usa la carpeta recomendada:\n{carpetaRecomendada}");
            }

            return (true, "Carpeta válida");
        }

        public static BackupInfo GetLastBackupInfo()
        {
            try
            {
                var settings = SettingsService.Current;
                if (string.IsNullOrEmpty(settings?.CarpetaBackups) || !Directory.Exists(settings.CarpetaBackups))
                {
                    return new BackupInfo { HasBackup = false };
                }

                var infoFile = Path.Combine(settings.CarpetaBackups, BACKUP_INFO_FILE);
                if (File.Exists(infoFile))
                {
                    var lines = File.ReadAllLines(infoFile);
                    if (lines.Length >= 2 && DateTime.TryParse(lines[0], out DateTime fecha))
                    {
                        return new BackupInfo
                        {
                            HasBackup = true,
                            FechaUltimoBackup = fecha,
                            NombreArchivo = lines[1]
                        };
                    }
                }

                // Si no hay archivo de info, buscar el backup más reciente
                var backupFiles = Directory.GetFiles(settings.CarpetaBackups, "*.bak")
                                          .OrderByDescending(f => File.GetCreationTime(f))
                                          .ToList();

                if (backupFiles.Any())
                {
                    var ultimoBackup = backupFiles.First();
                    return new BackupInfo
                    {
                        HasBackup = true,
                        FechaUltimoBackup = File.GetCreationTime(ultimoBackup),
                        NombreArchivo = Path.GetFileName(ultimoBackup)
                    };
                }

                return new BackupInfo { HasBackup = false };
            }
            catch
            {
                return new BackupInfo { HasBackup = false };
            }
        }

        public static bool CrearBackup(out string mensaje)
        {
            mensaje = "";
            try
            {
                var settings = SettingsService.Current;
                if (string.IsNullOrEmpty(settings?.CarpetaBackups))
                {
                    mensaje = "No se ha configurado una carpeta de backups.";
                    return false;
                }

                // Validar carpeta antes de intentar el backup
                var validacion = ValidarCarpetaBackup(settings.CarpetaBackups);
                if (!validacion.esValida)
                {
                    mensaje = validacion.mensaje;
                    return false;
                }

                if (!Directory.Exists(settings.CarpetaBackups))
                {
                    Directory.CreateDirectory(settings.CarpetaBackups);
                }

                var nombreBackup = $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                var rutaCompleta = Path.Combine(settings.CarpetaBackups, nombreBackup);

                // Obtener el nombre de la base de datos de la cadena de conexión
                var builder = new SqlConnectionStringBuilder(BDGeneral.ConnectionString);
                var nombreBD = builder.InitialCatalog;

                if (string.IsNullOrEmpty(nombreBD))
                {
                    mensaje = "No se pudo determinar el nombre de la base de datos.";
                    return false;
                }

                using (var conn = new SqlConnection(BDGeneral.ConnectionString))
                {
                    conn.Open();

                    var sql = $@"
                        BACKUP DATABASE [{nombreBD}] 
                        TO DISK = @RutaBackup 
                        WITH FORMAT, 
                        MEDIANAME = 'ProyectoTaller2Backup',
                        NAME = 'Backup completo de {nombreBD}';";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.CommandTimeout = 300; // 5 minutos
                        cmd.Parameters.AddWithValue("@RutaBackup", rutaCompleta);
                        cmd.ExecuteNonQuery();
                    }
                }

                // Guardar información del último backup
                var infoFile = Path.Combine(settings.CarpetaBackups, BACKUP_INFO_FILE);
                File.WriteAllLines(infoFile, new[] 
                { 
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), 
                    nombreBackup 
                });

                // Limpiar backups antiguos (mantener solo los últimos 10)
                LimpiarBackupsAntiguos(settings.CarpetaBackups, 10);

                mensaje = $"✅ Backup creado exitosamente:\n{nombreBackup}\n\nUbicación:\n{settings.CarpetaBackups}";
                return true;
            }
            catch (SqlException ex)
            {
                // Errores específicos de SQL Server
                if (ex.Message.Contains("Operating system error 5") || ex.Message.Contains("Acceso denegado"))
                {
                    var carpetaRecomendada = ObtenerCarpetaRecomendada();
                    mensaje = "❌ SQL Server no tiene permisos para escribir en la carpeta seleccionada.\n\n" +
                              $"💡 SOLUCIÓN:\n" +
                              $"Usa la carpeta recomendada:\n{carpetaRecomendada}\n\n" +
                              "O configura permisos manualmente (requiere privilegios de administrador).";
                }
                else if (ex.Message.Contains("Cannot open backup device"))
                {
                    var carpetaRecomendada = ObtenerCarpetaRecomendada();
                    mensaje = "❌ No se puede acceder a la carpeta de destino.\n\n" +
                              "Causas comunes:\n" +
                              "• La carpeta está en OneDrive/Dropbox\n" +
                              "• SQL Server no tiene permisos\n" +
                              "• La ruta es demasiado larga\n\n" +
                              $"💡 SOLUCIÓN:\n" +
                              $"Usa la carpeta recomendada:\n{carpetaRecomendada}";
                }
                else
                {
                    mensaje = $"Error de SQL Server:\n{ex.Message}\n\nCódigo de error: {ex.Number}";
                }
                return false;
            }
            catch (Exception ex)
            {
                mensaje = $"Error inesperado al crear backup:\n{ex.Message}";
                return false;
            }
        }

        private static void LimpiarBackupsAntiguos(string carpeta, int mantener)
        {
            try
            {
                var backups = Directory.GetFiles(carpeta, "*.bak")
                                      .Select(f => new FileInfo(f))
                                      .OrderByDescending(f => f.CreationTime)
                                      .ToList();

                if (backups.Count > mantener)
                {
                    foreach (var backup in backups.Skip(mantener))
                    {
                        try
                        {
                            backup.Delete();
                        }
                        catch
                        {
                            // Ignorar errores al eliminar backups antiguos
                        }
                    }
                }
            }
            catch
            {
                // Ignorar errores en la limpieza
            }
        }

        public static bool RestaurarBackup(string rutaBackup, out string mensaje)
        {
            mensaje = "";
            try
            {
                if (!File.Exists(rutaBackup))
                {
                    mensaje = "El archivo de backup no existe.";
                    return false;
                }

                var builder = new SqlConnectionStringBuilder(BDGeneral.ConnectionString);
                var nombreBD = builder.InitialCatalog;

                if (string.IsNullOrEmpty(nombreBD))
                {
                    mensaje = "No se pudo determinar el nombre de la base de datos.";
                    return false;
                }

                using (var conn = new SqlConnection(BDGeneral.ConnectionString))
                {
                    conn.Open();

                    // Poner la BD en modo single user para poder restaurar
                    var sqlSingleUser = $@"
                        ALTER DATABASE [{nombreBD}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";

                    using (var cmd = new SqlCommand(sqlSingleUser, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Restaurar el backup
                    var sqlRestore = $@"
                        RESTORE DATABASE [{nombreBD}] 
                        FROM DISK = @RutaBackup 
                        WITH REPLACE;";

                    using (var cmd = new SqlCommand(sqlRestore, conn))
                    {
                        cmd.CommandTimeout = 300; // 5 minutos
                        cmd.Parameters.AddWithValue("@RutaBackup", rutaBackup);
                        cmd.ExecuteNonQuery();
                    }

                    // Volver a modo multi user
                    var sqlMultiUser = $@"
                        ALTER DATABASE [{nombreBD}] SET MULTI_USER;";

                    using (var cmd = new SqlCommand(sqlMultiUser, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                mensaje = "Base de datos restaurada exitosamente.";
                return true;
            }
            catch (Exception ex)
            {
                mensaje = $"Error al restaurar backup: {ex.Message}";
                return false;
            }
        }
    }

    public class BackupInfo
    {
        public bool HasBackup { get; set; }
        public DateTime FechaUltimoBackup { get; set; }
        public string NombreArchivo { get; set; }

        public string GetTiempoTranscurrido()
        {
            if (!HasBackup) return "Nunca";

            var tiempo = DateTime.Now - FechaUltimoBackup;

            if (tiempo.TotalMinutes < 1)
                return "Hace menos de 1 minuto";
            if (tiempo.TotalMinutes < 60)
                return $"Hace {(int)tiempo.TotalMinutes} minuto{((int)tiempo.TotalMinutes != 1 ? "s" : "")}";
            if (tiempo.TotalHours < 24)
                return $"Hace {(int)tiempo.TotalHours} hora{((int)tiempo.TotalHours != 1 ? "s" : "")}";
            if (tiempo.TotalDays < 30)
                return $"Hace {(int)tiempo.TotalDays} día{((int)tiempo.TotalDays != 1 ? "s" : "")}";
            if (tiempo.TotalDays < 365)
            {
                var meses = (int)(tiempo.TotalDays / 30);
                return $"Hace {meses} mes{(meses != 1 ? "es" : "")}";
            }

            var años = (int)(tiempo.TotalDays / 365);
            return $"Hace {años} año{(años != 1 ? "s" : "")}";
        }
    }
}
