using Microsoft.Data.SqlClient;

namespace Proyecto_Taller_2.Data
{
    public static class BDGeneral
    {
        private static readonly ISqlConnectionFactory _factory = new SqlConnectionFactory("ERP");

        public static SqlConnection GetConnection()
        {
            return _factory.Create();
        }
    }
}
