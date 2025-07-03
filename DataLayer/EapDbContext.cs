using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer
{
    public class EapDbContext
    {
        private readonly MySqlConnection _connection;
        public MySqlConnection Connection => _connection;
        public EapDbContext(MySqlConnection connection) 
        {
            _connection = connection;
        }

        public bool IsConnect()
        {
            if (Connection == null)
            {
                return false;
            }
            _connection.Open();
            return true;
        }

        public void Close()
        {
            Connection.Close();
        }
    }
}
