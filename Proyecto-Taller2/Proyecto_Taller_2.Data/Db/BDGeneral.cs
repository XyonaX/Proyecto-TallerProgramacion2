using System.Data.SqlClient;
using System.Configuration;

public static class BDGeneral
{
    public static SqlConnection GetConnection()
    {
        // Usa el nombre correcto de la cadena de conexión definida en App.config
        string connectionString = ConfigurationManager.ConnectionStrings["ERP"].ConnectionString;
        var cn = new SqlConnection(connectionString);
        cn.Open();
        return cn;
    }
}
