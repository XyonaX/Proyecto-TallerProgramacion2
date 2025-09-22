using System;
using System.Configuration;
using Microsoft.Data.SqlClient;

namespace Proyecto_Taller_2.Data
{
    public interface ISqlConnectionFactory
    {
        SqlConnection Create();
    }

    public sealed class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _cs;

        public SqlConnectionFactory(string name = "ERP")
        {
            var item = ConfigurationManager.ConnectionStrings[name]
                ?? throw new InvalidOperationException(
                    $"Falta connectionStrings['{name}'] en App.config del proyecto UI.");

            _cs = item.ConnectionString;
        }

        public SqlConnection Create()
        {
            var cn = new SqlConnection(_cs);
            cn.Open();
            return cn;
        }
    }
}
