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
        public List<User> ReadAll() {
            var users = new List<User>();

            if (_eapDbContext.IsConnect())
            {
                try
                {
                    MySqlConnector.MySqlCommand command = new MySqlConnector.MySqlCommand("SELECT * FROM User", _eapDbContext.Connection);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new User
                            {
                                Id = reader["Id"].ToString(),
                                Name = reader["Name"].ToString(),
                                Email = reader["Email"].ToString(),
                                Password = reader["Password"].ToString(),
                                Role = (Role)Convert.ToInt32(reader["Role"]),
                                AbsenceDays = Convert.ToInt32(reader["AbsenceDays"])
                            });
                        }
                    }
                    _eapDbContext.Close();
                    return users;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading users: {ex.Message}");
                    _eapDbContext.Close();
                    return new List<User>();
                }
            }
            else
            {
                Console.WriteLine("Failed to connect to the database.");
                return new List<User>();
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
                    command.Parameters.AddWithValue("@role", (int)user.Role);
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
                    command.Parameters.AddWithValue("@role", (int)user.Role);
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
