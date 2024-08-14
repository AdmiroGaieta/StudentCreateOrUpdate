using System.Data.SqlClient;

namespace StudentCreateOrUpdate.Data
{
    public static class SqlConnectionHelper
    {
        public static SqlConnection GetSqlConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }
}
