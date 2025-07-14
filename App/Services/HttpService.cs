using BusinessLayer;
using System.Text;
using System.Text.Json;
namespace App.Services
{
    public class HttpService
    {
        private readonly HttpClient _httpClient;
        public HttpService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }
        public async Task<Dictionary<string,object>> GetAppVersionAsync()
        {
            var response = await _httpClient.GetAsync("App/getappversion");
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                var versionInfo = JsonSerializer.Deserialize<Dictionary<string,object>>(responseContent);
                return versionInfo;
            }
            return null;
        }
        public async Task<User> PostUserLogin(string email, string password)
        {
            var json = JsonSerializer.Serialize(new { email, password });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("User/login", content);
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                User user = JsonSerializer.Deserialize<User>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return response.RequestMessage != null && response.RequestMessage.Content != null
                    ? user
                    : null;
            }
            return null;
        }

        public async Task<List<BusinessTrip>> GetAllBusinessTripsAsync()
        {
            var response = await _httpClient.GetAsync("BusinessTrip/all");
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<BusinessTrip>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return new List<BusinessTrip>();
        }

        public async Task<bool> DeleteBusinessTripAsync(int id)
        {
        
                var response = await _httpClient.DeleteAsync($"BusinessTrip/cancel/{id}");
            User user = App.User;
            if (user.BusinessTrips != null)
            {
                var tripToRemove = user.BusinessTrips.FirstOrDefault(t => t.Id == id);
                if (tripToRemove != null)
                {
                    user.BusinessTrips.Remove(tripToRemove);
                }
            }
            return response.IsSuccessStatusCode;
           
        }
        public async Task<List<HolidayDay>> GetAllHolidayDaysAsync()
        {
            var response = await _httpClient.GetAsync("HolidayDay/all");
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<HolidayDay>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return new List<HolidayDay>();
        }

        public async Task<List<BusinessTrip>> GetUserBusinessTripsAsync(string userId)
        {
            var response = await _httpClient.GetAsync($"BusinessTrip/user/{userId}");
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<BusinessTrip>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return new List<BusinessTrip>();
        }

        public async Task<List<Absence>> GetUserAbsencesAsync(string userId)
        {
            var response = await _httpClient.GetAsync($"Absence/user/{userId}");
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Absence>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return new List<Absence>();
        }

        public async Task<bool> CreateAbsenceAsync(Absence absence)
        {
            var json = JsonSerializer.Serialize(absence);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("Absence/create", content);
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                absence = JsonSerializer.Deserialize<Absence>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                App.User.Absences?.Add(absence);
                App.User.AbsenceDays -= absence.DaysTaken;
                MessagingCenter.Send<HttpService>(this, "AbsenceCreated");
                return true;
            }
            return false;
        }

        public async Task<bool> CreateBusinessTripAsync(BusinessTrip businessTrip)
        {
            var json = JsonSerializer.Serialize(businessTrip);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("BusinessTrip/create", content);
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                List<BusinessTrip> businessTrips = JsonSerializer.Deserialize<List<BusinessTrip>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                App.User.BusinessTrips = businessTrips;
                return true;
            }
            return false;
        }
        public async Task<HolidayDay> CreateHolidayDayAsync(HolidayDay holidayDay)
        {
            var json = JsonSerializer.Serialize(holidayDay);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("HolidayDay/create", content);
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                holidayDay = JsonSerializer.Deserialize<HolidayDay>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return holidayDay;
            }
            return null;
        }


        public async Task<bool> CancelAbsenceAsync(int absenceId)
        {
            var response = await _httpClient.DeleteAsync($"Absence/cancel/{absenceId}");
            return response.IsSuccessStatusCode;
        }

        // Admin methods for user management
        public async Task<List<User>> GetAllUsersAsync()
        {
            var response = await _httpClient.GetAsync("User/all");
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<User>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return new List<User>();
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            var json = JsonSerializer.Serialize(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("User/create", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            var json = JsonSerializer.Serialize(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"User/edit", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var response = await _httpClient.DeleteAsync($"User/delete/{userId}");
            return response.IsSuccessStatusCode;
        }

        // Admin methods for absence management
        public async Task<List<Absence>> GetAllAbsencesAsync()
        {
            var response = await _httpClient.GetAsync("Absence/all");
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Absence>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return new List<Absence>();
        }

        public async Task<bool> ApproveAbsenceAsync(int absenceId)
        {
            var json = JsonSerializer.Serialize(new { id = absenceId, status = BusinessTripStatus.Approved });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"Absence/requestupdate", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RejectAbsenceAsync(int absenceId)
        {
            var json = JsonSerializer.Serialize(new { id = absenceId, status = BusinessTripStatus.Rejected });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"Absence/requestupdate", content);
            return response.IsSuccessStatusCode;
        }


        public async Task<bool> ApproveBusinessTripAsync(int tripId)
        {
            var json = JsonSerializer.Serialize(new { id = tripId, status = BusinessTripStatus.Approved });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"BusinessTrip/requestupdate", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RejectBusinessTripAsync(int tripId)
        {
            var json = JsonSerializer.Serialize(new { id = tripId, status = BusinessTripStatus.Rejected });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"BusinessTrip/requestupdate", content);
            return response.IsSuccessStatusCode;
        }

    }
}
