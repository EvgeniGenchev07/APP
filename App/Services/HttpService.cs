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
    }
}
