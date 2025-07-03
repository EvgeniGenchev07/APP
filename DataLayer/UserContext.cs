using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
namespace DataLayer
{
    public class UserContext
    {
        private readonly EapDbContext _eapDbContext;
        public UserContext(EapDbContext eapDbContext)
        {
            _eapDbContext = eapDbContext ?? throw new ArgumentNullException(nameof(eapDbContext));
        }
        public string ReadAll() {
            if (_eapDbContext.IsConnect())
            {
                MySqlConnector.MySqlCommand command = new MySqlConnector.MySqlCommand("SELECT * FROM User", _eapDbContext.Connection);
                using (var read = command.ExecuteReader())
                {
                    //MD5 md5 = MD5.Create();
                    string result = "";
                    while (read.Read())
                    {
                        for (int i = 0; i < read.FieldCount; i++)
                        {
                            result += ($"{read.GetName(i)}: {read[i]}");
                        }
                    }
                    _eapDbContext.Close();
                    return result;
                }
            }
            else
            {
                return "Failed to connect to the database.";
            }
        }
    }
}
