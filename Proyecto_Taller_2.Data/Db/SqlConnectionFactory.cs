using System;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Proyecto_Taller_2.Data.Db   // Nota: Db (no DB)
{
    public interface ISqlConnectionFactory { IDbConnection Create(); }

    public sealed class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _cs;
        public SqlConnectionFactory(string name = "ERP")
        {
            var item = ConfigurationManager.ConnectionStrings[name]
                ?? throw new InvalidOperationException($"Falta connectionStrings['{name}'] en App.config del proyecto UI.");
            _cs = item.ConnectionString;
        }
        public IDbConnection Create() => new SqlConnection(_cs);
    }
}
