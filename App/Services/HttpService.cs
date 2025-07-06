using BusinessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
namespace App.Services
{
    public class HttpService
    {
        private readonly HttpClient _httpClient;
        public HttpService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }
        public async Task<User> PostUserLogin(string email, string password)
        {
            var json = JsonSerializer.Serialize(new {email,password });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("User/login",content);
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
            if(response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                absence =  JsonSerializer.Deserialize<Absence>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                App.User.Absences?.Add(absence);
                App.User.AbsenceDays -= absence.DaysCount;
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
                businessTrip = JsonSerializer.Deserialize<BusinessTrip>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                App.User.BusinessTrips?.Add(businessTrip);
                return true;
            }
            return false;
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
            var response = await _httpClient.PutAsync($"User/update/{user.Id}", content);
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
            var response = await _httpClient.PutAsync($"Absence/approve/{absenceId}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RejectAbsenceAsync(int absenceId)
        {
            var response = await _httpClient.PutAsync($"Absence/reject/{absenceId}", null);
            return response.IsSuccessStatusCode;
        }

        // Admin methods for business trip management
        public async Task<bool> ApproveBusinessTripAsync(int tripId)
        {
            var response = await _httpClient.PutAsync($"BusinessTrip/approve/{tripId}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RejectBusinessTripAsync(int tripId)
        {
            var response = await _httpClient.PutAsync($"BusinessTrip/reject/{tripId}", null);
            return response.IsSuccessStatusCode;
        }
    }
}
