using System;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Proyecto_Taller_2.Data
{

    public sealed class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _cs;

        public SqlConnectionFactory(string name = "ERP")
        {
            var item = ConfigurationManager.ConnectionStrings[name]
            _cs = item.ConnectionString;
        }
    }
}
