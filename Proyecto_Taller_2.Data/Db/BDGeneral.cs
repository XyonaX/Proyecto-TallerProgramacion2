using System.Configuration;
using System.Data.SqlClient;

namespace Proyecto_Taller_2.Data
{
    public static class BDGeneral
    {
        public static SqlConnection GetConnection()
        {
            string cs = ConfigurationManager.ConnectionStrings["ERP"].ConnectionString;
            return new SqlConnection(cs);
        }

        public static string ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["ERP"].ConnectionString;
            }
        }
    }
}
