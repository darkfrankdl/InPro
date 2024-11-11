using Npgsql;

namespace sensade_project
{
    public class DatabaseHelper
    {
        private string _connectionString;

        public DatabaseHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Execute a query that does not return data (e.g., INSERT, UPDATE, DELETE)
        public int ExecuteNonQuery(string query, params NpgsqlParameter[] parameters)
        {
            int result = -1;
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddRange(parameters);
                    result = cmd.ExecuteNonQuery();
                }
            }
            return result;
        }

        // Execute a query that returns data (e.g., SELECT)
        public List<Dictionary<string, object>> ExecuteQuery(string query, params NpgsqlParameter[] parameters)
        {
            var results = new List<Dictionary<string, object>>();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddRange(parameters);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row[reader.GetName(i)] = reader.GetValue(i);
                            }
                            results.Add(row);
                        }
                    }
                }
            }
            return results;
        }
    }
}
