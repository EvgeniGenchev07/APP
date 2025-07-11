﻿using BusinessLayer;

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
                        "INSERT INTO HolidayDay (name, date, isCustom) " +
                        "VALUES (@name, @date, @isCustom)",
                        _eapDbContext.Connection);

                    command.Parameters.AddWithValue("@name", holiday.Name);
                    command.Parameters.AddWithValue("@date", holiday.Date);
                    command.Parameters.AddWithValue("@isCustom", holiday.IsCustom);

                    int rowsAffected = command.ExecuteNonQuery();
                    _eapDbContext.Close();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    _eapDbContext.Close();
                    throw new Exception("Error creating holiday in the database.", ex);
                }
            }
            throw new Exception("Database connection is not established.");
        }

        public List<HolidayDay> GetAll()
        {
            var holidays = new List<HolidayDay>();
            if (_eapDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlConnector.MySqlCommand(
                        "SELECT * FROM HolidayDay ORDER BY date DESC",
                        _eapDbContext.Connection);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            holidays.Add(new HolidayDay
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Name = reader["name"].ToString(),
                                IsCustom = Convert.ToBoolean(reader["isCustom"]),
                                Date = Convert.ToDateTime(reader["date"]),
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error retrieving holidays from the database.", ex);
                }
                finally
                {
                    _eapDbContext.Close();
                }
                int year = DateTime.Now.Year;

                holidays.AddRange(InitializeOfficialHolidays(year - 1).Result);
                holidays.AddRange(InitializeOfficialHolidays(year).Result);
                holidays.AddRange(InitializeOfficialHolidays(year + 1).Result);
                return holidays;
            }
            throw new Exception("Database connection is not established.");
        }
        private DateTime CalculateOrthodoxEaster(int year)
        {
            int a = year % 4;
            int b = year % 7;
            int c = year % 19;
            int d = (19 * c + 15) % 30;
            int e = (2 * a + 4 * b - d + 34) % 7;
            int month = (int)Math.Floor((d + e + 114) / 31M);
            int day = ((d + e + 114) % 31) + 1;

            DateTime easter = new DateTime(year, month, day);
            return easter.AddDays(13);
        }
        private async Task<List<HolidayDay>> InitializeOfficialHolidays(int year)
        {
            var holidays = new List<HolidayDay>();
            var fixedHolidays = new List<HolidayDay>
        {
            new HolidayDay(){Date=new DateTime(year, 1, 1),Name="Нова година" },
             new HolidayDay(){Date=new DateTime(year, 3, 3),Name = "Ден на Освобождението"},
             new HolidayDay(){Date=new DateTime(year, 5, 1), Name = "Ден на труда" },
             new HolidayDay(){Date=new DateTime(year, 5, 6), Name = "Гергьовден" },
            new HolidayDay(){Date =new DateTime(year, 5, 24), Name = "Ден на българската просвета и култура" },
             new HolidayDay(){Date=new DateTime(year, 9, 6), Name = "Ден на Съединението" },
            new HolidayDay(){Date =new DateTime(year, 9, 22), Name = "Ден на Независимостта" },
             new HolidayDay(){Date=new DateTime(year, 12, 24), Name = "Бъдни вечер" },
           new HolidayDay(){Date  =new DateTime(year, 12, 25), Name = "Коледа" },
            new HolidayDay(){Date= new DateTime(year, 12, 26), Name = "Коледа" }
        };

            var easter = CalculateOrthodoxEaster(year);
            var easterHolidays = new List<HolidayDay>
        {
           new HolidayDay(){Date = easter.AddDays(-2),Name="Разпети петък" },
             new HolidayDay(){Date =easter.AddDays(-1),Name="Страстна събота" },
             new HolidayDay(){Date =easter,Name="Великден" },
             new HolidayDay(){Date =easter.AddDays(1),Name="Великден" }
        };

            holidays.AddRange(fixedHolidays);
            holidays.AddRange(easterHolidays);
            for (int i = 0; i < holidays.Count; i++)
            {
                var holiday = holidays[i];
                if (holiday.Date.DayOfWeek == DayOfWeek.Saturday || holiday.Date.DayOfWeek == DayOfWeek.Sunday)
                {
                    DateTime monday = holiday.Date;
                    while (monday.DayOfWeek != DayOfWeek.Monday)
                    {
                        monday = monday.AddDays(1);
                    }

                    if (!holidays.Select(h => h.Date).Contains(monday))
                    {
                        holidays.Add(new HolidayDay() { Date = monday, Name = $"Почивен поради {holiday.Name}" });
                    }
                }
            }
            return holidays;
        }
    }
}
