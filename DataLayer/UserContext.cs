using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Data;
using BusinessLayer;

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
        public bool Create(User user)
        {
            if (_eapDbContext.IsConnect())
            {
                try
                {
              
                    if (string.IsNullOrEmpty(user.Id))
                    {
                        user.Id = Guid.NewGuid().ToString();
                    }


                    var command = new MySqlConnector.MySqlCommand(
                        "INSERT INTO User (Id, Name, Email, Password, Role, AbsenceDays) " +
                        "VALUES (@id, @name, @email, @password, @role, @absenceDays)",
                        _eapDbContext.Connection);

                    command.Parameters.AddWithValue("@id", user.Id);
                    command.Parameters.AddWithValue("@name", user.Name);
                    command.Parameters.AddWithValue("@email", user.Email);
                    command.Parameters.AddWithValue("@password", user.Password);
                    command.Parameters.AddWithValue("@role", user.Role);
                    command.Parameters.AddWithValue("@absenceDays", user.AbsenceDays);

                    int rowsAffected = command.ExecuteNonQuery();
                    _eapDbContext.Close();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    
                    _eapDbContext.Close();
                    return false;
                }
            }
            return false;
        }

        public bool Update(User user)
        {
            if (_eapDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlConnector.MySqlCommand(
                        "UPDATE User SET Name = @name, Email = @email, Role = @role, " +
                        "AbsenceDays = @absenceDays WHERE Id = @id",
                        _eapDbContext.Connection);

                    command.Parameters.AddWithValue("@name", user.Name);
                    command.Parameters.AddWithValue("@email", user.Email);
                    command.Parameters.AddWithValue("@role", user.Role.ToString());
                    command.Parameters.AddWithValue("@absenceDays", user.AbsenceDays);
                    command.Parameters.AddWithValue("@id", user.Id);

                    int rowsAffected = command.ExecuteNonQuery();
                    _eapDbContext.Close();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    // Log the exception
                    _eapDbContext.Close();
                    return false;
                }
            }
            return false;
        }

        public bool Delete(string userId)
        {
            if (_eapDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlConnector.MySqlCommand(
                        "DELETE FROM User WHERE Id = @id",
                        _eapDbContext.Connection);

                    command.Parameters.AddWithValue("@id", userId);

                    int rowsAffected = command.ExecuteNonQuery();
                    _eapDbContext.Close();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    // Log the exception
                    _eapDbContext.Close();
                    return false;
                }
            }
            return false;
        }

        public User GetById(string userId)
        {
            if (_eapDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlConnector.MySqlCommand(
                        "SELECT * FROM User WHERE Id = @id",
                        _eapDbContext.Connection);

                    command.Parameters.AddWithValue("@id", userId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                Id = reader["Id"].ToString(),
                                Name = reader["Name"].ToString(),
                                Email = reader["Email"].ToString(),
                                Role = Enum.Parse<Role>(reader["Role"].ToString()),
                                AbsenceDays = Convert.ToInt32(reader["AbsenceDays"])
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception
                }
                finally
                {
                    _eapDbContext.Close();
                }
            }
            return null;
        }

        public User GetByEmail(string email)
        {
            if (_eapDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlConnector.MySqlCommand(
                        "SELECT * FROM User WHERE Email = @email",
                        _eapDbContext.Connection);

                    command.Parameters.AddWithValue("@email", email);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                Id = reader["Id"].ToString(),
                                Name = reader["Name"].ToString(),
                                Email = reader["Email"].ToString(),
                                Role = Enum.Parse<Role>(reader["Role"].ToString()),
                                AbsenceDays = Convert.ToInt32(reader["AbsenceDays"]),
                                Password = reader["Password"].ToString() // Only for authentication purposes
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception
                }
                finally
                {
                    _eapDbContext.Close();
                }
            }
            return null;
        }

        
        private string GetPasswordHash(string userId)
        {
            if (_eapDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlConnector.MySqlCommand(
                        "SELECT Password FROM User WHERE Id = @id",
                        _eapDbContext.Connection);

                    command.Parameters.AddWithValue("@id", userId);

                    var result = command.ExecuteScalar();
                    _eapDbContext.Close();
                    return result?.ToString();
                }
                catch (Exception ex)
                {
                    _eapDbContext.Close();
                    return null;
                }
            }
            return null;
        }

    }
}
