using BusinessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer
{
    public class HolidayDayContext
    {
        private readonly EapDbContext _eapDbContext;

        public HolidayDayContext(EapDbContext eapDbContext)
        {
            _eapDbContext = eapDbContext ?? throw new ArgumentNullException(nameof(eapDbContext));
        }

        public bool Create(HolidayDay holiday)
        {
            if (_eapDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlConnector.MySqlCommand(
                        "INSERT INTO HolidayDay (name, date) " +
                        "VALUES (@name, @date)",
                        _eapDbContext.Connection);

                    command.Parameters.AddWithValue("@name", holiday.Name);
                    command.Parameters.AddWithValue("@date", holiday.Date);

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

        public bool Update(HolidayDay holiday)
        {
            if (_eapDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlConnector.MySqlCommand(
                        "UPDATE HolidayDay SET name = @name, date = @date" +
                        "WHERE id = @id",
                        _eapDbContext.Connection);

                    command.Parameters.AddWithValue("@name", holiday.Name);
                    command.Parameters.AddWithValue("@date", holiday.Date);
                    command.Parameters.AddWithValue("@id", holiday.Id);

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

        public bool Delete(int holidayDayId)
        {
            if (_eapDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlConnector.MySqlCommand(
                        "DELETE FROM HolidayDay WHERE id = @id",
                        _eapDbContext.Connection);

                    command.Parameters.AddWithValue("@id", holidayDayId);

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


        public List<HolidayDay> GetAll()
        {
            var holidays = new List<HolidayDay>();
            if (_eapDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlConnector.MySqlCommand(
                        "SELECT * FROM Holiday ORDER BY created DESC",
                        _eapDbContext.Connection);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            holidays.Add(new HolidayDay
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Name = reader["name"].ToString(),
                                Date = Convert.ToDateTime(reader["date"]),
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    _eapDbContext.Close();
                }
            }
            return holidays;
        }
    }
}
