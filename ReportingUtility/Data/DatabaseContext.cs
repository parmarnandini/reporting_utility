using Microsoft.Data.SqlClient;


namespace ReportingUtility.Data
{
    public class DatabaseContext
    {
        private readonly string _connectionString;

        public DatabaseContext(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("connection") ?? throw new ArgumentNullException(nameof(_connectionString));
        }

        public System.Data.IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}
